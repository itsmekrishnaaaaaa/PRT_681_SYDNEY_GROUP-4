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
}
