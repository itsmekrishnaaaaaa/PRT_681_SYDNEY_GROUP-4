# OraEarn

Three separate services. They are not one app.

```
frontend/     React (Vite)     →  http://localhost:5173
backend/      ASP.NET API      →  http://localhost:5203
ai-advisor/   Aura (Python)    →  http://localhost:8000   optional
```

The browser talks only to the **backend**. The backend talks to **PostgreSQL** and, if it is running, to **Aura**.

## First-time install

Install [.NET 10 SDK](https://dotnet.microsoft.com/download), [Node.js 20+](https://nodejs.org/), [PostgreSQL](https://www.postgresql.org/download/windows/), and [Python 3.11+](https://www.python.org/) if you want Aura.

Then once:

```powershell
.\scripts\setup.ps1
```

If your Postgres password is not `12345`, edit `backend/OraEarn.Api/appsettings.Local.json`.

## Run (three terminals)

**1. Backend**

```powershell
cd backend
.\run.ps1
```

**2. Frontend**

```powershell
cd frontend
.\run.ps1
```

**3. Aura (optional)**

```powershell
cd ai-advisor
.\run.ps1
```

Then open **http://localhost:5173**

| Role | Email | Password |
|------|-------|----------|
| User | alice@oraearn.local | User123! |
| Admin | admin@oraearn.local | Admin123! |

## Config

| Service | File | When to edit |
|---------|------|----------------|
| Frontend | `frontend/.env` | Only if the API is not on port 5203 |
| Backend | `backend/OraEarn.Api/appsettings.Local.json` | Your Postgres host / password |
| Aura | none for local | Optional `AURA_API_KEY` in production |

No `.env` is needed in the backend.

## Tests

```powershell
dotnet test backend\OraEarn.Api.Tests\OraEarn.Api.Tests.csproj
```

## Production

Set on the API host: `ConnectionStrings__DefaultConnection`, `Jwt__Secret` (≥ 32 chars), `Cors__AllowedOrigins__0`, `Seed__DemoData=false`, `Seed__AdminEmail`, `Seed__AdminPassword`.

Build the frontend with your live API URLs:

```powershell
cd frontend
$env:VITE_API_URL="https://api.your-domain.com/api"
$env:VITE_HUB_URL="https://api.your-domain.com/hubs/shifts"
npm run build
```

Keep Aura and PostgreSQL off the public internet.

## More

- [backend/README.md](./backend/README.md)
- [frontend/README.md](./frontend/README.md)
- [ai-advisor/README.md](./ai-advisor/README.md)
- [docs/README.md](./docs/README.md)
