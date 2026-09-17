using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OraEarn.Api.Analytics;
using OraEarn.Api.Constants;
using OraEarn.Api.Users;

namespace OraEarn.Api.Admin;

[ApiController]
[Authorize(Roles = Roles.Admin)]
[Route("api/[controller]")]
public class AdminController(
    AdminService adminService,
    AnalyticsService analyticsService,
    RegistrationApprovalService registrationApprovalService,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpGet("analytics")]
    public async Task<ActionResult<AdminAnalyticsResponse>> GetAnalytics()
    {
        return Ok(await analyticsService.GetPlatformAnalyticsAsync());
    }

    [HttpGet("users")]
    public async Task<ActionResult<IReadOnlyList<AdminUserResponse>>> GetUsers()
    {
        var users = await adminService.GetUsersAsync();
        return Ok(users);
    }

    [HttpPut("users/{userId}/roles")]
    public async Task<IActionResult> UpdateRoles(string userId, UpdateUserRolesRequest request)
    {
        var (success, error) = await adminService.UpdateRolesAsync(userId, request);
        if (!success)
        {
            return BadRequest(new { message = error });
        }

        return NoContent();
    }

    [HttpGet("registrations/pending")]
    public async Task<ActionResult<IReadOnlyList<PendingRegistrationResponse>>> GetPendingRegistrations()
    {
        return Ok(await registrationApprovalService.GetPendingAsync());
    }

    [HttpGet("registrations/{userId}")]
    public async Task<ActionResult<PendingRegistrationResponse>> GetRegistration(string userId)
    {
        var registration = await registrationApprovalService.GetByIdAsync(userId);
        return registration is null ? NotFound() : Ok(registration);
    }

    [HttpGet("registrations/{userId}/document")]
    public async Task<IActionResult> GetRegistrationDocument(string userId)
    {
        var user = await userManager.FindByIdAsync(userId);
        if (user is null)
        {
            return NotFound();
        }

        var (fullPath, fileName, contentType) = registrationApprovalService.GetDocumentInfo(user);
        if (fullPath is null || contentType is null)
        {
            return NotFound(new { message = "Visa document not found." });
        }

        return PhysicalFile(fullPath, contentType, fileName);
    }

    [HttpPost("registrations/{userId}/approve")]
    public async Task<IActionResult> ApproveRegistration(string userId, RegistrationDecisionRequest request)
    {
        var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (adminId is null)
        {
            return Unauthorized();
        }

        var (success, error) = await registrationApprovalService.ApproveAsync(userId, adminId, request.Notes);
        return success ? NoContent() : BadRequest(new { message = error });
    }

    [HttpPost("registrations/{userId}/reject")]
    public async Task<IActionResult> RejectRegistration(string userId, RegistrationDecisionRequest request)
    {
        var adminId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (adminId is null)
        {
            return Unauthorized();
        }

        var (success, error) = await registrationApprovalService.RejectAsync(userId, adminId, request.Notes);
        return success ? NoContent() : BadRequest(new { message = error });
    }

    [HttpPost("registrations/{userId}/ai-review")]
    public async Task<ActionResult<RegistrationAiReviewResponse>> AiReviewRegistration(string userId)
    {
        var review = await registrationApprovalService.RunAiReviewAsync(userId);
        return review is null ? NotFound() : Ok(review);
    }
}