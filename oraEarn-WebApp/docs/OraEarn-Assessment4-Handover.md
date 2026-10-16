# OraEarn — Assessment 4 handover

**Unit:** Software Project  
**Submission:** Week 12  
**Product:** OraEarn — shift, finance, and visa-compliance platform  
**Date:** October 2026

| Name | Student number | Role on this project |
|------|----------------|----------------------|
| Dharma Bahadur Budhathoki | S395172 | Developer, business analyst |
| Jenish Gurung | S395321 | Business analyst, report writer |
| Krishna Sapkota | S395795 | Database administrator, tester |
| Suraj Khadka | S394949 | Data analyst, database administrator |

Each section below names the person who owns that part. The next team should treat this file, plus `README.md`, as the start of a handover. The running software is the rest of the submission.

**Figures.** Where a heading says *insert screenshot*, paste a picture and write the comment on the picture itself (arrow plus a short sentence). Do not leave the placeholder in the submitted PDF.

---

## 1. URLs

**Written by:** Jenish Gurung (S395321)

These are the places a new developer needs. The Jira board is the task record the marker will open. Dharma’s developer cards are the ones labelled `app-dev` or assigned to him.

| What | URL | Who keeps it |
|------|-----|----------------|
| Git repository | https://github.com/itsmekrishnaaaaaa/PRT_681_SYDNEY_GROUP-4 | Whole team |
| Jira board (EARN) | https://students-team-dh8zehye.atlassian.net/jira/core/projects/EARN/board?filter=&groupBy=none | Dharma (developer cards), whole team |
| Learning materials — requirements | `docs/OraEarn-Requirements.md` in this repo | Jenish |
| Learning materials — proposal | `docs/OraEarn-Project-Proposal.md` | Jenish |
| Learning materials — normalisation | `docs/OraEarn-Normalization-1NF-to-3NF.md` | Krishna |
| ERD (Draw.io) | `docs/OraEarn-ERD-3NF.drawio` | Krishna, Suraj |
| This handover | `docs/OraEarn-Assessment4-Handover.md` | Named authors above |

**Timesheets (one file per member, in this repository)**

| Member | File |
|--------|------|
| Dharma Bahadur Budhathoki | `(s395172) Dharma_Bahadur_Budhathoki/Individual Timesheets - Dharma Bahadur Budhathoki(S395172) - weeks 4-12.xlsx` |
| Jenish Gurung | `Jenish_Gurung_s395321/IndividualTimesheet_JenishGurung_s395321.xlsx` |
| Krishna Sapkota | `Krishna_sapkota_S395795/Individual_timesheet_Krishna_S395795.xlsx` |
| Suraj Khadka | Group sheet `Group4_Timesheet_PRT681.xlsx` — no separate Suraj file was found |

**Running app (development only)**

| Service | URL |
|---------|-----|
| Frontend | http://localhost:5173 |
| API and Swagger | http://localhost:5203 and http://localhost:5203/swagger |
| Health check | http://localhost:5203/health |
| SignalR hub | http://localhost:5203/hubs/shifts |
| Aura (optional) | http://localhost:8000 |

Demo accounts after the API has seeded: `alice@oraearn.local` / `User123!` (student), `bob@oraearn.local` / `User123!` (citizen, no 48-hour rule), `admin@oraearn.local` / `Admin123!`.

*Insert screenshot.* Browser address bar on http://localhost:5173 after login, and a second shot of Swagger at `/swagger`. Comment on the picture: “The browser talks only to the API. Aura is not a public URL.”

---

## 2. Set-up instructions

A new developer can follow this from a clean Windows machine. Commands assume PowerShell and the repo root.

### 2.1 Tools and versions

**Written by:** Dharma Bahadur Budhathoki (S395172)

| Tool | Version used | Why it is here |
|------|----------------|----------------|
| .NET SDK | 10 (`net10.0`) | API, Identity, EF Core |
| Node.js | 20 or newer | React app |
| PostgreSQL | 16 or newer, port 5432 | The only database the API uses |
| Python | 3.11 or newer | Aura. The rest of the app runs without it |
| Git | current | Source control |
| Editor | Visual Studio 2026 or VS Code / Cursor | Either can build the API |

Check before setup:

