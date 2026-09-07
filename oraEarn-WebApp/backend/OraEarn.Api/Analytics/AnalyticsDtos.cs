using OraEarn.Api.Notifications;
using OraEarn.Api.SavingsGoals;
using OraEarn.Api.VisaCompliance;

namespace OraEarn.Api.Analytics;

public record WeekdayAnalyticsItem(
    string DayLabel,
    string Date,
    decimal ShiftHours,
    int ShiftCount,
    decimal ExpenseAmount,
    int ExpenseCount);

public record UserAnalyticsResponse(
    int JobCount,
    int ShiftCount,
    decimal TotalHours,
    decimal TotalEarnings,
    int ExpenseCount,
    decimal TotalExpenses,
    decimal NetBalance,
    int SavingsGoalCount,
    decimal TotalSavingsTarget,
    decimal TotalAllocationPercent,
    IReadOnlyDictionary<string, decimal> ExpensesByCategory,
    IReadOnlyDictionary<string, int> ExpensesByKind,
    VisaComplianceResponse? VisaCompliance,
    IReadOnlyList<AppNotification> Notifications,
    IReadOnlyList<WeekdayAnalyticsItem> TimelineBreakdown,
    string Period,
    string PeriodLabel,
    string DateFrom,
    string DateTo,
    int UnreadNotificationCount);

public record AdminUserStatsResponse(
    string Id,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyList<string> Roles,
    DateTime CreatedAt,
    int JobCount,
    int ShiftCount,
    decimal TotalEarnings,
    int ExpenseCount,
    decimal TotalExpenses,
    int SavingsGoalCount);

public record AdminAnalyticsResponse(
    int TotalUsers,
    int TotalAdmins,
    int TotalJobs,
    int TotalShifts,
    decimal TotalHours,
    decimal TotalEarnings,
    int TotalExpenses,
    decimal TotalExpenseAmount,
    int TotalSavingsGoals,
    IReadOnlyList<AdminUserStatsResponse> Users);
