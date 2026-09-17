using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OraEarn.Api.Analytics;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class AnalyticsController(AnalyticsService analyticsService) : ControllerBase
{
    [HttpGet("me")]
    public async Task<ActionResult<UserAnalyticsResponse>> GetMyAnalytics([FromQuery] string? period = "7d")
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null) return Unauthorized();

        var parsed = UserAnalyticsBuilder.ParsePeriod(period);
        return Ok(await analyticsService.GetUserAnalyticsAsync(userId, parsed));
    }
}