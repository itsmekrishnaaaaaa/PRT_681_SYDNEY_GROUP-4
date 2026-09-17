namespace OraEarn.Api.Notifications;

public record AppNotification(
    string Id,
    string Type,
    string Title,
    string Message,
    string Severity,
    string Category,
    DateTime CreatedAt,
    bool IsRead);

public record NotificationInboxResponse(
    IReadOnlyList<AppNotification> Items,
    int UnreadCount);
