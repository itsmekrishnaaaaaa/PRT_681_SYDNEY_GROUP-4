using System.Net;
using System.Net.Http.Json;
using OraEarn.Api.Tests.Blackbox;

namespace OraEarn.Api.Tests.Blackbox;

public class AuthApiTests(OraEarnWebApplicationFactory factory) : IClassFixture<OraEarnWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Login_WithInvalidCredentials_ReturnsUnauthorized()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "test@oraearn.local",
            password = "WrongPassword!",
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsAccessToken()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "test@oraearn.local",
            password = "Test123!",
        });

        response.EnsureSuccessStatusCode();
        var body = await response.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.NotNull(body?.AccessToken);
        Assert.Equal("test@oraearn.local", body.User.Email);
    }

    [Fact]
    public async Task Me_WithoutToken_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/auth/me");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Me_WithValidToken_ReturnsProfile()
    {
        await _client.AuthenticateAsTestUserAsync();

        var response = await _client.GetAsync("/api/auth/me");
        response.EnsureSuccessStatusCode();

        var profile = await response.Content.ReadFromJsonAsync<UserProfileResponse>();
        Assert.Equal("Test", profile?.FirstName);
        Assert.Contains("User", profile?.Roles ?? []);
    }

    private sealed record AuthResponse(string AccessToken, UserProfileResponse User);

    private sealed record UserProfileResponse(
        string Id,
        string Email,
        string FirstName,
        string LastName,
        IReadOnlyList<string> Roles);
}
