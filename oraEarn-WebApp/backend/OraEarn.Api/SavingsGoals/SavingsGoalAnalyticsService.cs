using Microsoft.EntityFrameworkCore;
using OraEarn.Api.Analytics;
using OraEarn.Api.Data;
using OraEarn.Api.Shifts;
using OraEarn.Api.VisaCompliance;

namespace OraEarn.Api.SavingsGoals;

// Turns goals into "how many hours of work until I reach this" (FR-6.4, FR-6.5). All figures are
// estimates from the user's own shift history; nothing here is stored.
public class SavingsGoalAnalyticsService(
    ApplicationDbContext db,
    AnalyticsService analyticsService,
    VisaComplianceService visaComplianceService)
{
    /// <summary>
    /// Returns each goal with hours needed, average rate and an estimated finish date, plus savings and
    /// visa alerts for the goals page. Also the context source for the Aura chat.
    /// </summary>
    public async Task<SavingsGoalAnalyticsResponse> GetAnalyticsAsync(string userId)
    {
        var goals = await db.SavingsGoals.AsNoTracking()
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        var userAnalytics = await analyticsService.GetUserAnalyticsAsync(userId);
        var netBalance = userAnalytics.NetBalance;

        var shifts = await db.Shifts.AsNoTracking()
            .ForUser(userId)
            .ToListAsync();

        var monthlyIncome = EstimateMonthlyIncome(shifts);

        // Prefer the rate actually earned (pay / hours, so breaks and rate changes are reflected); with
        // no shifts yet, fall back to the average job rate. Note: AverageAsync on a user with no jobs
        // either is likely to throw rather than return 0 - not covered by tests.
        var avgHourlyRate = shifts.Count > 0
            ? shifts.Sum(s => s.TotalEarned) / Math.Max(shifts.Sum(s => s.TotalHours), 1)
            : await db.Jobs.Where(j => j.UserId == userId).Select(j => j.HourlyRate).AverageAsync() is decimal rate ? rate : 0;

        var avgHoursPerWeek = CalculateAvgHoursPerWeek(shifts);

        var items = goals.Select(g =>
        {
            var remaining = g.RemainingAmount;
            var hoursNeeded = avgHourlyRate > 0 ? Math.Round(remaining / avgHourlyRate, 1) : 0;
            var monthlySavings = monthlyIncome * (g.AllocationPercentage / 100m);
            // Two estimates: by the user's usual weekly hours if they have shift history, otherwise by
            // the allocated share of monthly income (4.33 = average weeks per month). Hours needed treats
            // every dollar earned as saved, so it is an optimistic lower bound.
            var weeksToComplete = avgHoursPerWeek > 0 && hoursNeeded > 0
                ? (int)Math.Ceiling(hoursNeeded / avgHoursPerWeek)
                : monthlySavings > 0 && remaining > 0
                    ? (int)Math.Ceiling(remaining / monthlySavings * 4.33m)
                    : (int?)null;

            DateOnly? finishDate = weeksToComplete.HasValue
                ? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(weeksToComplete.Value * 7))
                : null;

            return new SavingsGoalAnalyticsItem(
                g.Id,
                g.Title,
                g.Status,
                g.TargetAmount,
                g.CurrentAmount,
                remaining,
                g.ProgressPercent,
                g.AllocationPercentage,
                Math.Round(avgHourlyRate, 2),
                hoursNeeded,
                Math.Round(avgHoursPerWeek, 1),
                Math.Round(monthlySavings, 2),
                g.TargetDate,
                finishDate,
                weeksToComplete);
        }).ToList();

        // Visa alerts go first because they are legal limits, ahead of budgeting advice.
        var visaStatus = await visaComplianceService.GetStatusAsync(userId);
        var notifications = BuildNotifications(goals, userAnalytics, monthlyIncome, items);
        notifications.InsertRange(0, visaComplianceService.BuildNotifications(visaStatus));

        return new SavingsGoalAnalyticsResponse(
            Math.Round(monthlyIncome, 2),
            netBalance,
            goals.Sum(g => g.AllocationPercentage),
            items,
            notifications);
    }

    // Earnings in the last 30 days. If there are none, all-time earnings are used instead, which
    // overstates "monthly" income for users with a long history but keeps estimates non-zero.
    private static decimal EstimateMonthlyIncome(List<Shift> shifts)
    {
        if (shifts.Count == 0) return 0;

        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var recent = shifts.Where(s => s.WorkDate >= cutoff).ToList();
        if (recent.Count == 0)
        {
            recent = shifts;
        }

        return recent.Sum(s => s.TotalEarned);
    }

    // The span is at least one week so a single recent shift does not look like a huge weekly rate.
    private static decimal CalculateAvgHoursPerWeek(List<Shift> shifts)
    {
        if (shifts.Count == 0) return 0;

        var oldest = shifts.Min(s => s.WorkDate);
        var newest = shifts.Max(s => s.WorkDate);
        var days = Math.Max((newest.ToDateTime(TimeOnly.MinValue) - oldest.ToDateTime(TimeOnly.MinValue)).Days, 7);
        var weeks = days / 7m;
        return shifts.Sum(s => s.TotalHours) / weeks;
    }

    private static List<SavingsNotification> BuildNotifications(
        List<SavingsGoal> goals,
        UserAnalyticsResponse analytics,
        decimal monthlyIncome,
        List<SavingsGoalAnalyticsItem> items)
    {
        var notifications = new List<SavingsNotification>();

        var totalAlloc = goals.Sum(g => g.AllocationPercentage);
        if (totalAlloc > 100)
        {
            notifications.Add(new SavingsNotification(
                "allocation_overflow",
                $"You allocated {totalAlloc:F1}% of income but only 100% is available. Reduce allocations.",
                "error"));
        }

        if (analytics.NetBalance < 0)
        {
            notifications.Add(new SavingsNotification(
                "negative_balance",
                $"Not enough money: expenses exceed earnings by {Math.Abs(analytics.NetBalance):C}. Cut spending before saving more.",
                "error"));
        }

        if (monthlyIncome <= 0)
        {
            notifications.Add(new SavingsNotification(
                "no_income",
                "No shift earnings recorded yet. Log shifts so we can estimate hours needed for your goals.",
                "warning"));
        }

        foreach (var item in items.Where(i => i.Status != SavingsGoalStatus.Completed))
        {
            if (item.EstimatedMonthlySavings <= 0 && item.RemainingAmount > 0)
            {
                notifications.Add(new SavingsNotification(
                    "no_allocation",
                    $"Goal \"{item.Title}\" has 0% allocation — increase allocation or add savings manually.",
                    "warning"));
            }

            if (item.TargetDate.HasValue && item.EstimatedFinishDate.HasValue &&
                item.EstimatedFinishDate > item.TargetDate)
            {
                notifications.Add(new SavingsNotification(
                    "behind_schedule",
                    $"Goal \"{item.Title}\" may finish {item.EstimatedFinishDate:d} but target date is {item.TargetDate:d}. Work ~{item.HoursNeeded} more hours.",
                    "warning"));
            }

            if (item.HoursNeeded > 0)
            {
                notifications.Add(new SavingsNotification(
                    "hours_needed",
                    $"Goal \"{item.Title}\": {item.RemainingAmount:C} remaining — about {item.HoursNeeded} hours at ${item.AvgHourlyRate}/hr.",
                    "info"));
            }
        }

        return notifications;
    }
}
