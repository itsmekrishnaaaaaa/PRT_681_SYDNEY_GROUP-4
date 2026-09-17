using OraEarn.Api.Analytics;
using OraEarn.Api.Expenses;
using OraEarn.Api.Shifts;

namespace OraEarn.Api.Tests.Whitebox;

public class UserAnalyticsBuilderTests
{
    [Theory]
    [InlineData("7d", AnalyticsPeriod.SevenDays)]
    [InlineData("30d", AnalyticsPeriod.OneMonth)]
    [InlineData("1m", AnalyticsPeriod.OneMonth)]
    [InlineData("12m", AnalyticsPeriod.TwelveMonths)]
    [InlineData(null, AnalyticsPeriod.SevenDays)]
    public void ParsePeriod_MapsQueryValues(string? input, AnalyticsPeriod expected)
    {
        Assert.Equal(expected, UserAnalyticsBuilder.ParsePeriod(input));
    }

    [Fact]
    public void GetPeriodRange_SevenDays_SpansSevenDayWindow()
    {
        var (from, to, label) = UserAnalyticsBuilder.GetPeriodRange(AnalyticsPeriod.SevenDays);

        Assert.Equal("Last 7 days", label);
        Assert.Equal(6, to.DayNumber - from.DayNumber);
    }

    [Fact]
    public void BuildTimelineBreakdown_WithNoData_ReturnsEmptySeries()
    {
        var timeline = UserAnalyticsBuilder.BuildTimelineBreakdown(
            AnalyticsPeriod.SevenDays,
            shifts: [],
            expenses: []);

        Assert.NotEmpty(timeline);
        Assert.All(timeline, item =>
        {
            Assert.Equal(0, item.ShiftCount);
            Assert.Equal(0m, item.ExpenseAmount);
        });
    }
}
