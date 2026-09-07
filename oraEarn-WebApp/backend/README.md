# OraEarn API (.NET)

Standalone ASP.NET Core service. Talks to PostgreSQL and optionally to Aura on port 8000.

## First time

```powershell
dotnet restore
```

If your Postgres password is not `12345`, copy `OraEarn.Api/appsettings.Local.json.example` to `OraEarn.Api/appsettings.Local.json` and set `Password`.

## Run

```powershell
cd OraEarn.Api
dotnet run --urls http://localhost:5203
```

Or from this folder: `.\run.ps1`

- API: http://localhost:5203
- Swagger (Development): http://localhost:5203/swagger

The API creates the `oraearn` database and tables on first start.

## Tests

```powershell
dotnet test OraEarn.Api.Tests\OraEarn.Api.Tests.csproj
```
