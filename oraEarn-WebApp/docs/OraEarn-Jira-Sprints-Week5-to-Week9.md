# OraEarn — Jira work for 4 members (Weeks 5–9)

Copy this into Jira. One Scrum project: **OE** (OraEarn). **Five sprints**. **Four people** on every sprint.

---

## 1. Put names on the four seats

Fill the **Name** column, then use that name as the Jira **Assignee**.

| Seat | Jira display name | Role | Owns |
|------|-------------------|------|------|
| **M1** | ________________ | Scrum Master & Business Analyst | Board, sprint goals, stories, AC, UAT, ERD, docs |
| **M2** | ________________ | Backend developer | ASP.NET API, EF/Postgres, JWT, SignalR hub, visa API |
| **M3** | ________________ | Frontend developer | React pages, calendar, charts, forms, Aura widget UI |
| **M4** | ________________ | Aura & QA | Python Aura, tests, seed data, export, admin AI review |

**Project settings → People:** add all four. Board columns:

`Backlog → To Do → In Progress → In Review → BA UAT → Done`

| Who | Moves tickets |
|-----|----------------|
| M2, M3, M4 | To Do → In Progress → In Review |
| M1 | In Review → BA UAT → Done; starts and completes each sprint |

**How to assign in Jira**

1. Create the **Story**. **Assignee** = the **Lead** in the tables below.
2. Under the story, **Add sub-task** for every other person who works on it.
3. Set **Story points** on the story only. Set **Original estimate** (hours) on sub-tasks.
4. Do not leave a story with no assignee. Do not put all tickets on one person.

---

## 2. Create the 5 sprints

| Sprint name (exact) | Dates | Sprint goal (paste) |
|---------------------|-------|---------------------|
| Sprint 1 — Week 5 Foundation | 17–21 Aug 2026 | Users can register and log in with JWT; admin can approve accounts; 3NF ERD is agreed. |
| Sprint 2 — Week 6 Work & shifts | 24–28 Aug 2026 | Users can manage jobs and shifts, clock in/out, and see live calendar updates. |
| Sprint 3 — Week 7 Money | 31 Aug – 4 Sep 2026 | Users can log expenses and savings goals against their own data. |
| Sprint 4 — Week 8 Compliance & insight | 7–11 Sep 2026 | Student visa 48-hour rule is enforced; analytics can be exported. |
| Sprint 5 — Week 9 Intelligence & handoff | 14–18 Sep 2026 | Notifications and Aura work; assignment docs are complete. |

---

## 3. Epics (create these 5)

**OE-1 Foundation** — Account, admin approval, 3NF database.  
**OE-2 Work & shifts** — Jobs, shifts, clock in/out, live calendar.  
**OE-3 Money** — Expenses, payment methods, savings goals.  
**OE-4 Compliance & insight** — 48-hour visa rule, analytics, export.  
**OE-5 Intelligence & handoff** — Notifications, Aura, docs, demo.

---

## 4. Who leads what (whole project)

Each member is **Lead** on a similar share of points. Sub-tasks still spread the hours.

| Seat | Lead stories | Lead pts | Typical sub-tasks |
|------|----------------|----------|-------------------|
| M1 | OE-104, OE-302, OE-504, OE-505 | 10 | UAT on every story; ceremonies |
| M2 | OE-102, OE-201, OE-203, OE-401, OE-503 | 21 | API work under M3/M4 stories |
| M3 | OE-101, OE-204, OE-301, OE-402, OE-501 | 25 | UI work under M2/M4 stories |
| M4 | OE-103, OE-202, OE-303, OE-403, OE-502 | 21 | Tests + Aura + export |

Hours every week: **each member 6.0h** (24h team capacity). Story points stay on the parent story.

---

## 5. Story template

```
As a <role>
I want <capability>
So that <benefit>

Acceptance criteria
- [ ] ...

BA notes
- Source: FR-x.x
- UAT by: M1
```

---

## Sprint 1 — Week 5 Foundation (17–21 Aug) · 16 pts

**Sprint goal:** Users can register and log in with JWT; admin can approve accounts; 3NF ERD is agreed.

