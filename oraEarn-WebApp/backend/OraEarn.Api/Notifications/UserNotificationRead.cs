using OraEarn.Api.Users;

namespace OraEarn.Api.Notifications;

public class UserNotificationRead
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public string NotificationKey { get; set; } = string.Empty;
    public DateTime ReadAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
}
