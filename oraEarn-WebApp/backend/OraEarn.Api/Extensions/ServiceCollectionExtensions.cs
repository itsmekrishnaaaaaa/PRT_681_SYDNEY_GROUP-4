using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using OraEarn.Api.Admin;
using OraEarn.Api.AiAdvisor;
using OraEarn.Api.Analytics;
using OraEarn.Api.Auth;
using OraEarn.Api.Data;
using OraEarn.Api.Expenses;
using OraEarn.Api.Jobs;
using OraEarn.Api.Notifications;
using OraEarn.Api.SavingsGoals;
using OraEarn.Api.Shifts;
using OraEarn.Api.Users;
using OraEarn.Api.VisaCompliance;

namespace OraEarn.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<AiAdvisorSettings>(configuration.GetSection(AiAdvisorSettings.SectionName));

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            if (environment.IsEnvironment("Testing"))
            {
                var dbName = configuration["Testing:InMemoryDatabase"] ?? "OraEarnTests";
                options.UseInMemoryDatabase(dbName);
                return;
            }

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "ConnectionStrings:DefaultConnection is required. Set it via environment or appsettings.Local.json.");
            }

            options.UseNpgsql(connectionString);
        });

        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var jwt = RequireJwtSettings(configuration, environment);

        services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)),
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                        {
                            context.Token = accessToken;
                        }

                        return Task.CompletedTask;
                    }
                };
            });
        services.AddAuthorization();

        services.AddScoped<AuthService>();
        services.AddScoped<TokenService>();
        services.AddScoped<JobService>();
        services.AddScoped<ShiftService>();
        services.AddScoped<ActiveShiftSessionService>();
        services.AddScoped<ShiftNotificationService>();
        services.AddScoped<ExpenseCategoryService>();
        services.AddScoped<SavingsGoalService>();
        services.AddScoped<SavingsGoalSyncService>();
        services.AddScoped<SavingsGoalAnalyticsService>();
        services.AddScoped<SavingsAdvisorService>();
        services.AddHttpClient<PythonAdvisorClient>();
        services.AddScoped<ExpenseService>();
        services.AddScoped<AdminService>();
        services.AddScoped<AnalyticsService>();
        services.AddScoped<NotificationService>();
        services.AddScoped<VisaComplianceService>();
        services.AddScoped<RegistrationApprovalAdvisorService>();
        services.AddScoped<RegistrationApprovalService>();
        services.AddScoped<VisaDocumentStorageService>();

        services.AddControllers()
            .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });
        if (environment.IsDevelopment())
        {
            services.AddSwaggerDocumentation();
        }

        services.AddSignalR();
        services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy.WithOrigins(RequireCorsOrigins(configuration))
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        return services;
    }

    private static JwtSettings RequireJwtSettings(IConfiguration configuration, IHostEnvironment environment)
    {
        var jwt = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>()
            ?? throw new InvalidOperationException("JWT settings are missing.");

        // Refuse to boot with a short or placeholder key. A running API with a weak secret is worse than one that will not start.
        if (string.IsNullOrWhiteSpace(jwt.Secret) || jwt.Secret.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Secret must be at least 32 characters. Set Jwt__Secret.");
        }

        if (!environment.IsDevelopment()
            && !environment.IsEnvironment("Testing")
            && jwt.Secret.Contains("DevOnly", StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Jwt:Secret must not use the development placeholder outside Development.");
        }

        return jwt;
    }

    private static string[] RequireCorsOrigins(IConfiguration configuration)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()?
            .Where(origin => !string.IsNullOrWhiteSpace(origin))
            .ToArray() ?? [];

        if (origins.Length == 0)
        {
            throw new InvalidOperationException("Cors:AllowedOrigins must contain at least one origin.");
        }

        return origins;
    }
}
