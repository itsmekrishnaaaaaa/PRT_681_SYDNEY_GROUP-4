using OraEarn.Api.Users;

namespace OraEarn.Api.SavingsGoals;

public class SavingsGoal
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public decimal AllocationPercentage { get; set; }
    public DateOnly? TargetDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;

    // Remaining, progress and status are computed, not stored columns, so they can never drift out
    // of step with the saved amount. Saving more than the target caps progress at 100%.
    public decimal RemainingAmount => Math.Max(0, TargetAmount - CurrentAmount);

    public decimal ProgressPercent =>
        TargetAmount <= 0 ? 0 : Math.Min(100, Math.Round(CurrentAmount / TargetAmount * 100, 2));

    public SavingsGoalStatus Status =>
        CurrentAmount <= 0 ? SavingsGoalStatus.Pending :
        CurrentAmount >= TargetAmount && TargetAmount > 0 ? SavingsGoalStatus.Completed :
        SavingsGoalStatus.InProgress;
}