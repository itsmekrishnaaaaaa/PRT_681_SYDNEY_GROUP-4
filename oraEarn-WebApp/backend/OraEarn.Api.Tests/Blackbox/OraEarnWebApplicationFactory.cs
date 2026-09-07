using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OraEarn.Api.Constants;
using OraEarn.Api.Data;
using OraEarn.Api.Users;

namespace OraEarn.Api.Tests.Blackbox;

public class OraEarnWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _databaseName = $"OraEarnTests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Testing:InMemoryDatabase"] = _databaseName,
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        SeedAsync(scope.ServiceProvider).GetAwaiter().GetResult();

        return host;
    }

    private static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        await db.Database.EnsureCreatedAsync();

        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        foreach (var role in new[] { Roles.Admin, Roles.User })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        const string email = "test@oraearn.local";
        if (await userManager.FindByEmailAsync(email) is null)
        {
            var user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FirstName = "Test",
                LastName = "User",
                EmailConfirmed = true,
                ApprovalStatus = AccountApprovalStatus.Approved,
            };
            await userManager.CreateAsync(user, "Test123!");
            await userManager.AddToRoleAsync(user, Roles.User);
        }
    }
}

public static class HttpClientAuthExtensions
{
    public static async Task AuthenticateAsTestUserAsync(this HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/login", new
        {
            email = "test@oraearn.local",
            password = "Test123!",
        });

        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = json.GetProperty("accessToken").GetString()
            ?? throw new InvalidOperationException("Missing access token.");

        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