```powershell
dotnet --version
node --version
python --version
```

*Insert screenshot.* The three version lines in a terminal. Comment: “These three tools must be on PATH before `scripts\setup.ps1`.”

### 2.2 Libraries and versions

**Written by:** Dharma Bahadur Budhathoki (S395172)

**API** (`backend/OraEarn.Api/OraEarn.Api.csproj`)

| Package | Version |
|---------|---------|
| Microsoft.AspNetCore.Authentication.JwtBearer | 10.0.11 |
| Microsoft.AspNetCore.Identity.EntityFrameworkCore | 10.0.11 |
| Microsoft.EntityFrameworkCore.Design | 10.0.11 |
| Npgsql.EntityFrameworkCore.PostgreSQL | 10.0.3 |
| Swashbuckle.AspNetCore | 8.1.4 |

**Tests** (`backend/OraEarn.Api.Tests`)

| Package | Version |
|---------|---------|
| xunit | 2.9.3 |
| xunit.runner.visualstudio | 3.1.4 |
| Microsoft.NET.Test.Sdk | 17.14.1 |
| Microsoft.AspNetCore.Mvc.Testing | 10.0.11 |

**Frontend** (`frontend/package.json`)

| Package | Version |
|---------|---------|
| react, react-dom | 19.2.x |
| vite | 8.2.x |
| typescript | 6.0.x |
| react-router-dom | 7.18.x |
| axios | 1.19.x |
| @microsoft/signalr | 10.0.x |
| tailwindcss | 4.3.x |
| jspdf, jspdf-autotable, xlsx | export only |
| lucide-react | icons |

**Aura** (`ai-advisor/requirements.txt`): FastAPI ≥ 0.115, Uvicorn ≥ 0.32, Pydantic ≥ 2.10.

Do not add a package by editing `package-lock.json` by hand. Use `npm install <name>` or `dotnet add package`.

### 2.3 Architecture

**Written by:** Dharma Bahadur Budhathoki (S395172)

Three processes. They are not one deployable.

```
Browser (React, Vite)
    →  ASP.NET Core API  :5203   JWT + REST + SignalR
           →  PostgreSQL
           →  Aura (Python FastAPI :8000)   optional
```

The React app never calls Python and never opens a database connection. `frontend/src/shared/api/client.ts` sends every request to `VITE_API_URL`.

**API layout** (one folder per feature, not a generic Controllers dump):

```
backend/OraEarn.Api/
  Program.cs                         host, middleware, hub
  Extensions/ServiceCollectionExtensions.cs   all DI registration
  Auth/  Jobs/  Shifts/  Expenses/  SavingsGoals/
  Analytics/  Notifications/  Admin/  VisaCompliance/  AiAdvisor/
  Data/          DbContext, migrations, seed
  Users/         ApplicationUser
```

Each feature folder holds the entity, DTOs, service, and controller. `Program.cs` stays thin.

**Frontend layout:**

```
frontend/src/
  auth/            login, register, route guards
  features/        one folder per screen (jobs, shifts, expenses, …)
  shared/api/      axios client
  shared/form-field/   client validation
  shared/ui/       buttons, dialogs, inputs
  App.tsx          routes
```

*Insert screenshot.* Solution Explorer expanded to `Auth`, `Shifts`, and `Data`, beside the `frontend/src/features` tree. Comment: “A feature lives in one backend folder and one frontend folder.”

### 2.4 Authentication and authorisation

**Written by:** Dharma Bahadur Budhathoki (S395172)

**Authentication** answers “who is this?”. **Authorisation** answers “what may they do?”.

1. `POST /api/auth/register` accepts a multipart form (personal details, visa fields, identity file). The account is stored as `Pending`. No tokens are issued until an admin approves it.
2. `POST /api/auth/login` checks email, password, and approval. Success returns an access token (15 minutes) and a refresh token (7 days).
3. The React client stores both in `localStorage` under the key `auth` and sends `Authorization: Bearer <access token>`.
4. On a 401, `client.ts` calls `POST /api/auth/refresh` once. Logout calls `POST /api/auth/logout`, which clears the refresh token on the user row.
5. Passwords use ASP.NET Identity: at least 8 characters, upper, lower, and a digit. Lockout is 5 failures, then 15 minutes. The database stores `PasswordHash`, never the typed password.
6. JWT is checked in `ServiceCollectionExtensions`: issuer `OraEarn`, audience `OraEarnClient`, HMAC secret of at least 32 characters. SignalR reads the same token from the `access_token` query on `/hubs/*`, because a WebSocket cannot set that header easily.