| Key | Story | Pts | Lead (Assignee) | Hours M1 / M2 / M3 / M4 |
|-----|-------|----:|-----------------|-------------------------|
| OE-101 | Register account | 5 | **M3** | 1 / 2 / 2 / 1 |
| OE-102 | Login and JWT | 5 | **M2** | 1 / 2 / 2 / 1 |
| OE-103 | Admin approval | 3 | **M4** | 1 / 1 / 1 / 1 |
| OE-104 | 3NF database and ERD | 3 | **M1** | 3 / 1 / 0 / 1 |
| OE-110 | Sprint 1 ceremonies + extra tests | Task | **M1** | 0 / 0 / 1 / 2 |
| | **Week 5 hours** | | | **6 / 6 / 6 / 6** |

### OE-101 Register account — 5 pts — Epic OE-1 — Lead **M3**

```
As a new worker
I want to register with email, password, visa details and an ID document
So that admin can verify I am allowed to use OraEarn

Acceptance criteria
- [ ] Register form collects name, email, password, visa and ID upload
- [ ] Password rules: 8+ chars, upper, lower, digit
- [ ] New account status is Pending and user cannot use the app yet
- [ ] API returns a clear success message

BA notes
- Source: FR-1.1, FR-1.5
- UAT: register as David (pending) and confirm login is blocked
```

**Sub-tasks**

| Summary | Assignee | Estimate |
|---------|----------|----------|
| Register form, visa fields, ID upload | M3 | 2h |
| Register API + document storage | M2 | 2h |
| Password-rule tests; pending user cannot login | M4 | 1h |
| UAT as David pending | M1 | 1h |

### OE-102 Login and JWT — 5 pts — Epic OE-1 — Lead **M2**

```
As an approved user
I want to log in and stay signed in with JWT
So that I can use the app securely without sending my password on every request

Acceptance criteria
- [ ] Login with email and password returns access + refresh token
- [ ] Protected pages redirect to login if there is no token
- [ ] Logout invalidates the refresh token
- [ ] Token refresh works without a full re-login

BA notes — FR-1.2, FR-1.3, FR-1.4
```

**Sub-tasks**

| Summary | Assignee | Estimate |
|---------|----------|----------|
| TokenService, login/refresh/logout API | M2 | 2h |
| Login page, auth context, protected routes | M3 | 2h |
| API tests for refresh and logout | M4 | 1h |
| UAT login as Alice after approve | M1 | 1h |

### OE-103 Admin approval — 3 pts — Epic OE-1 — Lead **M4**

```
As an admin
I want to approve or reject pending registrations
So that only verified users enter the system

Acceptance criteria
- [ ] Admin sees a pending list
- [ ] Approve unlocks login
- [ ] Reject keeps the user out
- [ ] Non-admin cannot open admin pages

BA notes — FR-1.5, FR-1.7
```

**Sub-tasks**

| Summary | Assignee | Estimate |
|---------|----------|----------|
| Admin approve/reject API + role check | M2 | 1h |
| Admin pending list UI | M3 | 1h |
| Seed admin; black-box tests approve/reject | M4 | 1h |
| UAT as admin@oraearn.local | M1 | 1h |

### OE-104 3NF database and ERD — 3 pts — Epic OE-1 — Lead **M1**

```
As a BA
I want a 3NF ERD and matching database
So that markers can see the data model is normalized

Acceptance criteria
- [ ] UNF → 1NF → 2NF → 3NF document exists
- [ ] Draw.io 3NF ERD exists
- [ ] Shifts do not store UserId (owner via Job)
- [ ] Expenses do not store UserId (owner via Category)

BA notes — assignment ERD / normalization
```

**Sub-tasks**

| Summary | Assignee | Estimate |
|---------|----------|----------|
| UNF→3NF write-up + Draw.io | M1 | 3h |
| EF model + migrations match ERD | M2 | 1h |
| Review schema vs pages | M4 | 1h |

### OE-110 Sprint 1 ceremonies — Task — Lead **M1**

| Summary | Assignee | Estimate |
|---------|----------|----------|
| Plan, daily standup notes, review, retro | M1 | 1h (inside M1’s 6h with OE-104) |
| Auth integration test pass on demo seed | M4 | 2h |
| UI empty/error on login/register | M3 | 1h |

---

## Sprint 2 — Week 6 Work & shifts (24–28 Aug) · 18 pts

