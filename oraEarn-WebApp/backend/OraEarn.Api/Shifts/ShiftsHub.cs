using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;

namespace OraEarn.Api.Shifts;

[Authorize]
public class ShiftsHub : Hub
{
    public static string UserGroup(string userId) => $"user-{userId}";

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