**Authorisation**

| Surface | Rule |
|---------|------|
| Almost every controller | `[Authorize]` — no token is 401 |
| `AdminController` | `[Authorize(Roles = "Admin")]` — a student token is 403 |
| React `ProtectedRoute` | Signed-out users go to `/login`. Admins are sent to `/admin` |
| React `AdminRoute` | Non-admins are sent to `/dashboard` |
| Data | Services filter by the user id inside the token. One student cannot read another student’s jobs, shifts, or pay |

Hiding a page is not security. A new endpoint still needs `[Authorize]`. Swagger will call whatever the API allows.

*Insert screenshot.* Swagger `POST /api/jobs` without a token, showing 401, and the same call with the Authorize lock filled in. Comment: “401 means no token. 403 means the token is valid but the role is wrong.”

### 2.5 Git download process

**Written by:** Jenish Gurung (S395321)

1. Install Git and sign in to the host that holds the remote.
2. Clone once:

```powershell
git clone [paste remote URL]
cd oraEarn
```

The git root is the parent of `oraEarn-WebApp`. Open `oraEarn-WebApp` in the editor.

3. Create a branch before any feature work. Do not commit on `main`.

```powershell
git checkout main
git pull
git checkout -b OE-<ticket>-short-name
```

4. Task names in the board and in the branch use the `OE-` prefix already used by this team.
5. Before a pull request: `git status`, then commit only the files for that task. Do not commit `appsettings.Local.json`, `.env`, `node_modules`, `bin`, `obj`, or `.venv`.

*Insert screenshot.* `git status` on a feature branch, with the branch name visible. Comment: “Branch name matches the board card.”

### 2.6 Instructions and commands to set up the database

**Written by:** Krishna Sapkota (S395795)

PostgreSQL must be running on port 5432 before the API starts. The Windows service name usually starts with `postgresql`.

The API does **not** need a manual `CREATE DATABASE` on a normal first run. On startup, `WebApplicationExtensions.EnsureDatabaseExistsAsync` connects to the `postgres` maintenance database, creates `oraearn` if it is missing, then applies EF Core migrations and seeds roles, payment methods, and (in Development) demo users.

If the service is stopped:

```powershell
Get-Service postgresql* | Start-Service
```

Default local login used by this repo: user `postgres`, password `12345`, database `oraearn`. If that password is wrong, change only `backend/OraEarn.Api/appsettings.Local.json` (see 2.8). Do not put a personal password into `appsettings.json`.

To confirm the database exists after the first API start, in `psql`:

```sql
\c oraearn
\dt
```

Expect Identity tables (`AspNetUsers`, `AspNetRoles`, …) plus `Jobs`, `Shifts`, `ActiveShiftSessions`, `ExpenseCategories`, `PaymentMethods`, `Expenses`, `SavingsGoals`, `UserNotificationReads`.

*Insert screenshot.* `\dt` output. Comment: “Shifts has no UserId column. The worker is reached through Jobs.”

### 2.7 Entity Framework commands

**Written by:** Krishna Sapkota (S395795)

Migrations already in the repo live under `backend/OraEarn.Api/Data/Migrations/`. The important one for the current shape is `NormalizeTo3NF`. Startup calls `Database.MigrateAsync()` when `Database:ApplyMigrations` is true (the default). A new developer does not run a migration just to launch the app.

Install the EF tool once if `dotnet ef` is missing:

```powershell
dotnet tool install --global dotnet-ef
```

From `backend/OraEarn.Api`, after a model change:

```powershell
dotnet ef migrations add <DescriptiveName> --project OraEarn.Api.csproj
dotnet ef database update --project OraEarn.Api.csproj
```

Useful checks:

```powershell
dotnet ef migrations list --project OraEarn.Api.csproj
```

Rules for a new migration:

