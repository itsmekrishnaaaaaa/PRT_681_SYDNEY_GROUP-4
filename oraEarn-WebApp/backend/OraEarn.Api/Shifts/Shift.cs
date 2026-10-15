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

    /// <summary>
    /// Calculates paid hours and earnings for one shift. The caller stores the result on the row
    /// rather than recomputing it, so later edits to the job's hourly rate never rewrite past pay.
    /// </summary>
    public static (decimal Hours, decimal Earned) Calculate(
        DateOnly workDate,
        TimeOnly startTime,
        TimeOnly endTime,
        int breakMinutes,
        decimal hourlyRate)
    {
        var start = workDate.ToDateTime(startTime);
        var end = workDate.ToDateTime(endTime);

        // A shift only records times of day, so an end at or before the start can only mean the
        // shift crossed midnight (e.g. 22:00-06:00 is 8 hours, not -16). Equal times are rejected
        // earlier by ShiftService, so this never turns a zero-length entry into a 24-hour shift.
        if (end <= start)
        {
            end = end.AddDays(1);
        }

        // A break longer than the shift floors at zero instead of producing negative pay.
        var hours = Math.Max(0, (decimal)(end - start).TotalHours - breakMinutes / 60m);

        // Money and hours are rounded to cents/hundredths here, once, to match the numeric(18,2)
        // columns. Earnings use the unrounded hours so rounding errors do not compound.
        var earned = Math.Round(hours * hourlyRate, 2);
        return (Math.Round(hours, 2), earned);
    }
}
