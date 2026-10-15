using Microsoft.AspNetCore.SignalR;

namespace OraEarn.Api.Shifts;

// Pushes shift changes to every open tab of the same user so calendars stay in sync without polling.
// Messages go to the user's private group only, never to all clients, because they contain pay data.
public class ShiftNotificationService(IHubContext<ShiftsHub> hub)
{
    /// <summary>Tells the user's connected clients that a shift was created.</summary>
    public Task NotifyCreatedAsync(string userId, ShiftResponse shift) =>
        hub.Clients.Group(ShiftsHub.UserGroup(userId)).SendAsync("ShiftCreated", shift);

    /// <summary>Tells the user's connected clients that a shift was edited.</summary>
    public Task NotifyUpdatedAsync(string userId, ShiftResponse shift) =>
        hub.Clients.Group(ShiftsHub.UserGroup(userId)).SendAsync("ShiftUpdated", shift);

    /// <summary>Tells the user's connected clients that a shift was deleted.</summary>
    public Task NotifyDeletedAsync(string userId, Guid shiftId) =>
        hub.Clients.Group(ShiftsHub.UserGroup(userId)).SendAsync("ShiftDeleted", shiftId);
}