**Sprint goal:** Users can manage jobs and shifts, clock in/out, and see live calendar updates.

| Key | Story | Pts | Lead | Hours M1 / M2 / M3 / M4 |
|-----|-------|----:|------|-------------------------|
| OE-201 | Jobs CRUD | 3 | **M2** | 1 / 2 / 2 / 1 |
| OE-202 | Log and edit shifts | 5 | **M4** | 1 / 2 / 2 / 1 |
| OE-203 | Clock in and out | 5 | **M2** | 1 / 2 / 1 / 2 |
| OE-204 | Calendar and SignalR | 5 | **M3** | 1 / 1 / 2 / 1 |
| | **Week 6 hours (sub-tasks)** | | | **6 / 6 / 6 / 6** |

### OE-201 Jobs CRUD — 3 pts — Epic OE-2 — Lead **M2**

```
As a user
I want to add, edit and delete my jobs
So that each shift can use the correct hourly rate

Acceptance criteria
- [ ] Create job with title, rate, join date
- [ ] List only my jobs
- [ ] Update and delete work
- [ ] Rate is used in shift pay

BA notes — FR-2.1 to FR-2.5
```

**Sub-tasks:** M2 Jobs API 2h · M3 Jobs page 2h · M4 job tests 1h · M1 UAT 1h

### OE-202 Log and edit shifts — 5 pts — Epic OE-2 — Lead **M4**

```
As a user
I want to log a shift with date, start, end and break
So that hours and pay are calculated for me

Acceptance criteria
- [ ] Hours and pay calculate from the job rate
- [ ] Edit and delete a shift
- [ ] Start and end cannot be the same
- [ ] I only see my own shifts (via Job.UserId)

BA notes — FR-3.1 to FR-3.3, FR-3.10
```

**Sub-tasks:** M2 ShiftService calculate 2h · M3 shift form 2h · M4 pay/hours tests 1h · M1 UAT 1h

### OE-203 Clock in and out — 5 pts — Epic OE-2 — Lead **M2**

```
As a user
I want to clock in, take a break, and clock out
So that a live session becomes a completed shift

Acceptance criteria
- [ ] Only one live session per user
- [ ] Break minutes accumulate
- [ ] Clock out creates a shift

BA notes — FR-3.4 to FR-3.6
```

**Sub-tasks:** M2 live session API 2h · M3 clock-in bar 1h · M4 one-session tests 2h · M1 UAT 1h

### OE-204 Calendar and SignalR — 5 pts — Epic OE-2 — Lead **M3**

```
As a user
I want a monthly calendar that updates live
So that I see new shifts without refreshing

Acceptance criteria
- [ ] Green = completed day, red = not done
- [ ] Second tab updates when a shift is saved (SignalR)

BA notes — FR-3.7 to FR-3.9
```

**Sub-tasks:** M3 calendar UI 3h · M2 ShiftsHub 1h · M4 two-tab check 1h · M1 UAT 1h

---

## Sprint 3 — Week 7 Money (31 Aug – 4 Sep) · 12 pts

**Sprint goal:** Users can log expenses and savings goals against their own data.

| Key | Story | Pts | Lead | Hours M1 / M2 / M3 / M4 |
|-----|-------|----:|------|-------------------------|
| OE-301 | Categories and expenses | 5 | **M3** | 1 / 2 / 2 / 1 |
| OE-302 | Budget calculator | 2 | **M1** | 2 / 1 / 1 / 0 |
| OE-303 | Savings goals | 5 | **M4** | 1 / 2 / 2 / 3 |
| OE-310 | Sprint 3 ceremonies + 3NF payment methods | Task | **M2** | 2 / 1 / 1 / 2 |
| | **Use sub-tasks (6h each)** | | | **6 / 6 / 6 / 6** |

### OE-301 Expense categories and expenses — 5 pts — Epic OE-3 — Lead **M3**

```
As a user
I want categories and expenses with a payment method
So that I can see where my money goes

Acceptance criteria
- [ ] Create categories
- [ ] Add expense: amount, date, kind, payment method
- [ ] Payment methods load from GET /api/expenses/payment-methods
- [ ] Owner is Category.UserId (no UserId on expense)

BA notes — FR-5.1 to FR-5.6
```

