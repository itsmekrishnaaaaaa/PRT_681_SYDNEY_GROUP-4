using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using OraEarn.Api.Constants;
using OraEarn.Api.Expenses;
using OraEarn.Api.Users;

namespace OraEarn.Api.Data;

// Runs on every startup, so every step checks before inserting and is safe to repeat.
public static class DbSeeder
{
    /// <summary>
    /// Ensures roles and payment methods exist, then either loads the demo data set (Seed:DemoData)
    /// or creates a single admin from Seed:AdminEmail / Seed:AdminPassword if both are configured.
    /// </summary>
    public static async Task SeedAsync(IServiceProvider services, IConfiguration configuration)
    {
        using var scope = services.CreateScope();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var seed = configuration.GetSection(SeedOptions.SectionName).Get<SeedOptions>() ?? new SeedOptions();

        foreach (var role in Roles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        await SeedPaymentMethodsAsync(scope.ServiceProvider.GetRequiredService<ApplicationDbContext>());

        // The demo admin's password is public in the README, so DemoData must stay off anywhere real.
        if (seed.DemoData)
        {
            await EnsureAdminAsync(userManager, "admin@oraearn.local", "Admin123!", isDemo: true);
            await DemoDataSeeder.SeedDemoDataAsync(services);
            return;
        }

        if (!string.IsNullOrWhiteSpace(seed.AdminEmail) && !string.IsNullOrWhiteSpace(seed.AdminPassword))
        {
            await EnsureAdminAsync(userManager, seed.AdminEmail.Trim(), seed.AdminPassword, isDemo: false);
        }
    }

    private static async Task EnsureAdminAsync(
        UserManager<ApplicationUser> userManager,
        string email,
        string password,
        bool isDemo)
    {
        var admin = await userManager.FindByEmailAsync(email);
        if (admin is not null)
        {
            return;
        }

        admin = new ApplicationUser
        {
            UserName = email,
            Email = email,
            FirstName = isDemo ? "System" : "Admin",
            LastName = isDemo ? "Admin" : "User",
            EmailConfirmed = true,
            CreatedAt = isDemo ? DateTime.UtcNow.AddYears(-1) : DateTime.UtcNow,
            DateOfBirth = isDemo ? new DateOnly(1985, 1, 1) : null,
            Nationality = isDemo ? "Australian" : string.Empty,
            PhoneNumber = isDemo ? "+61400000001" : null,
            VisaType = isDemo ? "N/A" : string.Empty,
            VisaStatus = "Admin",
            WorkingRights = "Full Time",
            IdentityDocumentType = IdentityDocumentType.Passport,
            ApprovalStatus = AccountApprovalStatus.Approved,
            ApprovedAt = DateTime.UtcNow,
        };

        var result = await userManager.CreateAsync(admin, password);
        // Fail startup loudly (e.g. a configured admin password that breaks the password policy)
        // rather than run with no way to sign in as admin.
        if (!result.Succeeded)
        {
            var errors = string.Join("; ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Failed to create admin user '{email}': {errors}");
        }

        await userManager.AddToRoleAsync(admin, Roles.Admin);
    }

    private static async Task SeedPaymentMethodsAsync(ApplicationDbContext db)
    {
        foreach (var name in PaymentMethodNames.All)
        {
            if (!await db.PaymentMethods.AnyAsync(p => p.Name == name))
            {
                db.PaymentMethods.Add(new PaymentMethod { Name = name });
            }
        }

        await db.SaveChangesAsync();
    }
}
