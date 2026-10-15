using Microsoft.AspNetCore.HttpOverrides;
using OraEarn.Api.Auth;
using OraEarn.Api.Extensions;
using OraEarn.Api.Shifts;

var builder = WebApplication.CreateBuilder(args);

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

builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);

builder.Services.AddProblemDetails();
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

app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHub<ShiftsHub>("/hubs/shifts");
app.MapGet("/health", () => Results.Ok(new { status = "ok" })).AllowAnonymous();

if (!app.Environment.IsEnvironment("Testing"))
{
    await app.ApplyMigrationsAndSeedAsync();
}

app.Run();

public partial class Program;
