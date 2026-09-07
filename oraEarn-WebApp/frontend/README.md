# OraEarn frontend (React)

Standalone Vite + React app. It calls the .NET API at `http://localhost:5203`.

Start the **backend** in another terminal first.

## First time

```powershell
copy .env.example .env
npm install
```

## Run

```powershell
npm run dev
```

Or: `.\run.ps1`

Open http://localhost:5173

`.env` only has API URLs. You do not need a backend `.env`.
