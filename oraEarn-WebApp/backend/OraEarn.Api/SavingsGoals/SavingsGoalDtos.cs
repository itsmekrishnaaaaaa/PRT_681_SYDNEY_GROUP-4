namespace OraEarn.Api.SavingsGoals;

public record SavingsGoalRequest(
    string Title,
    decimal TargetAmount,
    decimal CurrentAmount,
    decimal AllocationPercentage,
    DateOnly? TargetDate);

public record SavingsGoalResponse(
    Guid Id,
    string Title,
    decimal TargetAmount,
    decimal CurrentAmount,
    decimal AllocationPercentage,
    DateOnly? TargetDate,
    SavingsGoalStatus Status,
    decimal ProgressPercent,
    decimal RemainingAmount);

public record SavingsGoalContributionRequest(decimal Amount);

public record SavingsGoalAnalyticsItem(
    Guid GoalId,
    string Title,
    SavingsGoalStatus Status,
    decimal TargetAmount,
    decimal CurrentAmount,
    decimal RemainingAmount,
    decimal ProgressPercent,
    decimal AllocationPercentage,
    decimal AvgHourlyRate,
    decimal HoursNeeded,
    decimal AvgHoursPerWeek,
    decimal EstimatedMonthlySavings,
    DateOnly? TargetDate,
    DateOnly? EstimatedFinishDate,
    int? WeeksToComplete);

public record SavingsNotification(
    string Type,
    string Message,
    string Severity);

public record SavingsGoalAnalyticsResponse(
    decimal MonthlyIncomeEstimate,
    decimal NetBalance,
    decimal TotalAllocationPercent,
    IReadOnlyList<SavingsGoalAnalyticsItem> Goals,
    IReadOnlyList<SavingsNotification> Notifications);