- One migration per model change, named for the reason (`AddLeaveRequest`), not `Update` or `Fix`.
- Read the generated `Up` before you commit it.
- Delete behaviour is already set in `ApplicationDbContext`: delete user cascades jobs, categories, goals, and the live session; delete job is **restricted** while shifts exist; delete category is **restricted** while expenses exist.

Tests do not use PostgreSQL. The `Testing` environment uses EF in-memory (`UseInMemoryDatabase`), so `dotnet test` does not need a local server.

### 2.8 Connection strings

**Written by:** Krishna Sapkota (S395795)

Configuration is layered. Later files win.

| File | Committed? | What to put here |
|------|------------|------------------|
| `appsettings.json` | Yes | Empty JWT secret, empty CORS, demo seed off. Safe defaults |
| `appsettings.Development.json` | Yes | Local CORS for port 5173, development JWT secret, `Seed:DemoData = true` |
| `appsettings.Local.json` | No — copy from the example | Your Postgres host, port, user, password |
| Environment variables | On the server | Production connection string, JWT, CORS, seed |

`Program.cs` copies `appsettings.Local.json.example` to `appsettings.Local.json` on first run if the local file is missing. `scripts/setup.ps1` does the same.

Local shape:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=oraearn;Username=postgres;Password=12345"
  }
}
```

If `DefaultConnection` is missing, startup throws: `ConnectionStrings:DefaultConnection is required`.

**Production** (environment variables on the API host, not a committed file):

```text
ConnectionStrings__DefaultConnection=Host=...;Port=5432;Database=oraearn;Username=...;Password=...
Jwt__Secret=<at least 32 characters, not the DevOnly value>
Cors__AllowedOrigins__0=https://<your-frontend-host>
Seed__DemoData=false
Seed__AdminEmail=<admin email>
Seed__AdminPassword=<admin password>
```

Aura stays private. Set `AURA_API_KEY` and `AiAdvisor:ApiKey` together, and do not publish port 8000.

*Insert screenshot.* `appsettings.Local.json` with the password blurred. Comment: “This file is local only. Production uses environment variables.”

### 2.9 How to run the applications for the first time

**Written by:** Dharma Bahadur Budhathoki (S395172)

From the repo root, once:

```powershell
.\scripts\setup.ps1
```

That script checks `dotnet`, `node`, and `npm`, starts the PostgreSQL Windows service if it finds one, copies `.env` and `appsettings.Local.json` when missing, runs `npm install`, `dotnet restore`, and, if Python is installed, creates `ai-advisor/.venv`.

Then three terminals:

```powershell
cd backend
.\run.ps1
```

```powershell
cd frontend
.\run.ps1
```

```powershell
cd ai-advisor
.\run.ps1
```

Open http://localhost:5173. Sign in as Alice. Aura can be skipped; the chat then shows a fallback and shifts, expenses, and the visa bar still work.

`frontend/.env` only needs:

```text
VITE_API_URL=http://localhost:5203/api
VITE_HUB_URL=http://localhost:5203/hubs/shifts
```

Change it only if the API is not on port 5203.

*Insert screenshot.* Three terminals, with the API listening on 5203, Vite on 5173, and Uvicorn on 8000. Comment: “Start the API before the browser. Aura is the third terminal.”

### 2.10 Dependency injection

**Written by:** Dharma Bahadur Budhathoki (S395172)

`Program.cs` calls `builder.Services.AddApplicationServices(...)`. Every application service is registered there, as scoped (one instance per HTTP request):

`AuthService`, `TokenService`, `JobService`, `ShiftService`, `ActiveShiftSessionService`, `ShiftNotificationService`, `ExpenseCategoryService`, `ExpenseService`, `SavingsGoalService`, `SavingsGoalSyncService`, `SavingsGoalAnalyticsService`, `SavingsAdvisorService`, `AdminService`, `AnalyticsService`, `NotificationService`, `VisaComplianceService`, `RegistrationApprovalAdvisorService`, `RegistrationApprovalService`, `VisaDocumentStorageService`.

`ApplicationDbContext` is registered with `AddDbContext` (scoped). `PythonAdvisorClient` is registered with `AddHttpClient`, so it receives a managed `HttpClient` rather than `new HttpClient()`. Controllers take services in the constructor, for example `JobsController(JobService jobService)`. Do not `new` a service inside a controller.

A new service is invisible to the app until its `AddScoped` line exists. See section 3.

### 2.11 Exception handling

**Written by:** Dharma Bahadur Budhathoki (S395172)

Expected business failures are turned into HTTP responses in the controller. They are not left as unhandled 500s.

| Situation | What the code does | HTTP |
|-----------|--------------------|------|
| Bad input (empty title, rate ≤ 0, bad allocation) | Service throws `ArgumentException`. Controller returns `{ message }` | 400 |
| Visa fortnight would pass 48 hours | `VisaComplianceService` throws `InvalidOperationException`. `ShiftsController` returns that message | 400 |
| Row missing, or not this user’s row | Service returns null | 404 |
| No user id on the token | `UnauthorizedAccessException` | 401 |
| Aura is down | `PythonAdvisorClient` logs a warning and returns null. Chat still responds with a fallback | 200 from the API |
| Postgres is down at startup | `EnsureDatabaseExistsAsync` wraps `NpgsqlException` with a message that points at `appsettings.Local.json` | process stops |
| JWT secret missing, shorter than 32 chars, or still the dev placeholder outside Development | thrown while building services | process stops |
| CORS origins empty | thrown while building services | process stops |
| Any other exception in production | `UseExceptionHandler` plus `AddProblemDetails`. Development does not replace the developer exception page | 500 problem details |

The React form keeps the user on the dialog. `JobModal` copies the API `message` into the form error summary. A failed save does not close the dialog and does not clear the fields.

*Insert screenshot.* The shifts UI after a fortnight that would pass 48 hours, with the API message visible under the form. Comment: “The 48-hour refusal is a 400 from ShiftService, before any insert.”

### 2.12 Unit testing

**Written by:** Krishna Sapkota (S395795)

```powershell
dotnet test backend\OraEarn.Api.Tests\OraEarn.Api.Tests.csproj
```

Two styles, both xUnit:

| Folder | Style | What it locks |
|--------|--------|----------------|
| `Whitebox/` | Calls the class directly | `Shift.Calculate` (break, overnight, excessive break), savings progress and allocation, job title/rate validation, registration-review flags, analytics period maths |
| `Blackbox/` | `WebApplicationFactory` against the real pipeline, in-memory database | Login 401, login token, `GET /api/auth/me` with and without a token, jobs 401, create job, invalid rate 400 |

`OraEarnWebApplicationFactory` forces the `Testing` environment so no PostgreSQL and no demo seed are required. Black-box tests sign in as `test@oraearn.local` / `Test123!`.

**Gap the next team should close.** The 48-hour refusal is implemented in `VisaComplianceService` and caught in `ShiftsController`, and the live demo shows it, but there is no automated test that a student shift over 48 hours returns 400 and that a citizen shift does not. Add that test next to `ShiftCalculateTests` or as a black-box shifts test. Do not weaken the check to make a test pass.

*Insert screenshot.* Test Explorer, or the `dotnet test` summary, with the passed count visible. Comment: “White box covers the calculation. Black box covers 401 and a real POST.”

### 2.13 Class diagram and data model

**Written by:** Suraj Khadka (S394949), checked by Krishna Sapkota (S395795)

The model is in third normal form. A shift does not store the worker or the hourly rate. An expense does not store the worker or a payment-method string. Past `TotalHours` and `TotalEarned` stay on the shift so a later rate change does not rewrite history.

```
ApplicationUser 1 ── * Job 1 ── * Shift
        │                 │
        │                 └── * ActiveShiftSession   (UserId is unique: one live clock-in)
        ├── * ExpenseCategory 1 ── * Expense * ── 1 PaymentMethod
        ├── * SavingsGoal
        └── * UserNotificationRead     unique (UserId, NotificationKey)
