using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using OraEarn.Api.Users;

namespace OraEarn.Api.Auth;

[ApiController]
[Route("api/[controller]")]
public class AuthController(
    AuthService authService,
    TokenService tokenService,
    UserManager<ApplicationUser> userManager) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(6_000_000)]
    public async Task<ActionResult<RegisterResponse>> Register([FromForm] RegisterForm form)
    {
        var (success, response, error) = await authService.RegisterAsync(form.ToRequest(), form.VisaDocument);
        if (!success)
        {
            return BadRequest(new { message = error });
        }

        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
    {
        var (success, response, error) = await authService.LoginAsync(request);
        if (!success)
        {
            return Unauthorized(new { message = error });
        }

        return Ok(response);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Refresh(RefreshTokenRequest request)
    {
        var response = await tokenService.RefreshAsync(request.RefreshToken);
        if (response is null)
        {
            return Unauthorized(new { message = "Invalid or expired refresh token." });
        }

        return Ok(response);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(userId);
        if (user is not null)
        {
            await tokenService.RevokeRefreshTokenAsync(user);
        }

        return NoContent();
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<UserProfileResponse>> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId is null)
        {
            return Unauthorized();
        }

        var profile = await authService.GetProfileAsync(userId);
        if (profile is null)
        {
            return NotFound();
        }

        return Ok(profile);
    }
}
