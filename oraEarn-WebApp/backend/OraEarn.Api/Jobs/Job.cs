using OraEarn.Api.Shifts;
using OraEarn.Api.Users;

namespace OraEarn.Api.Jobs;

public class Job
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal HourlyRate { get; set; }
    public DateOnly DateOfJoin { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public ICollection<Shift> Shifts { get; set; } = [];
    public ICollection<ActiveShiftSession> ActiveShiftSessions { get; set; } = [];
}