**Sub-tasks:** M3 expenses UI 2h · M2 expense API + lookup table 2h · M4 3NF/owner tests 1h · M1 UAT Rent+Food 1h

### OE-302 Budget calculator — 2 pts — Epic OE-3 — Lead **M1**

```
As a user
I want to compare income and expenses
So that I know my savings rate

Acceptance criteria
- [ ] Enter monthly income
- [ ] See total spend, remaining, savings %

BA notes — FR-5.7
```

**Sub-tasks:** M1 AC + UAT script 2h · M2 totals API 1h · M3 calculator UI 1h

### OE-303 Savings goals — 5 pts — Epic OE-3 — Lead **M4**

```
As a user
I want savings goals with a target and allocation
So that I can track progress toward a target date

Acceptance criteria
- [ ] Create / update / delete / contribute
- [ ] Progress %, remaining, status
- [ ] Warn if total allocation > 100%

BA notes — FR-6.1 to FR-6.6
```

**Sub-tasks:** M4 allocation >100% tests + goal status 3h · M2 SavingsGoalService 2h · M3 goals page 2h · M1 UAT 1h  
(Hours beyond 6 sit in OE-310: M3 1h polish, M2 1h, M4 already 3h on this story so OE-310 M4 is seed goals 2h, M1 ceremonies 2h.)

---

## Sprint 4 — Week 8 Compliance & insight (7–11 Sep) · 13 pts

**Sprint goal:** Student visa 48-hour rule is enforced; analytics can be exported.

| Key | Story | Pts | Lead |
|-----|-------|----:|------|
| OE-401 | Visa 48-hour rule | 5 | **M2** |
| OE-402 | Analytics dashboard | 5 | **M3** |
| OE-403 | Export Excel and PDF | 3 | **M4** |

**Sub-task hours (6 each)**

| Story | M1 | M2 | M3 | M4 |
|-------|---:|---:|---:|---:|
| OE-401 | 1 UAT Alice vs Bob | 3 visa API | 1 warning banner | 1 limit tests |
| OE-402 | 1 UAT periods | 2 analytics API | 3 charts | 0 |
| OE-403 | 1 UAT files | 0 | 1 download buttons | 4 Excel/PDF |
| Ceremonies | 3 | 1 admin platform totals | 1 | 1 |
| **Total** | **6** | **6** | **6** | **6** |

### OE-401 Visa 48-hour rule — 5 pts — Epic OE-4 — Lead **M2**

```
As a student visa user
I want the system to block shifts over 48 hours in 14 days
So that I do not break my visa

Acceptance criteria
- [ ] Rolling 14-day window
- [ ] Create/update shift blocked if over 48 hours
- [ ] Citizens / full-time rights are not blocked
- [ ] Near-limit warning on shifts page

BA notes — FR-4.1 to FR-4.4
UAT: alice@oraearn.local blocked; bob@oraearn.local not blocked
```

### OE-402 Analytics dashboard — 5 pts — Epic OE-4 — Lead **M3**

```
As a user
I want a dashboard for 7 days, 1 month or 12 months
So that I can see earnings, spend and net balance

Acceptance criteria
- [ ] Period filter works
- [ ] Charts and category breakdown show
- [ ] Admin sees platform totals

BA notes — FR-7.1 to FR-7.6
```

### OE-403 Export Excel and PDF — 3 pts — Epic OE-4 — Lead **M4**

```
As a user
I want to download analytics as Excel and PDF
So that I can keep a record or show my adviser

Acceptance criteria
- [ ] Excel downloads
- [ ] PDF downloads
- [ ] File matches the selected period

BA notes — assignment export requirement
```

---

## Sprint 5 — Week 9 Intelligence & handoff (14–18 Sep) · 18 pts

**Sprint goal:** Notifications and Aura work on real user data. Documentation pack signed off by BA.

| Key | Story | Pts | Lead |
|-----|-------|----:|------|
| OE-501 | Notification inbox | 5 | **M3** |
| OE-502 | Aura AI advisor | 5 | **M4** |
| OE-503 | Admin AI registration review | 3 | **M2** |
| OE-504 | Assignment documentation pack | 3 | **M1** |
| OE-505 | Demo polish | 2 | **M1** |

**Sub-task hours (6 each)**

