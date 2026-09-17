using OraEarn.Api.Shifts;

namespace OraEarn.Api.Tests.Whitebox;

public class ShiftCalculateTests
{
    [Fact]
    public void Calculate_StandardShift_SubtractsBreakAndComputesEarnings()
    {
        var (hours, earned) = Shift.Calculate(
            new DateOnly(2026, 3, 1),
            new TimeOnly(9, 0),
            new TimeOnly(17, 0),
            breakMinutes: 30,
            hourlyRate: 30m);

        Assert.Equal(7.5m, hours);
        Assert.Equal(225m, earned);
    }

    [Fact]
    public void Calculate_OvernightShift_AddsOneDayToEndTime()
    {
        var (hours, earned) = Shift.Calculate(
            new DateOnly(2026, 3, 1),
            new TimeOnly(22, 0),
            new TimeOnly(6, 0),
            breakMinutes: 0,
            hourlyRate: 25m);

        Assert.Equal(8m, hours);
        Assert.Equal(200m, earned);
    }

    [Fact]
    public void Calculate_ExcessiveBreak_ReturnsZeroHours()
    {
        var (hours, earned) = Shift.Calculate(
            new DateOnly(2026, 3, 1),
            new TimeOnly(9, 0),
            new TimeOnly(10, 0),
            breakMinutes: 120,
            hourlyRate: 20m);

        Assert.Equal(0m, hours);
        Assert.Equal(0m, earned);
    }
}
