using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OraEarn.Api.Shifts;

[Authorize]
public class ShiftsHub : Hub
{
    public static string UserGroup(string userId) => $"user-{userId}";

    // The group is chosen from the JWT's user id, never from client input, so a client cannot
    // subscribe to someone else's shift updates.
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User?.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is not null)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, UserGroup(userId));
        }

        await base.OnConnectedAsync();
    }
}