```

| Class | Key columns | Notes |
|-------|-------------|--------|
| `ApplicationUser` | Identity id | Name, visa type/status/rights/expiry, approval, document path, refresh token |
| `Job` | `Id` | `UserId`, `Title`, `HourlyRate`, `DateOfJoin` |
| `Shift` | `Id` | `JobId`, times, break, `TotalHours`, `TotalEarned` |
| `ActiveShiftSession` | `Id`, unique `UserId` | Live clock-in, accumulated break |
| `ExpenseCategory` | `Id` | `UserId`, `Title` |
| `PaymentMethod` | `Id` | `Name` unique: Card, Cash, Bank Transfer, Other |
| `Expense` | `Id` | `CategoryId`, `PaymentMethodId`, amount, date, kind (Fixed, Variable, One-off) |
| `SavingsGoal` | `Id` | Target, current, manual amount, allocation %, optional target date |
| `UserNotificationRead` | `Id` | Which alert this user has marked read |

Visa documents are files on disk. The user row stores the path and the original file name, not the bytes.

Full walkthrough: `docs/OraEarn-Normalization-1NF-to-3NF.md`.

*Insert screenshot.* The Draw.io ERD, or the diagram above exported as an image. Comment on `Shift`: “No UserId. Owner is Job.UserId.” Comment on `Expense`: “Payment method is a foreign key, not free text.”

---

## 3. Adding a new feature — guide

**Written by:** Dharma Bahadur Budhathoki (S395172)  
**Task and commit steps written by:** Jenish Gurung (S395321)

Worked example: a **leave request** (date, reason, status). Copy this sequence for any new feature. Do the steps in this order.

### 3.1 Create the task

On the board, add a card `OE-### Leave requests`. Write the user-facing rule in one sentence (“A signed-in user can add, list, and delete their own leave requests”). Break it into sub-tasks: API, page, validation, test. Assign a branch owner. Everyone else can take a sub-task, but one person owns the card.