| Story | M1 | M2 | M3 | M4 |
|-------|---:|---:|---:|---:|
| OE-501 | 1 UAT bell | 2 notification API | 2 bell UI | 1 alert rules |
| OE-502 | 0.5 UAT chat | 1 .NET Aura client | 1 widget | 3 Python Aura |
| OE-503 | 0.5 UAT admin AI | 2 review API | 1 admin button | 1 summary flags |
| OE-504 | 3 docs pack | 0 | 0 | 0.5 README check |
| OE-505 | 1 demo script + review | 1 seed logins | 1 empty states | 0.5 Aura offline |
| **Total** | **6** | **6** | **6** | **6** |

### OE-501 Notification inbox — 5 pts — Epic OE-5 — Lead **M3**

```
As a user
I want a bell with visa, balance and savings alerts
So that I do not miss a compliance or money problem

Acceptance criteria
- [ ] Bell on every page
- [ ] Unread count on the icon
- [ ] Mark one as read
- [ ] Mark all as read
- [ ] Alerts cover visa, negative balance, savings

BA notes — FR-8.1 to FR-8.5
```

### OE-502 Aura AI advisor — 5 pts — Epic OE-5 — Lead **M4**

```
As a user
I want to chat with Aura about my hours and savings
So that I get advice based on my own data

Acceptance criteria
- [ ] Floating chat widget
- [ ] Uses visa, shifts, goals, balance
- [ ] Short practical replies
- [ ] App still works if Aura is offline

BA notes — FR-9.1 to FR-9.3
```

### OE-503 Admin AI registration review — 3 pts — Epic OE-5 — Lead **M2**

```
As an admin
I want an AI summary of a pending registration
So that I can approve faster with a documented reason

Acceptance criteria
- [ ] AI review button on pending user
- [ ] Summary and flags shown
- [ ] Admin can still approve or reject manually

BA notes — FR-1.6
```

### OE-504 Assignment documentation pack — 3 pts — Epic OE-5 — Lead **M1**

```
As a BA
I want the proposal, requirements, 3NF docs and sprint plan complete
So that the team can submit the assignment

Acceptance criteria
- [ ] README
- [ ] Proposal + requirements
- [ ] 1NF–3NF doc + 3NF draw.io
- [ ] Sprint plan + this Jira pack
- [ ] Seeded demo accounts documented
```

### OE-505 Sprint 5 demo polish — 2 pts — Epic OE-5 — Lead **M1**

```
As a Scrum Master
I want a clean demo path
So that the sprint review takes under 10 minutes

Acceptance criteria
- [ ] Demo script: admin → Alice visa → expenses → Aura
- [ ] Empty and error states checked
- [ ] Demo logins work
```

---

## 6. Comments to paste

**M1 after creating a story**

```
BA: AC added from FR-x.x. Lead is [M2/M3/M4]. Sub-tasks assigned. I will UAT when this is In Review.
```

**M1 when accepting**

```
BA UAT passed. Tested as alice@oraearn.local / User123!
AC ticked. Moving to Done.
```

**Daily standup (each member, 1 line)**

```
Yesterday: [ticket]  Today: [ticket]  Blockers: none
```

---

## 7. Timesheets — 6 hours per member per week

**Week commencing** as in the sprint table. Replace M1–M4 with real names.

### Week 5 (17–21 Aug) — Sprint 1

| Member | Mon | Tue | Wed | Thu | Fri | Tickets |
|--------|-----|-----|-----|-----|-----|---------|
| M1 | 1.5 plan sprint + epics | 1.5 AC OE-101/102 | 1.0 UAT pending | 1.0 ERD | 1.0 review/retro | OE-104, OE-110 |
| M2 | 1.5 Identity + migrations | 1.5 register API | 1.5 JWT | 1.0 admin API | 0.5 fix + 0.5 review | OE-102, OE-101 |
| M3 | 1.5 register form | 1.5 login + routes | 1.0 admin UI | 1.0 UAT support | 1.0 empty states | OE-101, OE-102 |
| M4 | 1.5 test project setup | 1.5 password/pending tests | 1.0 approve tests | 1.0 seed admin/Alice | 1.0 retro evidence | OE-103 |

### Week 6 (24–28 Aug) — Sprint 2

