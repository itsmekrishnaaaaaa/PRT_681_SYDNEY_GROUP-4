# OraEarn — 5 Sprint Plan

OraEarn is delivered in **five sprints**. Each sprint ends with a working, demoable slice — not just documents.

**Team approach:** iterative (same as proposal §10).  
**Stack:** ASP.NET Core API · React (Vite) · PostgreSQL · JWT · SignalR · Aura (Python)

---

## Sprint map

| Sprint | Name | What you can demo at the end |
|--------|------|------------------------------|
| **1** | Foundation | Register, login, JWT, roles, admin approve/reject, 3NF ERD |
| **2** | Work & shifts | Jobs, log shifts, clock in/out, calendar, live SignalR updates |
| **3** | Money | Expense categories, expenses, payment methods, savings goals |
| **4** | Compliance & insight | 48-hour visa rule, analytics dashboard, Excel/PDF export |
| **5** | Intelligence & handoff | Notifications, Aura chat, admin AI review, docs & polish |

---

## Sprint 1 — Foundation

**Goal:** A signed-in user and a normalized database. Nothing else ships until this works.

| Area | Work |
|------|------|
| Database | ERD, UNF→1NF→2NF→3NF, EF Core + PostgreSQL, Identity tables |
| Auth | Register, login, logout, JWT access + refresh tokens |
| Roles | Admin and User |
| Admin | Approve / reject pending registrations |
| Frontend | Login, register, protected routes, auth context |

**Done when:** Admin can approve Alice; Alice can log in with a JWT. Draw.io 3NF ERD exists.

**Requirements:** FR-1.1 to FR-1.7 (auth & roles). Visa *fields* on register are collected here; *enforcement* waits until Sprint 4.

---

## Sprint 2 — Work & shifts

**Goal:** Track time and pay for one or more jobs.

| Area | Work |
|------|------|
| Jobs | CRUD (title, hourly rate, date of join) |
| Shifts | Manual log, edit, delete; hours and pay calculated from job rate |
| Live session | Clock in, break, clock out → completed shift |
| Calendar | Month view; completed vs upcoming days |
| Real-time | SignalR hub `/hubs/shifts` refreshes other open tabs |

**Done when:** User clocks out and the calendar updates live on a second browser tab.

**Requirements:** FR-2.1–2.5, FR-3.1–3.10.

**3NF note:** Shifts store `JobId` only. Owner is `Job.UserId`.

---

## Sprint 3 — Money

**Goal:** Track spending and savings against the same user.

| Area | Work |
|------|------|
| Categories | User-owned expense categories |
| Expenses | Amount, date, kind (Fixed / Variable / One-off), payment method |
| Lookup | `PAYMENT_METHODS` table (3NF); UI loads methods from the API |
| Budget | Income vs spend calculator |
| Savings | Goals, allocation %, contribute, progress |

**Done when:** User adds Rent + Food, sees a category total, and a savings goal with progress.

**Requirements:** FR-5.1–5.7, FR-6.1–6.6.

**3NF note:** Expenses store `CategoryId` + `PaymentMethodId` only. Owner is `Category.UserId`.

---

## Sprint 4 — Compliance & insight

**Goal:** Student visa safety and a dashboard the user can export.

| Area | Work |
|------|------|
| Visa | Rolling 14-day / 48-hour limit on shift create and update |
| Warnings | Near-limit and over-limit status on shifts + dashboard |
| Analytics | Period filters (7d / 30d / 12m), charts, net balance |
| Export | Excel and PDF of analytics |

**Done when:** A student visa user is blocked from saving a shift that would exceed 48 hours, and can download a PDF of last 7 days.

**Requirements:** FR-4.1–4.4, FR-7.x (analytics & export).

---

## Sprint 5 — Intelligence & handoff

**Goal:** Advice, alerts, and assignment-ready documentation.

| Area | Work |
|------|------|
| Notifications | Bell inbox: visa, balance, savings reminders; mark as read |
| Aura | Python FastAPI advisor; chat on savings / visa |
| Admin AI | Registration document review suggestion |
| Polish | UI consistency, empty/error states, demo seed data |
| Docs | README, COPYING, proposal, requirements, 1NF–3NF, this sprint plan |

**Done when:** Aura answers a savings question from live user data, and the report pack is complete.

**Requirements:** FR-8.x (notifications), FR-9.x (Aura / AI).

---

## What not to mix across sprints

- Do **not** start SignalR before JWT works (Sprint 2 depends on Sprint 1).
- Do **not** enforce visa hours before shifts exist (Sprint 4 depends on Sprint 2).
- Do **not** chat with Aura before expenses/goals exist (Sprint 5 depends on Sprint 3).
- Keep **docs and polish in Sprint 5** so earlier sprints stay demo-focused.

---

## Suggested demo accounts (after seed)

| Email | Password | Role | Use in demo |
|-------|----------|------|-------------|
| `admin@shifttracker.local` | `Admin123!` | Admin | Approvals, platform analytics |
| `alice@shifttracker.local` | `User123!` | User (student visa) | 48-hour limit |
| `bob@shifttracker.local` | `User123!` | User (citizen) | Full work rights |

---

## Files to show markers per sprint

| Sprint | Evidence |
|--------|----------|
| 1 | `docs/OraEarn-ERD-3NF.drawio`, `docs/OraEarn-Normalization-1NF-to-3NF.md`, Auth + Admin pages |
| 2 | Jobs page, Shifts calendar, `ShiftsHub` |
| 3 | Expenses + Savings pages, `PaymentMethods` |
| 4 | Analytics page, visa banner, export buttons |
| 5 | Notification bell, Aura chat, `docs/` pack |
