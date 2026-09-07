# OraEarn — Jira copy pack + Week 5 timesheet

**Your roles on this project**

| Role | Share of your work | What you write in Jira |
|------|--------------------|------------------------|
| **Business Analyst (main)** | Stories, acceptance criteria, UAT, sign-off | Description + AC + “Accepted” comment |
| **Scrum Master** | Sprint board, ceremonies, impediments | Sprint goal, standup notes, retro actions |
| **Developer (1 on the team)** | You support the one developer; you do not own all code | Review comments, small tasks assigned to you |

Create **one Jira project**: `OE` (OraEarn). Board type: **Scrum**. 5 sprints.

---

## 1. What to create first in Jira

**Project settings → People**

- You: Business Analyst, Scrum Master  
- Other member: Developer  

**5 sprints (create in order)**

| Sprint name | Goal (paste into Sprint goal) |
|-------------|-------------------------------|
| Sprint 1 — Foundation | Users can register and log in with JWT; admin can approve accounts; 3NF ERD is agreed. |
| Sprint 2 — Work & shifts | Users can manage jobs and shifts, clock in/out, and see live calendar updates. |
| Sprint 3 — Money | Users can log expenses and savings goals against their own data. |
| Sprint 4 — Compliance & insight | Student visa 48-hour rule is enforced; analytics can be exported. |
| Sprint 5 — Intelligence & handoff | Notifications and Aura work; assignment docs are complete. |

---

## 2. Epics (create these 5)

Paste the **Epic Name** and **Description** into Jira.

### Epic OE-1 — Foundation
Users can create an account, wait for admin approval, and sign in securely. Database is designed in 3NF.

### Epic OE-2 — Work & shifts
Users can add jobs, log shifts, clock in/out, and see the calendar update in real time.

### Epic OE-3 — Money
Users can record expenses and savings goals. Payment methods come from a lookup table (3NF).

### Epic OE-4 — Compliance & insight
Student visa hours are limited to 48 per fortnight. Users can view and export analytics.

### Epic OE-5 — Intelligence & handoff
Users get alerts and Aura advice. The team hands over docs and a polished demo.

---

## 3. User stories to paste (all sprints)

Use issue type **Story**. Set **Epic link**, **Sprint**, **Story points**, **Assignee**.

**Story template (use for every ticket)**

```
As a <role>
I want <capability>
So that <benefit>

Acceptance criteria
- [ ] ...
- [ ] ...

BA notes
- Source: FR-x.x
- UAT by: Business Analyst
```

---

### Sprint 1 stories

**OE-101 Register account** — points 5 — Epic OE-1 — Assignee: Developer  
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

**OE-102 Login and JWT** — points 5 — Epic OE-1 — Assignee: Developer  
```
As an approved user
I want to log in and stay signed in with JWT
So that I can use the app securely without sending my password on every request

Acceptance criteria
- [ ] Login with email and password returns access + refresh token
- [ ] Protected pages redirect to login if there is no token
- [ ] Logout invalidates the refresh token
- [ ] Token refresh works without a full re-login

BA notes
- Source: FR-1.2, FR-1.3, FR-1.4
```

**OE-103 Admin approval** — points 3 — Epic OE-1 — Assignee: Developer  
```
As an admin
I want to approve or reject pending registrations
So that only verified users enter the system

Acceptance criteria
- [ ] Admin sees a pending list
- [ ] Approve unlocks login
- [ ] Reject keeps the user out
- [ ] Non-admin cannot open admin pages

BA notes
- Source: FR-1.5, FR-1.7
```

**OE-104 3NF database and ERD** — points 3 — Epic OE-1 — Assignee: You (BA) + Developer  
```
As a BA
I want a 3NF ERD and matching database
So that markers can see the data model is normalized

Acceptance criteria
- [ ] UNF → 1NF → 2NF → 3NF document exists
- [ ] Draw.io 3NF ERD exists
- [ ] Shifts do not store UserId (owner via Job)
- [ ] Expenses do not store UserId (owner via Category)

BA notes
- Source: assignment ERD / normalization
```

---

### Sprint 2 stories

**OE-201 Jobs CRUD** — points 3 — Epic OE-2  
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

**OE-202 Log and edit shifts** — points 5 — Epic OE-2  
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

**OE-203 Clock in and out** — points 5 — Epic OE-2  
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

**OE-204 Calendar and SignalR** — points 5 — Epic OE-2  
```
As a user
I want a monthly calendar that updates live
So that I see new shifts without refreshing

Acceptance criteria
- [ ] Green = completed day, red = not done
- [ ] Second tab updates when a shift is saved (SignalR)

BA notes — FR-3.7 to FR-3.9
```

---

### Sprint 3 stories

**OE-301 Expense categories and expenses** — points 5 — Epic OE-3  
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

**OE-302 Budget calculator** — points 2 — Epic OE-3  
```
As a user
I want to compare income and expenses
So that I know my savings rate

Acceptance criteria
- [ ] Enter monthly income
- [ ] See total spend, remaining, savings %

BA notes — FR-5.7
```

**OE-303 Savings goals** — points 5 — Epic OE-3  
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

---

### Sprint 4 stories

