using OraEarn.Api.SavingsGoals;

namespace OraEarn.Api.Tests.Whitebox;

public class SavingsGoalLogicTests
{
    [Fact]
    public void ProgressPercent_CapsAtOneHundred()
    {
        var goal = new SavingsGoal { TargetAmount = 100m, CurrentAmount = 150m };

        Assert.Equal(100m, goal.ProgressPercent);
        Assert.Equal(SavingsGoalStatus.Completed, goal.Status);
    }

    [Fact]
    public void RemainingAmount_NeverGoesNegative()
    {
        var goal = new SavingsGoal { TargetAmount = 50m, CurrentAmount = 80m };

        Assert.Equal(0m, goal.RemainingAmount);
    }

    [Fact]
    public void Status_PendingWhenNoContributions()
    {
        var goal = new SavingsGoal { TargetAmount = 500m, CurrentAmount = 0m };

        Assert.Equal(SavingsGoalStatus.Pending, goal.Status);
        Assert.Equal(0m, goal.ProgressPercent);
    }

    [Fact]
    public void Allocate_UsesLeftoverPayTimesPercent()
    {
        var saved = SavingsGoalAllocator.Allocate(1000m, 200m, 20m, 5000m, 20m);

        Assert.Equal(160m, saved);
    }

    [Fact]
    public void Allocate_DropsWhenExpensesRise()
    {
        var afterShift = SavingsGoalAllocator.Allocate(1000m, 0m, 20m, 5000m, 20m);
        var afterExpense = SavingsGoalAllocator.Allocate(1000m, 400m, 20m, 5000m, 20m);

        Assert.Equal(200m, afterShift);
        Assert.Equal(120m, afterExpense);
    }

    [Fact]
    public void Allocate_ScalesWhenPercentsPassOneHundred()
    {
        var first = SavingsGoalAllocator.Allocate(100m, 0m, 60m, 1000m, 120m);
        var second = SavingsGoalAllocator.Allocate(100m, 0m, 60m, 1000m, 120m);

        Assert.Equal(50m, first);
        Assert.Equal(50m, second);
    }

    [Fact]
    public void Apply_UpdatesGoalCurrentAmount()
    {
        var goal = new SavingsGoal { TargetAmount = 400m, AllocationPercentage = 25m, ManualAmount = 10m };
        SavingsGoalAllocator.Apply([goal], 200m, 40m);

        Assert.Equal(50m, goal.CurrentAmount);
    }
}
