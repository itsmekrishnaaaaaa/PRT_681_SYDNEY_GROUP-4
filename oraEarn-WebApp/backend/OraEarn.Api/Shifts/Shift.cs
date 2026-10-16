using OraEarn.Api.Jobs;

namespace OraEarn.Api.Shifts;

public class Shift
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid JobId { get; set; }
    public DateOnly WorkDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public int BreakMinutes { get; set; }
    public decimal TotalHours { get; set; }
    public decimal TotalEarned { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public Job Job { get; set; } = null!;

    public static (decimal Hours, decimal Earned) Calculate(
        DateOnly workDate,
        TimeOnly startTime,
        TimeOnly endTime,
        int breakMinutes,
        decimal hourlyRate)
    {
        var start = workDate.ToDateTime(startTime);
        var end = workDate.ToDateTime(endTime);
        // A shift that ends at or before it starts crossed midnight, so the end is the next day.
        if (end <= start)
        {
            end = end.AddDays(1);
        }

        var hours = Math.Max(0, (decimal)(end - start).TotalHours - breakMinutes / 60m);
        // Rounded to cents and stored on the row so a later rate change does not rewrite this pay.
        var earned = Math.Round(hours * hourlyRate, 2);
        return (Math.Round(hours, 2), earned);
    }
}
