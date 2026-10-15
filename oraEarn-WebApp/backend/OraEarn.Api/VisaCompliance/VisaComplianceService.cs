using Microsoft.EntityFrameworkCore;
using OraEarn.Api.Data;
using OraEarn.Api.Analytics;
using OraEarn.Api.SavingsGoals;
using OraEarn.Api.AiAdvisor;
using OraEarn.Api.Users;
using OraEarn.Api.Jobs;
using OraEarn.Api.Shifts;
using OraEarn.Api.Expenses;
using OraEarn.Api.Notifications;

namespace OraEarn.Api.VisaCompliance;

// Enforces requirement NFR-7.1: an Australian student visa holder may work at most 48 hours per
// fortnight during study periods (the Home Affairs work condition the user-facing messages refer to).
//
// Known simplifications, kept deliberately small for this project and worth checking against the
// current Home Affairs wording before relying on it:
// - The "fortnight" is a rolling 14-day window ending today, not a fixed fortnight calendar.
// - There is no study-period vs. scheduled-break distinction; the limit applies all year.
// - "Today" is the UTC date, so for Australian users the window moves at 10-11am local time.
public class VisaComplianceService(ApplicationDbContext db)
{
    public const decimal StudentFortnightHourLimit = 48m;

    // Warn from 85% of the limit (40.8 h, leaving 7.2 h). The ratio is an app choice, not part of
    // the visa rule; the code does not record why 85% was picked.
    private const decimal NearLimitRatio = 0.85m;

    /// <summary>
    /// Returns the user's hours in the rolling 14-day window and whether they are near or over the
    /// 48-hour student limit. Non-student users get <c>Applies = false</c>.
    /// </summary>
    public async Task<VisaComplianceResponse> GetStatusAsync(string userId)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == userId)
            ?? throw new InvalidOperationException("User not found.");

        if (!IsStudentLimited(user))
        {
            return new VisaComplianceResponse(
                Applies: false,
                user.VisaStatus,
                user.WorkingRights,
                0,
                0,
                0,
                false,
                false,
                DateOnly.FromDateTime(DateTime.UtcNow),
                DateOnly.FromDateTime(DateTime.UtcNow));
        }

        // -13, not -14: the window is inclusive at both ends, so today plus the 13 days before it is
        // exactly 14 calendar days. A shift dated 14 days ago has already dropped out.
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var fortnightStart = today.AddDays(-13);
        var hours = await SumFortnightHoursAsync(userId, fortnightStart, today);

        // Exactly 48 hours is allowed; only going above it counts as over the limit.
        var remaining = Math.Max(0, StudentFortnightHourLimit - hours);
        var isOver = hours > StudentFortnightHourLimit;
        var isNear = !isOver && hours >= StudentFortnightHourLimit * NearLimitRatio;

        return new VisaComplianceResponse(
            Applies: true,
            user.VisaStatus,
            user.WorkingRights,
            Math.Round(hours, 2),
            StudentFortnightHourLimit,
            Math.Round(remaining, 2),
            isOver,
            isNear,
            fortnightStart,
            today);
    }

    /// <summary>
    /// Throws <see cref="InvalidOperationException"/> if saving a shift of <paramref name="shiftHours"/>
    /// would take a student over 48 hours in the current window. Pass <paramref name="excludeShiftId"/>
    /// when editing so the shift's previous hours are not counted twice.
    /// </summary>
    public async Task ValidateShiftAsync(
        string userId,
        decimal shiftHours,
        DateOnly workDate,
        Guid? excludeShiftId = null)
    {
        var user = await db.Users.AsNoTracking().SingleOrDefaultAsync(u => u.Id == userId);
        if (user is null || !IsStudentLimited(user))
        {
            return;
        }

        // Only shifts inside the current window are checked. A back-dated shift older than 14 days
        // cannot change today's total, but note that future-dated shifts are also skipped: the rule
        // is enforced against hours already worked, not against a planned roster.
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var fortnightStart = today.AddDays(-13);
        if (workDate < fortnightStart || workDate > today)
        {
            return;
        }

        var currentHours = await SumFortnightHoursAsync(userId, fortnightStart, today, excludeShiftId);
        var projected = currentHours + shiftHours;

        if (projected > StudentFortnightHourLimit)
        {
            var overBy = Math.Round(projected - StudentFortnightHourLimit, 1);
            throw new InvalidOperationException(
                $"Student visa work limit exceeded: this shift would bring you to {projected:F1} hours in the last 14 days " +
                $"(limit {StudentFortnightHourLimit:F0} hrs/fortnight during study periods). " +
                $"Remove {overBy:F1} hours or wait until older shifts fall outside the fortnight window.");
        }
    }

    /// <summary>
    /// Turns a compliance status into at most one bell notification: error when over the limit,
    /// warning from 85%, and an info nudge once 8 or fewer hours remain.
    /// </summary>
    public List<SavingsNotification> BuildNotifications(VisaComplianceResponse status)
    {
        if (!status.Applies)
        {
            return [];
        }

        var notifications = new List<SavingsNotification>();

        if (status.IsOverLimit)
        {
            var overBy = Math.Round(status.FortnightHoursWorked - status.FortnightHourLimit, 1);
            notifications.Add(new SavingsNotification(
                "visa_limit_exceeded",
                $"Student visa hour limit hit: {status.FortnightHoursWorked:F1} hrs worked in the last 14 days " +
                $"(limit {status.FortnightHourLimit:F0} hrs/fortnight). You are {overBy:F1} hrs over — reduce upcoming shifts or contact your employer.",
                "error"));
        }
        else if (status.IsNearLimit)
        {
            notifications.Add(new SavingsNotification(
                "visa_limit_warning",
                $"Approaching student visa limit: {status.FortnightHoursWorked:F1}/{status.FortnightHourLimit:F0} hrs this fortnight. " +
                $"Only {status.RemainingHours:F1} hrs remaining before you must stop work (Home Affairs study-period rule).",
                "warning"));
        }
        else if (status.RemainingHours <= 8)
        {
            notifications.Add(new SavingsNotification(
                "visa_limit_info",
                $"Student visa: {status.FortnightHoursWorked:F1}/{status.FortnightHourLimit:F0} hrs this fortnight — {status.RemainingHours:F1} hrs left.",
                "info"));
        }

        return notifications;
    }

    private async Task<decimal> SumFortnightHoursAsync(
        string userId,
        DateOnly from,
        DateOnly to,
        Guid? excludeShiftId = null)
    {
        var query = db.Shifts.AsNoTracking()
            .ForUser(userId)
            .Where(s => s.WorkDate >= from && s.WorkDate <= to);

        if (excludeShiftId.HasValue)
        {
            query = query.Where(s => s.Id != excludeShiftId.Value);
        }

        return await query.SumAsync(s => s.TotalHours);
    }

    // Either field can mark a student because both are free text captured at registration; matching
    // on both avoids missing a student whose visa status and work-rights answers differ.
    private static bool IsStudentLimited(ApplicationUser user) =>
        user.WorkingRights.Equals("Student", StringComparison.OrdinalIgnoreCase)
        || user.VisaStatus.Equals("Student", StringComparison.OrdinalIgnoreCase);
}