*Insert screenshot.* The card with the `OE-` name, checklist, and member. Comment: “No code until the card exists.”

### 3.2 Branch

```powershell
git checkout main
git pull
git checkout -b OE-###-leave-requests
```

### 3.3 Backend code

Add a folder `backend/OraEarn.Api/Leave/`:

| File | Job |
|------|-----|
| `LeaveRequest.cs` | Entity. `Id`, `UserId`, `Date`, `Reason`, navigation to `ApplicationUser` |
| `LeaveDtos.cs` | Request and response records |
| `LeaveService.cs` | All queries filter by the token user id. Throw `ArgumentException` for a bad reason or date |
| `LeaveController.cs` | `[Authorize]`, `[Route("api/[controller]")]`. Catch `ArgumentException` and return `BadRequest(new { message = ex.Message })` |

Register the set on `ApplicationDbContext`:

```csharp
public DbSet<LeaveRequest> LeaveRequests => Set<LeaveRequest>();
```

In `OnModelCreating`, index `UserId` and cascade-delete with the user, matching `SavingsGoal`.

### 3.4 Register the service (dependency injection)

In `ServiceCollectionExtensions.AddApplicationServices`, next to the other scoped services:

```csharp
services.AddScoped<LeaveService>();
```

No other file should construct `LeaveService`. The controller receives it as a constructor parameter.

### 3.5 Migration

From `backend/OraEarn.Api`:

```powershell
dotnet ef migrations add AddLeaveRequests --project OraEarn.Api.csproj
```

Read `Up`. Commit the migration with the feature. Restart the API so `MigrateAsync` applies it.

### 3.6 New .NET library

Only if the feature needs one. From `backend/OraEarn.Api`:

```powershell
dotnet add package <PackageName>
```

Commit the `csproj` change. Do not copy a DLL into the repo.

### 3.7 Frontend page and API

Add `frontend/src/features/leave/`:

| File | Job |
|------|-----|
| `api.ts` | `getLeave`, `createLeave`, `deleteLeave` using the shared `api` axios instance |
| `LeavePage.tsx` | List and empty state |
| `components/LeaveModal.tsx` | Form |

Register the route in `App.tsx` inside the existing `ProtectedRoute` + `Layout` block:

```tsx
<Route path="/leave" element={<LeavePage />} />
```

Add a nav item in `shared/layout/Layout.tsx` so the page is reachable. Follow `JobsPage` for loading and error text.

### 3.8 JavaScript library

From `frontend`, if the feature needs a package the app does not already have:

```powershell
npm install <package>
```

Commit `package.json` and `package-lock.json` together. Prefer a library already in the app (axios, the dialog in `shared/ui`, lucide-react) before adding a new one.

### 3.9 Styles