| Member | Mon | Tue | Wed | Thu | Fri | Tickets |
|--------|-----|-----|-----|-----|-----|---------|
| M1 | 1.5 start Sprint 2 | 1.5 AC jobs/shifts | 1.0 UAT clock-out | 1.0 UAT calendar | 1.0 review | OE-201–204 |
| M2 | 1.5 Jobs API | 1.5 ShiftService | 1.5 live session | 1.0 ShiftsHub | 0.5 fix + 0.5 review | OE-201, OE-203 |
| M3 | 1.5 jobs page | 1.5 shift form | 1.0 clock-in bar | 1.5 calendar | 0.5 review | OE-204 |
| M4 | 1.5 job tests | 1.5 pay/hours tests | 1.5 one-session tests | 1.0 two-tab SignalR | 0.5 review | OE-202 |

### Week 7 (31 Aug – 4 Sep) — Sprint 3

| Member | Mon | Tue | Wed | Thu | Fri | Tickets |
|--------|-----|-----|-----|-----|-----|---------|
| M1 | 1.5 start Sprint 3 | 1.5 AC expenses/goals | 1.0 UAT Rent+Food | 1.0 budget UAT | 1.0 review | OE-301, OE-302 |
| M2 | 1.5 expense API | 1.5 payment-methods lookup | 1.5 goals API | 1.0 budget totals | 0.5 fix + 0.5 review | OE-301, OE-303 |
| M3 | 1.5 expenses UI | 1.5 categories | 1.5 goals page | 1.0 calculator UI | 0.5 review | OE-301 |
| M4 | 1.5 owner/3NF tests | 1.5 allocation tests | 1.5 contribute/status | 1.0 seed expenses | 0.5 review | OE-303 |

### Week 8 (7–11 Sep) — Sprint 4

| Member | Mon | Tue | Wed | Thu | Fri | Tickets |
|--------|-----|-----|-----|-----|-----|---------|
| M1 | 1.5 start Sprint 4 | 1.5 AC visa rule | 1.0 UAT Alice block | 1.0 UAT export | 1.0 review | OE-401–403 |
| M2 | 1.5 visa window API | 1.5 block on save | 1.5 analytics API | 1.0 admin totals | 0.5 fix + 0.5 review | OE-401 |
| M3 | 1.5 visa banner | 1.5 dashboard | 1.5 charts | 1.0 download buttons | 0.5 review | OE-402 |
| M4 | 1.5 limit tests | 1.5 Bob vs Alice | 1.5 Excel | 1.0 PDF | 0.5 review | OE-403 |

### Week 9 (14–18 Sep) — Sprint 5

| Member | Mon | Tue | Wed | Thu | Fri | Tickets |
|--------|-----|-----|-----|-----|-----|---------|
| M1 | 1.5 start Sprint 5 | 1.5 AC Aura/bell | 1.0 UAT bell | 1.0 docs pack | 1.0 final demo | OE-504, OE-505 |
| M2 | 1.5 notification API | 1.5 Aura client | 1.5 admin AI API | 1.0 seed | 0.5 fix + 0.5 demo | OE-503 |
| M3 | 1.5 bell UI | 1.5 mark read | 1.5 Aura widget | 1.0 empty states | 0.5 demo | OE-501 |
| M4 | 1.5 Aura Python | 1.5 visa/savings prompts | 1.5 admin summary | 1.0 offline fallback | 0.5 demo | OE-502 |

Each person **30 hours** across weeks 5–9 (6h × 5).

---

## 8. Point totals

| Sprint | Week | Stories | Pts | Leads |
|--------|------|---------|----:|-------|
| 1 Foundation | 5 | OE-101–104 | 16 | M3, M2, M4, M1 |
| 2 Work & shifts | 6 | OE-201–204 | 18 | M2, M4, M2, M3 |
| 3 Money | 7 | OE-301–303 | 12 | M3, M1, M4 |
| 4 Compliance | 8 | OE-401–403 | 13 | M2, M3, M4 |
| 5 Intelligence | 9 | OE-501–505 | 18 | M3, M4, M2, M1, M1 |
| **Total** | | **20 stories** | **77** | Four members every sprint |

Demo logins: `alice@oraearn.local` / `User123!` · `admin@oraearn.local` / `Admin123!`
