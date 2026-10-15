using Microsoft.OpenApi.Models;

namespace OraEarn.Api.Auth;

// Registered only in Development (see ServiceCollectionExtensions and Program).
public static class SwaggerExtensions
{
    /// <summary>
    /// Adds Swagger generation with a JWT bearer scheme, so the Authorize button can call protected endpoints.
    /// </summary>
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "OraEarn API",
                Version = "v1",
                Description = "Shift, finance, and visa compliance API with JWT authentication.",
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Paste JWT only, or use: Bearer {your token}",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer",
                        },
                    },
                    Array.Empty<string>()
                },
            });
        });

        return services;
    }

    /// <summary>Serves the OpenAPI document and the Swagger UI at /swagger.</summary>
    public static WebApplication UseSwaggerDocumentation(this WebApplication app)
    {
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            options.SwaggerEndpoint("/swagger/v1/swagger.json", "OraEarn API v1");
            options.RoutePrefix = "swagger";
        });

        return app;
    }
}