**OE-401 Visa 48-hour rule** — points 5 — Epic OE-4  
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
UAT account: alice@shifttracker.local
```

**OE-402 Analytics dashboard** — points 5 — Epic OE-4  
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

**OE-403 Export Excel and PDF** — points 3 — Epic OE-4  
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

### Sprint 5 stories (this is Week 5 — create these now)

**OE-501 Notification inbox** — points 5 — Epic OE-5 — Assignee: Developer  
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
UAT by BA in Week 5
```

**OE-502 Aura AI advisor** — points 5 — Epic OE-5 — Assignee: Developer  
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

**OE-503 Admin AI registration review** — points 3 — Epic OE-5  
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

**OE-504 Assignment documentation pack** — points 3 — Epic OE-5 — Assignee: You (BA)  
```
As a BA
I want the proposal, requirements, 3NF docs and sprint plan complete
So that the team can submit the assignment

Acceptance criteria
- [ ] README and COPYING
- [ ] Proposal + requirements
- [ ] 1NF–3NF doc + 3NF draw.io
- [ ] Sprint plan
- [ ] Seeded demo accounts documented

BA notes — Sprint 5 handoff
```

**OE-505 Sprint 5 demo polish** — points 2 — Epic OE-5  
```
As a Scrum Master
I want a clean demo path
So that the sprint review takes under 10 minutes

Acceptance criteria
- [ ] Demo script: admin → Alice visa → expenses → Aura
- [ ] Empty and error states checked
- [ ] Demo logins work

BA / SM notes — review on Friday Week 5
```

---

## 4. What YOU write on tickets (copy these comments)

**As Business Analyst — after creating a story**

```
BA: Acceptance criteria added from FR-x.x.
Ready for development. I will UAT on the demo accounts after the developer moves this to In Review.
```

**As Business Analyst — when you accept**

```
BA UAT passed.
Tested as alice@shifttracker.local / User123!
AC ticked. Moving to Done.
```

**As Scrum Master — daily standup (paste in Sprint comment or Confluence)**

```
Yesterday: [your work]
Today: [your work]
Blockers: none / [name the blocker]
Developer focus: [ticket key]
```

**As Scrum Master — Sprint 5 goal (already in the sprint)**

```
Notifications and Aura work on real user data. Documentation pack signed off by BA.
```

---

## 5. Individual timesheet — Week 5 (6 hours)

**Person:** [Your name]  
**Main role:** Business Analyst  
**Other roles:** Scrum Master; supporting Developer  
**Week:** Week 5 (Sprint 5 — Intelligence & handoff)  
**Week commencing:** Monday 17 August 2026  
**Total hours:** 6.0

| Date | Hours | Role | Jira | What you did |
|------|------:|------|------|----------------|
| Mon 17 Aug | 1.5 | Scrum Master | Sprint 5 | Planned Sprint 5, set sprint goal, pulled OE-501–505 onto the board, confirmed developer capacity |
| Tue 18 Aug | 1.5 | Business Analyst | OE-501, OE-502 | Wrote / refined AC for notifications and Aura; mapped FR-8 and FR-9; clarified offline Aura behaviour |
| Wed 19 Aug | 1.0 | Business Analyst | OE-501 | UAT on notification bell: unread count, mark one / mark all, visa + savings alerts as Alice |
| Thu 20 Aug | 1.0 | Developer (support) | OE-502, OE-503 | Reviewed Aura chat and admin AI review with the developer; logged review comments; checked API still works if Aura is down |
| Fri 21 Aug | 1.0 | BA + Scrum Master | OE-504, OE-505 | Signed off docs pack (3NF ERD, sprint plan); ran sprint review demo path; wrote retro notes |

**Hours by role (Week 5)**

| Role | Hours | % |
|------|------:|--:|
| Business Analyst | 3.0 | 50% |
| Scrum Master | 2.0 | 33% |
| Developer (support) | 1.0 | 17% |
| **Total** | **6.0** | **100%** |

Mon 1.5 SM + Tue 1.5 BA + Wed 1.0 BA + Thu 1.0 Dev + Fri 0.5 BA + 0.5 SM = 6.0  
(Friday 1.0 is split 0.5 BA / 0.5 SM in the role table.)

**What not to put on the timesheet:** coding the whole feature yourself (that is the Developer’s 1 role). Your 1.0h Thursday is review/support only.

---

## 6. Jira board columns

`Backlog → To Do → In Progress → In Review → BA UAT → Done`

You move tickets **In Review → BA UAT → Done**.  
Developer moves **To Do → In Progress → In Review**.  
You (SM) start/complete the sprint.

---

## 7. Week 5 standup lines (use Mon–Fri)

**Mon:** Yesterday finished Sprint 4 review. Today I open Sprint 5 and lock OE-501–505. No blockers.  
**Tue:** Yesterday board was ready. Today I finish AC for Aura and notifications. No blockers.  
**Wed:** Yesterday AC signed. Today I UAT the bell as Alice. Waiting on Developer if mark-all is not on In Review.  
**Thu:** Yesterday UAT on OE-501. Today I review Aura with Developer. No blockers.  
**Fri:** Yesterday Aura reviewed. Today sprint review + docs sign-off. No blockers.
