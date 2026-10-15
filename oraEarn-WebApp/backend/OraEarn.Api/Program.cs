using Microsoft.AspNetCore.HttpOverrides;
using OraEarn.Api.Auth;
using OraEarn.Api.Extensions;
using OraEarn.Api.Shifts;

var builder = WebApplication.CreateBuilder(args);

// First-run convenience: give a new developer an editable appsettings.Local.json (gitignored) with
// a default local connection string, so real passwords never have to go into a committed file.
var localSettings = Path.Combine(builder.Environment.ContentRootPath, "appsettings.Local.json");
var localExample = Path.Combine(builder.Environment.ContentRootPath, "appsettings.Local.json.example");
if (!File.Exists(localSettings) && File.Exists(localExample))
{
    try
    {
        File.Copy(localExample, localSettings);
    }
    catch (IOException) when (File.Exists(localSettings))
    {
        // Another host instance created it first (e.g. parallel test factories).
    }
}

// Added after CreateBuilder's defaults, so it is the last source and wins over appsettings.json,
// appsettings.{Environment}.json, environment variables and command-line arguments. That makes a
// developer's local file authoritative on their machine, but it also means a stray Local.json on a
// server would override production environment variables - keep it out of deployments.
builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddProblemDetails();

// Hosting platforms terminate HTTPS at a proxy whose address is not known in advance, so the known
// proxy lists are cleared and X-Forwarded-For/Proto are trusted from any sender. This is only safe
// when the API is reachable solely through that proxy.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});
builder.Services.AddApplicationServices(builder.Configuration, builder.Environment);

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseForwardedHeaders();
    app.UseExceptionHandler();
    app.UseHsts();
    app.UseHttpsRedirection();
}

if (app.Environment.IsDevelopment())
{
    app.UseSwaggerDocumentation();
}

// CORS must run before authentication so browser preflight requests are answered without a token.
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ShiftsHub>("/hubs/shifts");
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

// Tests use the in-memory provider, which has no migrations, and seed their own data.
if (!app.Environment.IsEnvironment("Testing"))
{
    await app.ApplyMigrationsAndSeedAsync();
}

app.Run();

// Makes the top-level Program visible to WebApplicationFactory<Program> in the test project.
public partial class Program;
