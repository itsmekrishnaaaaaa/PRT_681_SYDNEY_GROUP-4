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

public class VisaComplianceService(ApplicationDbContext db)
{
    // Student-visa fortnight cap during study. It is a rule here, not a column on the user.
    public const decimal StudentFortnightHourLimit = 48m;
    private const decimal NearLimitRatio = 0.85m;

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

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var fortnightStart = today.AddDays(-13);
        var hours = await SumFortnightHoursAsync(userId, fortnightStart, today);

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

    private static bool IsStudentLimited(ApplicationUser user) =>
        user.WorkingRights.Equals("Student", StringComparison.OrdinalIgnoreCase)
        || user.VisaStatus.Equals("Student", StringComparison.OrdinalIgnoreCase);
}