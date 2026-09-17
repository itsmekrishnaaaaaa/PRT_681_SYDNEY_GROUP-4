using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OraEarn.Api.Notifications;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class NotificationsController(NotificationService notificationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<NotificationInboxResponse>> GetInbox()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        return Ok(await notificationService.GetInboxAsync(userId));
    }

    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkRead(string id)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        await notificationService.MarkReadAsync(userId, id);
        return NoContent();
    }

    [HttpPost("read-all")]
    public async Task<IActionResult> MarkAllRead([FromBody] MarkAllReadRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        await notificationService.MarkAllReadAsync(userId, request.NotificationIds);
        return NoContent();
    }
}

public record MarkAllReadRequest(IReadOnlyList<string> NotificationIds);