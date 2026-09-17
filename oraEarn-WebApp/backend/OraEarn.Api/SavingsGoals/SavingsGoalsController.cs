using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OraEarn.Api.AiAdvisor;

namespace OraEarn.Api.SavingsGoals;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class SavingsGoalsController(
    SavingsGoalService savingsGoalService,
    SavingsGoalAnalyticsService analyticsService,
    SavingsAdvisorService advisorService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<SavingsGoalResponse>>> GetAll()
    {
        var userId = GetUserId();
        return Ok(await savingsGoalService.GetAllAsync(userId));
    }

    [HttpGet("analytics")]
    public async Task<ActionResult<SavingsGoalAnalyticsResponse>> GetAnalytics()
    {
        var userId = GetUserId();
        return Ok(await analyticsService.GetAnalyticsAsync(userId));
    }

    [HttpPost("advisor/chat")]
    public async Task<ActionResult<AdvisorChatResponse>> AdvisorChat(AdvisorChatRequest request)
    {
        var userId = GetUserId();
        if (string.IsNullOrWhiteSpace(request.Message))
        {
            return BadRequest(new { message = "Message is required." });
        }

        return Ok(await advisorService.ChatAsync(userId, request.Message.Trim()));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SavingsGoalResponse>> GetById(Guid id)
    {
        var userId = GetUserId();
        var goal = await savingsGoalService.GetByIdAsync(userId, id);
        return goal is null ? NotFound() : Ok(goal);
    }

    [HttpPost]
    public async Task<ActionResult<SavingsGoalResponse>> Create(SavingsGoalRequest request)
    {
        try
        {
            var userId = GetUserId();
            var goal = await savingsGoalService.CreateAsync(userId, request);
            return CreatedAtAction(nameof(GetById), new { id = goal.Id }, goal);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<SavingsGoalResponse>> Update(Guid id, SavingsGoalRequest request)
    {
        try
        {
            var userId = GetUserId();
            var goal = await savingsGoalService.UpdateAsync(userId, id, request);
            return goal is null ? NotFound() : Ok(goal);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/contribute")]
    public async Task<ActionResult<SavingsGoalResponse>> Contribute(Guid id, SavingsGoalContributionRequest request)
    {
        try
        {
            var userId = GetUserId();
            var goal = await savingsGoalService.ContributeAsync(userId, id, request);
            return goal is null ? NotFound() : Ok(goal);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var deleted = await savingsGoalService.DeleteAsync(userId, id);
        return deleted ? NoContent() : NotFound();
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();
}