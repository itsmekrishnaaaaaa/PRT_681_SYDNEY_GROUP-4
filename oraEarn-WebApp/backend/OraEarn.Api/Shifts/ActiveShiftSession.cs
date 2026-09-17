using OraEarn.Api.Jobs;
using OraEarn.Api.Users;

namespace OraEarn.Api.Shifts;

public class ActiveShiftSession
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public Guid JobId { get; set; }
    public DateOnly ShiftDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public int BreakMinutesAccumulated { get; set; }
    public DateTime? BreakStartedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public Job Job { get; set; } = null!;
}
