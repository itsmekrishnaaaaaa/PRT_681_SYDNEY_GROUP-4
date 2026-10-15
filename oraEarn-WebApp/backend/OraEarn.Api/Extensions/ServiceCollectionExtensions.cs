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

// Composition root for the API. Configuration problems are turned into startup exceptions here on
// purpose: a misconfigured server should refuse to start, not run and fail on the first request.
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers persistence, Identity, JWT authentication, CORS, SignalR and every module service.
    /// Throws at startup if the connection string, JWT secret or CORS origins are unusable.
    /// </summary>
    public static IServiceCollection AddApplicationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));
        services.Configure<AiAdvisorSettings>(configuration.GetSection(AiAdvisorSettings.SectionName));

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            // The test host swaps in an in-memory database so the suite runs without PostgreSQL.
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

        // Password and lockout policy implement NFR-2.2: 8+ characters with upper, lower and digit.
        // Symbols are not required. Five failures lock the account for 15 minutes to slow guessing.
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

                    // Default skew is 5 minutes; 1 minute keeps short-lived access tokens short
                    // while tolerating small clock differences between servers.
                    ClockSkew = TimeSpan.FromMinutes(1)
                };

                options.Events = new JwtBearerEvents
                {
                    // Browsers cannot set an Authorization header on a WebSocket, so the SignalR
                    // client sends the token as ?access_token=. It is accepted only on /hubs paths
                    // so tokens in query strings (which end up in logs) stay limited to that case.
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

        // Enums travel as strings ("Pending", "Fixed") so the React client and Swagger stay readable
        // and do not break if enum members are reordered.
        services.AddControllers()
            .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        });
        // Swagger is a development tool only; production does not expose the API description.
        if (environment.IsDevelopment())
        {
            services.AddSwaggerDocumentation();
        }

        services.AddSignalR();

        // An explicit origin list is required because AllowCredentials (needed by SignalR) cannot be
        // combined with "allow any origin".
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

        // Tokens are signed with HMAC-SHA256, which needs a key of at least 256 bits; a short or
        // missing secret would make every token forgeable, so the API refuses to start instead.
        if (string.IsNullOrWhiteSpace(jwt.Secret) || jwt.Secret.Length < 32)
        {
            throw new InvalidOperationException("Jwt:Secret must be at least 32 characters. Set Jwt__Secret.");
        }

        // The committed development secret is public in the repository. Refusing it outside
        // Development/Testing stops a deployment from accidentally going live with a known key.
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

        // With no origins the browser would block every frontend call, which looks like a network
        // fault rather than a config error; failing at startup makes the cause obvious.
        if (origins.Length == 0)
        {
            throw new InvalidOperationException("Cors:AllowedOrigins must contain at least one origin.");
        }

        return origins;
    }
}
