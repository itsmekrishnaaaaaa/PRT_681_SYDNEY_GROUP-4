using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace OraEarn.Api.Tests.Blackbox;

public class RegistrationAndAdminApiTests(OraEarnWebApplicationFactory factory) : IClassFixture<OraEarnWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task Register_ValidStudent_ReturnsPendingAndBlocksLogin()
    {
        var email = $"student-{Guid.NewGuid():N}@test.local";

        var register = await _client.PostAsync("/api/auth/register", BuildRegistration(email, "Passw0rd!", "Passw0rd!"));
        Assert.Equal(HttpStatusCode.OK, register.StatusCode);
        var body = await register.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Pending", body.GetProperty("approvalStatus").GetString());

        var login = await _client.PostAsJsonAsync("/api/auth/login", new { email, password = "Passw0rd!" });
        Assert.Equal(HttpStatusCode.Unauthorized, login.StatusCode);
        Assert.Contains("pending", await login.Content.ReadAsStringAsync(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Register_PasswordMismatch_ReturnsBadRequest()
    {
        var response = await _client.PostAsync("/api/auth/register",
            BuildRegistration($"mismatch-{Guid.NewGuid():N}@test.local", "Passw0rd!", "Different1!"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("do not match", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Register_InvalidAustralianPhone_ReturnsBadRequest()
    {
        var response = await _client.PostAsync("/api/auth/register",
            BuildRegistration($"phone-{Guid.NewGuid():N}@test.local", "Passw0rd!", "Passw0rd!", phone: "12345"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Australian phone", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task AdminUsers_AsStandardUser_ReturnsForbidden()
    {
        await _client.AuthenticateAsTestUserAsync();

        var response = await _client.GetAsync("/api/admin/users");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static MultipartFormDataContent BuildRegistration(string email, string password, string confirm, string phone = "+61412345678")
    {
        var form = new MultipartFormDataContent
        {
            { new StringContent(email), "email" },
            { new StringContent(password), "password" },
            { new StringContent(confirm), "confirmPassword" },
            { new StringContent("Test"), "firstName" },
            { new StringContent("Student"), "lastName" },
            { new StringContent(phone), "phoneNumber" },
            { new StringContent("2000-01-01"), "dateOfBirth" },
            { new StringContent("Nepalese"), "nationality" },
            { new StringContent("Subclass 500"), "visaType" },
            { new StringContent("Student"), "visaStatus" },
            { new StringContent("Student"), "workingRights" },
            { new StringContent(DateTime.UtcNow.AddYears(2).ToString("yyyy-MM-dd")), "visaExpiryDate" },
            { new StringContent("Passport"), "identityDocumentType" },
        };

        var document = new ByteArrayContent("%PDF-1.4 test"u8.ToArray());
        document.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("application/pdf");
        form.Add(document, "visaDocument", "visa.pdf");
        return form;
    }
}
