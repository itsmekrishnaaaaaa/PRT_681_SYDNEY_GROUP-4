using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OraEarn.Api.Constants;
using OraEarn.Api.Users;

namespace OraEarn.Api.Auth;

// Token model (NFR-2.1, NFR-2.5, NFR-4.4):
// - Access token: a signed JWT, 15 minutes by default (Jwt:AccessTokenMinutes). It is stateless, so
//   it is not stored and cannot be revoked; keeping it short limits the damage of a leaked token.
// - Refresh token: 64 random bytes, 7 days by default (Jwt:RefreshTokenDays), stored on the user row.
//   Each refresh issues a new pair and overwrites the stored value (rotation), so a refresh token
//   works once. Logout clears it (revocation).
// Only one refresh token is stored per user, so signing in on a second device ends the first
// device's ability to refresh; it keeps working only until its access token expires.
public class TokenService(
    UserManager<ApplicationUser> userManager,
    IOptions<JwtSettings> jwtOptions)
{
    private readonly JwtSettings _jwt = jwtOptions.Value;

    /// <summary>
    /// Issues a new access token and a new refresh token for the user, replacing any refresh token
    /// they had before.
    /// </summary>
    public async Task<AuthResponse> CreateAuthResponseAsync(ApplicationUser user)
    {
        var roles = await userManager.GetRolesAsync(user);
        var accessToken = GenerateAccessToken(user, roles);
        var refreshToken = GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(_jwt.RefreshTokenDays);
        await userManager.UpdateAsync(user);

        return new AuthResponse(
            accessToken,
            refreshToken,
            DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes),
            MapUser(user, roles));
    }

    /// <summary>
    /// Exchanges a valid, unexpired refresh token for a new token pair (rotation).
    /// Returns null for an unknown, already-used, revoked or expired token.
    /// </summary>
    public async Task<AuthResponse?> RefreshAsync(string refreshToken)
    {
        // The stored token is compared as plain text and approval status is not re-checked here;
        // a user rejected after approval keeps refreshing until the token expires or they log out.
        var user = userManager.Users.SingleOrDefault(u =>
            u.RefreshToken == refreshToken &&
            u.RefreshTokenExpiry > DateTime.UtcNow);

        if (user is null)
        {
            return null;
        }

        return await CreateAuthResponseAsync(user);
    }

    /// <summary>
    /// Clears the user's refresh token so it can no longer be used. Access tokens already issued stay
    /// valid until they expire.
    /// </summary>
    public async Task RevokeRefreshTokenAsync(ApplicationUser user)
    {
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await userManager.UpdateAsync(user);
    }

    private string GenerateAccessToken(ApplicationUser user, IList<string> roles)
    {
        // NameIdentifier duplicates sub because controllers and the SignalR hub read the user id from
        // ClaimTypes.NameIdentifier. Roles drive [Authorize(Roles = ...)] on the admin endpoints.
        // The name claims are not read by the React app, which uses the user object in AuthResponse.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id),
            new("firstName", user.FirstName),
            new("lastName", user.LastName)
        };

        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.Secret));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwt.Issuer,
            audience: _jwt.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwt.AccessTokenMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    // A refresh token is an opaque random value, not a JWT: it only has to be unguessable and is
    // validated by looking it up, which is what makes rotation and revocation possible.
    private static string GenerateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    /// <summary>Builds the user profile returned with tokens and by /api/auth/me.</summary>
    public static UserProfileResponse MapUser(ApplicationUser user, IList<string> roles) =>
        new(user.Id, user.Email ?? string.Empty, user.FirstName, user.LastName, roles.ToList(), user.ApprovalStatus);
}
