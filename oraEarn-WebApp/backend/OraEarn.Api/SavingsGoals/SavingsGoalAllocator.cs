namespace OraEarn.Api.SavingsGoals;

public static class SavingsGoalAllocator
{
    public static decimal NetAvailable(decimal earnings, decimal expenses) =>
        Math.Max(0, earnings - expenses);

    public static decimal Allocate(
        decimal earnings,
        decimal expenses,
        decimal allocationPercentage,
        decimal targetAmount,
        decimal totalAllocationPercent,
        decimal extraAmount = 0)
    {
        var available = NetAvailable(earnings, expenses);
        var scale = totalAllocationPercent > 100 ? 100m / totalAllocationPercent : 1m;
        var auto = Math.Round(available * (Math.Max(0, allocationPercentage) / 100m) * scale, 2);
        var total = auto + Math.Max(0, extraAmount);
        if (targetAmount > 0)
        {
            total = Math.Min(total, targetAmount);
        }

        return Math.Max(0, total);
    }

    public static void Apply(IReadOnlyList<SavingsGoal> goals, decimal earnings, decimal expenses)
    {
        var totalAllocation = goals.Sum(g => g.AllocationPercentage);
        foreach (var goal in goals)
        {
            goal.CurrentAmount = Allocate(
                earnings,
                expenses,
                goal.AllocationPercentage,
                goal.TargetAmount,
                totalAllocation,
                goal.ManualAmount);
        }
    }
}