Do not add a new CSS framework. The app uses Tailwind 4. Put layout on the element with utilities already used elsewhere (`flex`, `gap-3`, `rounded-lg`, `bg-card`, `text-muted-foreground`). Colours come from `frontend/src/index.css` (`--primary`, `--destructive`, `--success`). Change those variables only when the whole product palette changes, not for one screen.

*Insert screenshot.* The new page on a narrow window and a desktop window. Comment: “Same page. The sidebar collapses; the bottom nav is the mobile path.”

### 3.10 Client-side validation

Use `shared/form-field`, as `JobModal` does. On submit, call `validate` with `pickErrors` and `validators`. Do not post if validation fails.

```tsx
validate(() =>
  pickErrors([
    ['reason', validators.required(form.reason, 'Reason is required.')],
    ['date', validators.dateRequired(form.date)],
  ]),
)
```

`FormField` renders the message under the input. `FormErrorSummary` shows the API message if the server still rejects the body (for example a rule only the server knows). Server checks stay in the service. The browser check is only so the user sees the error before the round trip.

*Insert screenshot.* The modal with the red message under the empty field, and the summary still showing a server message. Comment: “Field error is local. The summary is the API message.”

### 3.11 Unit test

Add one white-box test that the service throws on an empty reason, and one black-box test that `GET /api/leave` without a token returns 401 and that a signed-in user can create a row. Run:

```powershell
dotnet test backend\OraEarn.Api.Tests\OraEarn.Api.Tests.csproj
```

Fix a failing assertion by changing the feature or the test, whichever was wrong. Do not delete the test to go green.

### 3.12 Comment and commit

Comment only where the next reader would otherwise guess: a non-obvious visa rule, a delete restriction, a token quirk. Do not comment `// get the user`.

```powershell
git add backend/OraEarn.Api/Leave backend/OraEarn.Api/Data/Migrations frontend/src/features/leave frontend/src/App.tsx
git status
git commit -m "Add leave requests so a user can record time away from shifts."
git push -u origin OE-###-leave-requests
```

Open a pull request. The card moves to done only after the branch is reviewed and the test command above passes.

*Insert screenshot.* The commit on the host, message visible, files limited to the leave feature. Comment: “One card, one branch, one purpose in the message.”

---

## 4. Who did what

**Written by:** Jenish Gurung (S395321), confirmed by the team

This matches the roles used in the Assessment 3 presentation. Weekly hours belong in the timesheet links in section 1, not in a guessed total here.

| Person | Led | Also did |
|--------|-----|----------|
| Dharma Bahadur Budhathoki | The build: API structure, React app, auth, DI, run scripts | Business view of the screens; Scrum coordination |
| Jenish Gurung | Requirements, proposal, this handover’s task and git sections | Rules the features had to meet |
| Krishna Sapkota | Database to 3NF, migrations, xUnit suite | The 48-hour behaviour and the test gap called out in 2.12 |
| Suraj Khadka | Figures the product reports: earnings, expenses, net, the 7.5 of 48 bar | Data model review with Krishna |

A story had one owner. The other three took sub-tasks on that story so one person did not hold the whole week.

### Dharma — developer cards on the EARN board

**Written by:** Dharma Bahadur Budhathoki (S395172)

Board: https://students-team-dh8zehye.atlassian.net/jira/core/projects/EARN/board?filter=&groupBy=none

Finished developer work already in **Done** (from the board evidence in `(s395172) Dharma_Bahadur_Budhathoki/figures/`):

| Key | Work | What was delivered |
|-----|------|--------------------|
| EARN-105 | Login | Email and password sign-in, JWT access and refresh tokens, password visibility on the login form |
| EARN-106 | User dashboard | Personal analytics screen: net, earnings, expenses, period filter |
| EARN-107 | Jobs CRUD | Create, list, update, and delete jobs. Hourly rate is what shift pay uses |
| EARN-70 | Log a shift manually | Shift form, calendar, hours and pay calculated on the server |

These four cards are the developer work that is closed. Register (EARN-8), admin approval (EARN-12), Excel/PDF export (EARN-147), and Aura tests (EARN-164) were still outside Done on the saved board. Move a card to Done only after its description says what was built and a screenshot is attached.
