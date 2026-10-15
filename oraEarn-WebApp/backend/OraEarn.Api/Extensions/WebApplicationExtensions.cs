using Microsoft.EntityFrameworkCore;
using Npgsql;
using OraEarn.Api.Data;

namespace OraEarn.Api.Extensions;

public static class WebApplicationExtensions
{
    /// <summary>
    /// Startup database flow: create the database if missing, apply pending EF migrations, then seed.
    /// Lets a new developer run the API against an empty PostgreSQL server with no manual steps.
    /// </summary>
    public static async Task ApplyMigrationsAndSeedAsync(this WebApplication app)
    {
        // On by default for the zero-setup local experience. A deployment that runs migrations as a
        // separate release step can set Database:ApplyMigrations=false so app instances never race
        // each other to migrate the same database.
        var applyMigrations = app.Configuration.GetValue("Database:ApplyMigrations", true);

        try
        {
            await EnsureDatabaseExistsAsync(app.Configuration);
        }
        // Connection failures are the most common first-run problem, so they are rethrown with the
        // exact file to edit instead of a raw Npgsql socket error.
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

        // Seeding always runs, even when migrations are skipped, because it is repeat-safe.
        await DbSeeder.SeedAsync(app.Services, app.Configuration);
    }

    // Connects to the always-present "postgres" maintenance database and creates the app database if
    // missing. This also runs when migrations are disabled, and it is the first thing to touch the
    // server, so connection problems surface here with the friendly message above. (EF's
    // MigrateAsync can create a missing database too; why this step was written separately is not
    // recorded.) The login in the connection string needs CREATEDB rights on first run.
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

        // CREATE DATABASE cannot take a parameter, so the name is quoted as an identifier with
        // embedded quotes doubled; the name comes from server configuration, not from users.
        await using var create = connection.CreateCommand();
        create.CommandText = $"CREATE DATABASE \"{databaseName.Replace("\"", "\"\"")}\"";
        await create.ExecuteNonQueryAsync();
    }
}
