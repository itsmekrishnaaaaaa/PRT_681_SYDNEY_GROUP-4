using Microsoft.AspNetCore.SignalR;

namespace OraEarn.Api.Shifts;

public class ShiftNotificationService(IHubContext<ShiftsHub> hub)
{
    public Task NotifyCreatedAsync(string userId, ShiftResponse shift) =>
        hub.Clients.Group(ShiftsHub.UserGroup(userId)).SendAsync("ShiftCreated", shift);

    public Task NotifyUpdatedAsync(string userId, ShiftResponse shift) =>
        hub.Clients.Group(ShiftsHub.UserGroup(userId)).SendAsync("ShiftUpdated", shift);

    public Task NotifyDeletedAsync(string userId, Guid shiftId) =>
        hub.Clients.Group(ShiftsHub.UserGroup(userId)).SendAsync("ShiftDeleted", shiftId);
}
