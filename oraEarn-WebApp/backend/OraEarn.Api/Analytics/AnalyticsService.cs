using Microsoft.EntityFrameworkCore;
using OraEarn.Api.Data;
using OraEarn.Api.Notifications;
using OraEarn.Api.SavingsGoals;
using OraEarn.Api.Users;
using OraEarn.Api.VisaCompliance;

namespace OraEarn.Api.Analytics;

public class AnalyticsService(
    ApplicationDbContext db,
    VisaComplianceService visaComplianceService,
    NotificationService notificationService)
{
    public async Task<UserAnalyticsResponse> GetUserAnalyticsAsync(string userId, AnalyticsPeriod period = AnalyticsPeriod.SevenDays)
    {
        var (from, to, periodLabel) = UserAnalyticsBuilder.GetPeriodRange(period);

        var jobs = await db.Jobs.CountAsync(j => j.UserId == userId);
        var allShifts = await db.Shifts.ForUser(userId).ToListAsync();
        var allExpenses = await db.Expenses.Include(e => e.Category).ForUser(userId).ToListAsync();
        var goals = await db.SavingsGoals.Where(g => g.UserId == userId).ToListAsync();

        var periodShifts = allShifts.Where(s => s.WorkDate >= from && s.WorkDate <= to).ToList();
        var periodExpenses = allExpenses.Where(e => e.Date >= from && e.Date <= to).ToList();

        var totalEarnings = periodShifts.Sum(s => s.TotalEarned);
        var totalExpenses = periodExpenses.Sum(e => e.Amount);
        var netBalance = totalEarnings - totalExpenses;

        var byCategory = periodExpenses
            .GroupBy(e => e.Category.Title)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));
        var byKind = periodExpenses
            .GroupBy(e => e.ExpenseKind.ToString())
            .ToDictionary(g => g.Key, g => g.Count());

        var visaCompliance = await visaComplianceService.GetStatusAsync(userId);
        var timelineBreakdown = UserAnalyticsBuilder.BuildTimelineBreakdown(period, allShifts, allExpenses);
        var notifications = await notificationService.BuildNotificationsAsync(userId);

        return new UserAnalyticsResponse(
            jobs,
            periodShifts.Count,
            periodShifts.Sum(s => s.TotalHours),
            totalEarnings,
            periodExpenses.Count,
            totalExpenses,
            netBalance,
            goals.Count,
            goals.Sum(g => g.TargetAmount),
            goals.Sum(g => g.AllocationPercentage),
            byCategory,
            byKind,
            visaCompliance.Applies ? visaCompliance : null,
            notifications,
            timelineBreakdown,
            period.ToString(),
            periodLabel,
            from.ToString("yyyy-MM-dd"),
            to.ToString("yyyy-MM-dd"),
            notifications.Count(n => !n.IsRead));
    }

    public async Task<AdminAnalyticsResponse> GetPlatformAnalyticsAsync()
    {
        var users = await db.Users.OrderBy(u => u.Email).ToListAsync();
        var userStats = new List<AdminUserStatsResponse>();

        var totalJobs = 0;
        var totalShifts = 0;
        var totalHours = 0m;
        var totalEarnings = 0m;
        var totalExpenseCount = 0;
        var totalExpenseAmount = 0m;
        var totalGoals = 0;
        var adminCount = 0;

        foreach (var user in users)
        {
            var roles = await db.UserRoles
                .Where(ur => ur.UserId == user.Id)
                .Join(db.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name!)
                .ToListAsync();

            if (roles.Contains("Admin")) adminCount++;

            var jobs = await db.Jobs.CountAsync(j => j.UserId == user.Id);
            var shifts = await db.Shifts.ForUser(user.Id).ToListAsync();
            var expenses = await db.Expenses.ForUser(user.Id).ToListAsync();
            var goals = await db.SavingsGoals.CountAsync(g => g.UserId == user.Id);

            var earnings = shifts.Sum(s => s.TotalEarned);
            var expenseTotal = expenses.Sum(e => e.Amount);

            totalJobs += jobs;
            totalShifts += shifts.Count;
            totalHours += shifts.Sum(s => s.TotalHours);
            totalEarnings += earnings;
            totalExpenseCount += expenses.Count;
            totalExpenseAmount += expenseTotal;
            totalGoals += goals;

            userStats.Add(new AdminUserStatsResponse(
                user.Id,
                user.Email ?? string.Empty,
                user.FirstName,
                user.LastName,
                roles,
                user.CreatedAt,
                jobs,
                shifts.Count,
                earnings,
                expenses.Count,
                expenseTotal,
                goals));
        }

        return new AdminAnalyticsResponse(
            users.Count,
            adminCount,
            totalJobs,
            totalShifts,
            totalHours,
            totalEarnings,
            totalExpenseCount,
            totalExpenseAmount,
            totalGoals,
            userStats);
    }
}
