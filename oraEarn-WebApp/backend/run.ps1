$ErrorActionPreference = "Stop"
$Api = Join-Path $PSScriptRoot "OraEarn.Api"
$example = Join-Path $Api "appsettings.Local.json.example"
$local = Join-Path $Api "appsettings.Local.json"
if (-not (Test-Path $local) -and (Test-Path $example)) {
    Copy-Item $example $local
    Write-Host "Created appsettings.Local.json - change Password if your Postgres is not 12345." -ForegroundColor Yellow
}
Set-Location $Api
dotnet run --urls http://localhost:5203
