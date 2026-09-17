# OraEarn — 5 Sprint Plan (Weeks 5–9)

OraEarn is delivered in **five one-week sprints**, matching semester **Weeks 5, 6, 7, 8 and 9**. Each sprint ends with a working, demoable slice — not just documents.

**Team:** 4 members (M1 Scrum Master & BA · M2 Backend · M3 Frontend · M4 Aura & QA).  
**Team approach:** iterative (same as proposal §10).  
**Stack:** ASP.NET Core API · React (Vite) · PostgreSQL · JWT · SignalR · Aura (Python)  
**Jira project:** `OE` (OraEarn) · Board type: **Scrum**

---

## Sprint map

| Uni week | Dates | Jira sprint | What you can demo at the end |
|----------|-------|-------------|------------------------------|
| **Week 5** | Mon 17 – Fri 21 Aug 2026 | **Sprint 1 — Foundation** | Register, login, JWT, roles, admin approve/reject, 3NF ERD |
| **Week 6** | Mon 24 – Fri 28 Aug 2026 | **Sprint 2 — Work & shifts** | Jobs, log shifts, clock in/out, calendar, live SignalR updates |
| **Week 7** | Mon 31 Aug – Fri 4 Sep 2026 | **Sprint 3 — Money** | Expense categories, expenses, payment methods, savings goals |
| **Week 8** | Mon 7 – Fri 11 Sep 2026 | **Sprint 4 — Compliance & insight** | 48-hour visa rule, analytics dashboard, Excel/PDF export |
| **Week 9** | Mon 14 – Fri 18 Sep 2026 | **Sprint 5 — Intelligence & handoff** | Notifications, Aura chat, admin AI review, docs & polish |

Paste-ready Jira sprint names and goals: [OraEarn-Jira-Sprints-Week5-to-Week9.md](./OraEarn-Jira-Sprints-Week5-to-Week9.md).

---

## Sprint 1 — Week 5 Foundation

**Goal:** A signed-in user and a normalized database. Nothing else ships until this works.

| Area | Work |
|------|------|
| Database | ERD, UNF→1NF→2NF→3NF, EF Core + PostgreSQL, Identity tables |
| Auth | Register, login, logout, JWT access + refresh tokens |
| Roles | Admin and User |
| Admin | Approve / reject pending registrations |
| Frontend | Login, register, protected routes, auth context |

**Done when:** Admin can approve Alice; Alice can log in with a JWT. Draw.io 3NF ERD exists.

**Requirements:** FR-1.1 to FR-1.7 (auth & roles). Visa *fields* on register are collected here; *enforcement* waits until Week 8.

**Jira stories:** OE-101 to OE-104.

---

## Sprint 2 — Week 6 Work & shifts

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

**Jira stories:** OE-201 to OE-204.

---

## Sprint 3 — Week 7 Money

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

**Jira stories:** OE-301 to OE-303.

---

## Sprint 4 — Week 8 Compliance & insight

**Goal:** Student visa safety and a dashboard the user can export.

| Area | Work |
|------|------|
| Visa | Rolling 14-day / 48-hour limit on shift create and update |
| Warnings | Near-limit and over-limit status on shifts + dashboard |
| Analytics | Period filters (7d / 30d / 12m), charts, net balance |
| Export | Excel and PDF of analytics |

**Done when:** A student visa user is blocked from saving a shift that would exceed 48 hours, and can download a PDF of last 7 days.

**Requirements:** FR-4.1–4.4, FR-7.x (analytics & export).

**Jira stories:** OE-401 to OE-403.

---

## Sprint 5 — Week 9 Intelligence & handoff

**Goal:** Advice, alerts, and assignment-ready documentation.

| Area | Work |
|------|------|
| Notifications | Bell inbox: visa, balance, savings reminders; mark as read |
| Aura | Python FastAPI advisor; chat on savings / visa |
| Admin AI | Registration document review suggestion |
| Polish | UI consistency, empty/error states, demo seed data |
| Docs | README, proposal, requirements, 1NF–3NF, this sprint plan |

**Done when:** Aura answers a savings question from live user data, and the report pack is complete.

**Requirements:** FR-8.x (notifications), FR-9.x (Aura / AI).

**Jira stories:** OE-501 to OE-505.

---

## What not to mix across sprints

- Do **not** start SignalR before JWT works (Week 6 depends on Week 5).
- Do **not** enforce visa hours before shifts exist (Week 8 depends on Week 6).
- Do **not** chat with Aura before expenses/goals exist (Week 9 depends on Week 7).
- Keep **docs and polish in Week 9** so earlier sprints stay demo-focused.

---

## Suggested demo accounts (after seed)

| Email | Password | Role | Use in demo |
|-------|----------|------|-------------|
| `admin@oraearn.local` | `Admin123!` | Admin | Approvals, platform analytics |
| `alice@oraearn.local` | `User123!` | User (student visa) | 48-hour limit |
| `bob@oraearn.local` | `User123!` | User (citizen) | Full work rights |

---

## Files to show markers per sprint

| Week | Evidence |
|------|----------|
| 5 | `docs/OraEarn-ERD-3NF.drawio`, `docs/OraEarn-Normalization-1NF-to-3NF.md`, Auth + Admin pages |
| 6 | Jobs page, Shifts calendar, `ShiftsHub` |
| 7 | Expenses + Savings pages, `PaymentMethods` |
| 8 | Analytics page, visa banner, export buttons |
| 9 | Notification bell, Aura chat, `docs/` pack |
