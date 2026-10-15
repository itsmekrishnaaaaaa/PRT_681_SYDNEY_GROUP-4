using OraEarn.Api.VisaCompliance;
using OraEarn.Api.Notifications;
using OraEarn.Api.SavingsGoals;
using OraEarn.Api.AiAdvisor;
using OraEarn.Api.Users;
using OraEarn.Api.Jobs;
using OraEarn.Api.Shifts;
using OraEarn.Api.Expenses;

namespace OraEarn.Api.Analytics;

// Pure functions with no database access, so the period and chart rules can be unit tested directly
// (see UserAnalyticsBuilderTests).
public static class UserAnalyticsBuilder
{
    /// <summary>
    /// Returns the inclusive date range for a period: today and the 6 or 29 days before it, or the
    /// current month plus the 11 before it, so the yearly chart always shows 12 whole month buckets.
    /// </summary>
    public static (DateOnly From, DateOnly To, string Label) GetPeriodRange(AnalyticsPeriod period)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        return period switch
        {
            AnalyticsPeriod.OneMonth => (today.AddDays(-29), today, "Last 30 days"),
            AnalyticsPeriod.TwelveMonths => (new DateOnly(today.Year, today.Month, 1).AddMonths(-11), today, "Last 12 months"),
            _ => (today.AddDays(-6), today, "Last 7 days"),
        };
    }

    /// <summary>Maps the ?period= query value to a period; anything unrecognised means 7 days.</summary>
    public static AnalyticsPeriod ParsePeriod(string? value) =>
        value?.ToLowerInvariant() switch
        {
            "30d" or "1m" or "month" => AnalyticsPeriod.OneMonth,
            "12m" or "year" => AnalyticsPeriod.TwelveMonths,
            _ => AnalyticsPeriod.SevenDays,
        };

    /// <summary>Builds chart buckets: one per day for 7 and 30 days, one per month for 12 months, including empty buckets.</summary>
    public static IReadOnlyList<WeekdayAnalyticsItem> BuildTimelineBreakdown(
        AnalyticsPeriod period,
        List<Shift> shifts,
        List<Expense> expenses)
    {
        var (from, to, _) = GetPeriodRange(period);

        return period switch
        {
            AnalyticsPeriod.TwelveMonths => BuildMonthlyBreakdown(from, to, shifts, expenses),
            AnalyticsPeriod.OneMonth => BuildDailyBreakdown(from, to, shifts, expenses, compactLabels: true),
            _ => BuildDailyBreakdown(from, to, shifts, expenses, compactLabels: false),
        };
    }

    /// <summary>
    /// Derives notifications from visa status, net balance and goal progress. Currently not called:
    /// the bell and dashboard use NotificationService, which has its own, fuller version of these rules.
    /// </summary>
    public static List<AppNotification> BuildNotificationItems(
        VisaComplianceResponse? visa,
        List<SavingsGoal> goals,
        decimal netBalance)
    {
        var now = DateTime.UtcNow;
        var notifications = new List<AppNotification>();

        if (visa?.Applies == true)
        {
            if (visa.IsOverLimit)
            {
                var over = Math.Round(visa.FortnightHoursWorked - visa.FortnightHourLimit, 1);
                notifications.Add(new AppNotification(
                    "visa_overwork",
                    "visa_overwork",
                    "Visa hour limit exceeded",
                    $"You worked {visa.FortnightHoursWorked:F1} hrs in 14 days — {over} hrs over the student limit. Reduce upcoming shifts.",
                    "error",
                    "visa",
                    now,
                    false));
            }
            else if (visa.IsNearLimit)
            {
                notifications.Add(new AppNotification(
                    "visa_near_limit",
                    "visa_near_limit",
                    "Approaching visa work limit",
                    $"Only {visa.RemainingHours:F1} hrs left this fortnight before you hit the 48 hr cap.",
                    "warning",
                    "visa",
                    now,
                    false));
            }
        }

        if (netBalance < 0)
        {
            notifications.Add(new AppNotification(
                "negative_balance",
                "negative_balance",
                "Spending ahead of earnings",
                $"Expenses exceed earnings by {Math.Abs(netBalance):C}. Review spending in analytics.",
                "warning",
                "balance",
                now,
                false));
        }

        foreach (var goal in goals)
        {
            if (goal.Status == SavingsGoalStatus.Completed)
            {
                continue;
            }

            if (goal.CurrentAmount <= 0)
            {
                notifications.Add(new AppNotification(
                    // Goal notifications embed the goal id in the key so read state is tracked per goal.
                    $"goal_not_started:{goal.Id}",
                    "goal_not_started",
                    "Savings goal not started",
                    $"\"{goal.Title}\" has no contributions yet — add savings to get started.",
                    "warning",
                    "savings",
                    now,
                    false));
            }
            else if (goal.ProgressPercent < 15 && (DateTime.UtcNow - goal.CreatedAt).TotalDays >= 7)
            {
                notifications.Add(new AppNotification(
                    $"goal_stalled:{goal.Id}",
                    "goal_stalled",
                    "Savings goal stalled",
                    $"\"{goal.Title}\" is only at {goal.ProgressPercent:F0}% — progress has slowed.",
                    "warning",
                    "savings",
                    now,
                    false));
            }
            else if (goal.TargetDate.HasValue &&
                     goal.TargetDate.Value < DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)) &&
                     goal.ProgressPercent < 50)
            {
                notifications.Add(new AppNotification(
                    $"goal_behind:{goal.Id}",
                    "goal_behind",
                    "Savings goal behind schedule",
                    $"\"{goal.Title}\" is {goal.ProgressPercent:F0}% done with target date {goal.TargetDate:d}.",
                    "warning",
                    "savings",
                    now,
                    false));
            }
        }

        return notifications;
    }

    /// <summary>The same notifications in the simpler type/message/severity shape. Currently not called.</summary>
    public static List<SavingsNotification> BuildNotifications(
        VisaComplianceResponse? visa,
        List<SavingsGoal> goals,
        decimal netBalance) =>
        BuildNotificationItems(visa, goals, netBalance)
            .Select(n => new SavingsNotification(n.Type, n.Message, n.Severity))
            .ToList();

    private static IReadOnlyList<WeekdayAnalyticsItem> BuildDailyBreakdown(
        DateOnly from,
        DateOnly to,
        List<Shift> shifts,
        List<Expense> expenses,
        bool compactLabels)
    {
        var days = new List<DateOnly>();
        for (var d = from; d <= to; d = d.AddDays(1))
        {
            days.Add(d);
        }

        return days.Select(date =>
        {
            var dayShifts = shifts.Where(s => s.WorkDate == date).ToList();
            var dayExpenses = expenses.Where(e => e.Date == date).ToList();
            var label = compactLabels
                ? date.Day.ToString()
                : date.ToString("ddd");
            return new WeekdayAnalyticsItem(
                label,
                date.ToString("yyyy-MM-dd"),
                Math.Round(dayShifts.Sum(s => s.TotalHours), 1),
                dayShifts.Count,
                Math.Round(dayExpenses.Sum(e => e.Amount), 2),
                dayExpenses.Count);
        }).ToList();
    }

    private static IReadOnlyList<WeekdayAnalyticsItem> BuildMonthlyBreakdown(
        DateOnly from,
        DateOnly to,
        List<Shift> shifts,
        List<Expense> expenses)
    {
        var months = new List<(DateOnly Start, DateOnly End, string Label)>();
        var cursor = new DateOnly(from.Year, from.Month, 1);
        while (cursor <= to)
        {
            var monthEnd = cursor.AddMonths(1).AddDays(-1);
            var end = monthEnd > to ? to : monthEnd;
            months.Add((cursor, end, cursor.ToString("MMM")));
            cursor = cursor.AddMonths(1);
        }

        return months.Select(m =>
        {
            var monthShifts = shifts.Where(s => s.WorkDate >= m.Start && s.WorkDate <= m.End).ToList();
            var monthExpenses = expenses.Where(e => e.Date >= m.Start && e.Date <= m.End).ToList();
            return new WeekdayAnalyticsItem(
                m.Label,
                m.Start.ToString("yyyy-MM"),
                Math.Round(monthShifts.Sum(s => s.TotalHours), 1),
                monthShifts.Count,
                Math.Round(monthExpenses.Sum(e => e.Amount), 2),
                monthExpenses.Count);
        }).ToList();
    }
}