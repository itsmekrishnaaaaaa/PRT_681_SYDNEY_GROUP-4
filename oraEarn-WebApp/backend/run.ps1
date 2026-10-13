$ErrorActionPreference = "Stop"
$Api = Join-Path $PSScriptRoot "OraEarn.Api"
$example = Join-Path $Api "appsettings.Local.json.example"
$local = Join-Path $Api "appsettings.Local.json"
if (-not (Test-Path $local) -and (Test-Path $example)) {
    Copy-Item $example $local
    Write-Host "Created appsettings.Local.json - change Password if your Postgres is not 12345." -ForegroundColor Yellow
}
Set-Location $Api
$urls = "http://localhost:5203"
$sdks = @(
    try { & dotnet --list-sdks 2>$null }
    catch { @() }
)
if ($sdks.Count -gt 0) {
    dotnet run --urls $urls
    exit $LASTEXITCODE
}

$vsRuntime = Join-Path ${env:ProgramFiles} "Microsoft Visual Studio\18\Community\dotnet\net10.0\runtime"
$hostExe = Join-Path $vsRuntime "dotnet.exe"
$dll = Join-Path $Api "bin\Debug\net10.0\OraEarn.Api.dll"
if (-not (Test-Path $hostExe)) {
    throw "No .NET 10 SDK on PATH. Install it from https://aka.ms/dotnet/download then run this script again."
}
if (-not (Test-Path $dll)) {
    throw "Build OraEarn.Api in Visual Studio first (Build > Build OraEarn.Api), then run this script."
}

Write-Host "No .NET SDK on PATH; starting with Visual Studio's .NET 10 runtime." -ForegroundColor Yellow
$env:DOTNET_ROOT = $vsRuntime
$env:DOTNET_MULTILEVEL_LOOKUP = "0"
$env:ASPNETCORE_ENVIRONMENT = "Development"
cmd.exe /c "`"$hostExe`" exec `"$dll`" --urls $urls"
