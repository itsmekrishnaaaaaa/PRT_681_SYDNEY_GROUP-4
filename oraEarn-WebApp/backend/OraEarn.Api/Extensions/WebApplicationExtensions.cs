using Microsoft.EntityFrameworkCore;
using Npgsql;
using OraEarn.Api.Data;

namespace OraEarn.Api.Extensions;

public static class WebApplicationExtensions
{
    public static async Task ApplyMigrationsAndSeedAsync(this WebApplication app)
    {
        var applyMigrations = app.Configuration.GetValue("Database:ApplyMigrations", true);

        try
        {
            await EnsureDatabaseExistsAsync(app.Configuration);
        }
        catch (Exception ex) when (ex is NpgsqlException or InvalidOperationException)
        {
            throw new InvalidOperationException(
                "Could not connect to PostgreSQL. Set Host, Port, Database, Username, and Password in " +
                "backend/OraEarn.Api/appsettings.Local.json (copy from appsettings.Local.json.example if needed).",
                ex);
        }

        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        if (applyMigrations)
        {
            await db.Database.MigrateAsync();
        }

        await DbSeeder.SeedAsync(app.Services, app.Configuration);
    }

    private static async Task EnsureDatabaseExistsAsync(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            throw new InvalidOperationException("ConnectionStrings:DefaultConnection is empty.");
        }

        var builder = new NpgsqlConnectionStringBuilder(connectionString);
        var databaseName = builder.Database
            ?? throw new InvalidOperationException("Database name is required in the connection string.");

        builder.Database = "postgres";

        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        await connection.OpenAsync();

        await using var check = connection.CreateCommand();
        check.CommandText = "SELECT 1 FROM pg_database WHERE datname = @name";
        check.Parameters.AddWithValue("name", databaseName);
        var exists = await check.ExecuteScalarAsync() is not null;

        if (exists)
        {
            return;
        }

        await using var create = connection.CreateCommand();
        create.CommandText = $"CREATE DATABASE \"{databaseName.Replace("\"", "\"\"")}\"";
        await create.ExecuteNonQueryAsync();
    }
}
