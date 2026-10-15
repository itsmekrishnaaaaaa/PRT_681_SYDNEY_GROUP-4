using Microsoft.EntityFrameworkCore;
using OraEarn.Api.Data;
using OraEarn.Api.Analytics;
using OraEarn.Api.VisaCompliance;
using OraEarn.Api.SavingsGoals;
using OraEarn.Api.AiAdvisor;
using OraEarn.Api.Users;
using OraEarn.Api.Jobs;
using OraEarn.Api.Shifts;
using OraEarn.Api.Expenses;

namespace OraEarn.Api.Notifications;

// Notifications are not stored. They are rebuilt from the user's shifts, goals and visa status on every
// request, so they can never go stale; only "this key was read" is persisted (UserNotificationReads).
public class NotificationService(
    ApplicationDbContext db,
    VisaComplianceService visaComplianceService)
{
    /// <summary>Returns the notification bell contents and the unread count for the user.</summary>
    public async Task<NotificationInboxResponse> GetInboxAsync(string userId)
    {
        var items = await BuildNotificationsAsync(userId);
        return new NotificationInboxResponse(items, items.Count(n => !n.IsRead));
    }

    /// <summary>
    /// Builds the current notification list and marks each one read if its key is in the user's read set.
    /// Also used by the analytics dashboard.
    /// </summary>
    public async Task<IReadOnlyList<AppNotification>> BuildNotificationsAsync(string userId)
    {
        var goals = await db.SavingsGoals.AsNoTracking()
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();
        var shifts = await db.Shifts.AsNoTracking().ForUser(userId).ToListAsync();
        var expenses = await db.Expenses.AsNoTracking().ForUser(userId).ToListAsync();

        var totalEarnings = shifts.Sum(s => s.TotalEarned);
        var totalExpenseAmount = expenses.Sum(e => e.Amount);
        var netBalance = totalEarnings - totalExpenseAmount;

        var visa = await visaComplianceService.GetStatusAsync(userId);
        var visaNotes = visaComplianceService.BuildNotifications(visa);
        var raw = BuildAllNotificationItems(visaNotes, goals, shifts, netBalance);

        // Known gap: keys without a goal id (e.g. visa_limit_warning, negative_balance) are fixed strings, so once
        // read they stay read even if the condition clears and later comes back.
        var readKeys = await db.UserNotificationReads.AsNoTracking()
            .Where(r => r.UserId == userId)
            .Select(r => r.NotificationKey)
            .ToHashSetAsync();

        return raw.Select(n => n with { IsRead = readKeys.Contains(n.Id) }).ToList();
    }

    /// <summary>Records one notification key as read; does nothing if it is already recorded.</summary>
    public async Task MarkReadAsync(string userId, string notificationId)
    {
        // Check-then-insert: two simultaneous requests for the same key can still hit the unique
        // (UserId, NotificationKey) index and fail with a database error.
        var exists = await db.UserNotificationReads.AnyAsync(r =>
            r.UserId == userId && r.NotificationKey == notificationId);
        if (exists)
        {
            return;
        }

        db.UserNotificationReads.Add(new UserNotificationRead
        {
            UserId = userId,
            NotificationKey = notificationId,
            ReadAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
    }

    /// <summary>Records every supplied key as read. Keys come from the client and are not checked against the
    /// current list, but rows are only ever written for the calling user.</summary>
    public async Task MarkAllReadAsync(string userId, IReadOnlyList<string> notificationIds)
    {
        var existing = await db.UserNotificationReads.AsNoTracking()
            .Where(r => r.UserId == userId)
            .Select(r => r.NotificationKey)
            .ToHashSetAsync();

        foreach (var id in notificationIds.Where(id => !existing.Contains(id)))
        {
            db.UserNotificationReads.Add(new UserNotificationRead
            {
                UserId = userId,
                NotificationKey = id,
                ReadAt = DateTime.UtcNow
            });
        }

        await db.SaveChangesAsync();
    }

    // These rules overlap with SavingsGoalAnalyticsService (estimates, 4.33 weeks per month, allocation over 100%),
    // and the two copies have drifted slightly (e.g. the average-rate fallback). Change both if a rule changes.
    private static List<AppNotification> BuildAllNotificationItems(
        IReadOnlyList<SavingsNotification> visaNotes,
        List<SavingsGoal> goals,
        List<Shift> shifts,
        decimal netBalance)
    {
        var now = DateTime.UtcNow;
        var notifications = new List<AppNotification>();
        var seenIds = new HashSet<string>();

        // De-duplicate by key so the same alert never appears twice.
        void Add(AppNotification n)
        {
            if (seenIds.Add(n.Id))
            {
                notifications.Add(n);
            }
        }

        foreach (var visaNote in visaNotes)
        {
            Add(new AppNotification(
                visaNote.Type,
                visaNote.Type,
                TitleForType(visaNote.Type),
                visaNote.Message,
                visaNote.Severity,
                "visa",
                now,
                false));
        }

        if (netBalance < 0)
        {
            Add(new AppNotification(
                "negative_balance",
                "negative_balance",
                "Spending ahead of earnings",
                $"Expenses exceed earnings by {Math.Abs(netBalance):C}. Cut spending before saving more.",
                "error",
                "balance",
                now,
                false));
        }

        var totalAlloc = goals.Sum(g => g.AllocationPercentage);
        if (totalAlloc > 100)
        {
            Add(new AppNotification(
                "allocation_overflow",
                "allocation_overflow",
                "Allocation exceeds 100%",
                $"You allocated {totalAlloc:F1}% of income but only 100% is available. Reduce allocations.",
                "error",
                "savings",
                now,
                false));
        }

        var monthlyIncome = EstimateMonthlyIncome(shifts);
        if (monthlyIncome <= 0 && goals.Any(g => g.Status != SavingsGoalStatus.Completed))
        {
            Add(new AppNotification(
                "no_income",
                "no_income",
                "No income recorded",
                "Log shifts so we can estimate hours needed and track savings progress.",
                "warning",
                "work",
                now,
                false));
        }

        var avgHourlyRate = shifts.Count > 0
            ? shifts.Sum(s => s.TotalEarned) / Math.Max(shifts.Sum(s => s.TotalHours), 1)
            : 0m;
        var avgHoursPerWeek = CalculateAvgHoursPerWeek(shifts);

        foreach (var goal in goals.Where(g => g.Status != SavingsGoalStatus.Completed))
        {
            if (goal.CurrentAmount <= 0)
            {
                Add(new AppNotification(
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
                Add(new AppNotification(
                    $"goal_stalled:{goal.Id}",
                    "goal_stalled",
                    "Savings goal stalled",
                    $"\"{goal.Title}\" is only at {goal.ProgressPercent:F0}% — progress has slowed.",
                    "warning",
                    "savings",
                    now,
                    false));
            }

            if (goal.AllocationPercentage <= 0 && goal.RemainingAmount > 0)
            {
                Add(new AppNotification(
                    $"no_allocation:{goal.Id}",
                    "no_allocation",
                    "Goal needs allocation",
                    $"\"{goal.Title}\" has 0% allocation — increase allocation or add savings manually.",
                    "warning",
                    "savings",
                    now,
                    false));
            }

            var remaining = goal.RemainingAmount;
            var hoursNeeded = avgHourlyRate > 0 ? Math.Round(remaining / avgHourlyRate, 1) : 0;
            var monthlySavings = monthlyIncome * (goal.AllocationPercentage / 100m);
            var weeksToComplete = avgHoursPerWeek > 0 && hoursNeeded > 0
                ? (int)Math.Ceiling(hoursNeeded / avgHoursPerWeek)
                : monthlySavings > 0 && remaining > 0
                    ? (int)Math.Ceiling(remaining / monthlySavings * 4.33m)
                    : (int?)null;
            DateOnly? finishDate = weeksToComplete.HasValue
                ? DateOnly.FromDateTime(DateTime.UtcNow.AddDays(weeksToComplete.Value * 7))
                : null;

            if (goal.TargetDate.HasValue && finishDate.HasValue && finishDate > goal.TargetDate)
            {
                Add(new AppNotification(
                    $"behind_schedule:{goal.Id}",
                    "behind_schedule",
                    "Goal behind schedule",
                    $"\"{goal.Title}\" may finish {finishDate:d} but target date is {goal.TargetDate:d}. Work ~{hoursNeeded} more hours.",
                    "warning",
                    "savings",
                    now,
                    false));
            }
            else if (goal.TargetDate.HasValue &&
                     goal.TargetDate.Value < DateOnly.FromDateTime(DateTime.UtcNow.AddDays(30)) &&
                     goal.ProgressPercent < 50)
            {
                Add(new AppNotification(
                    $"goal_behind:{goal.Id}",
                    "goal_behind",
                    "Savings goal behind target",
                    $"\"{goal.Title}\" is {goal.ProgressPercent:F0}% done with target date {goal.TargetDate:d}.",
                    "warning",
                    "savings",
                    now,
                    false));
            }
        }

        // Errors first, then warnings. IsRead is still false for every item here (read state is applied by the
        // caller), so the IsRead ordering has no effect.
        return notifications
            .OrderByDescending(n => n.Severity == "error")
            .ThenByDescending(n => n.Severity == "warning")
            .ThenBy(n => n.IsRead)
            .ToList();
    }

    private static string TitleForType(string type) => type switch
    {
        "visa_limit_exceeded" => "Visa hour limit exceeded",
        "visa_limit_warning" => "Approaching visa work limit",
        "visa_limit_info" => "Visa hours update",
        "negative_balance" => "Spending ahead of earnings",
        "allocation_overflow" => "Allocation exceeds 100%",
        "no_income" => "No income recorded",
        "no_allocation" => "Goal needs allocation",
        "behind_schedule" => "Goal behind schedule",
        "goal_not_started" => "Savings goal not started",
        "goal_stalled" => "Savings goal stalled",
        "goal_behind" => "Savings goal behind target",
        _ => "Notification"
    };

    // Earnings from the last 30 days; falls back to all-time earnings when there are no recent shifts.
    private static decimal EstimateMonthlyIncome(List<Shift> shifts)
    {
        if (shifts.Count == 0) return 0;
        var cutoff = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        var recent = shifts.Where(s => s.WorkDate >= cutoff).ToList();
        if (recent.Count == 0) recent = shifts;
        return recent.Sum(s => s.TotalEarned);
    }

    // Spread over at least one week so a single recent shift does not inflate the weekly average.
    private static decimal CalculateAvgHoursPerWeek(List<Shift> shifts)
    {
        if (shifts.Count == 0) return 0;
        var oldest = shifts.Min(s => s.WorkDate);
        var newest = shifts.Max(s => s.WorkDate);
        var days = Math.Max((newest.ToDateTime(TimeOnly.MinValue) - oldest.ToDateTime(TimeOnly.MinValue)).Days, 7);
        return shifts.Sum(s => s.TotalHours) / (days / 7m);
    }
}