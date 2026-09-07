# OraEarn Database Normalization (UNF → 1NF → 2NF → 3NF)

This document shows how the OraEarn database was fully normalized, then applied in the ASP.NET Core application.

**Rule used:** a relation is in a higher normal form only if it already satisfies the lower ones.

| Form | Requirement |
|------|-------------|
| **UNF** | Nested / repeating groups allowed (spreadsheet style) |
| **1NF** | Atomic values only; no repeating groups; a primary key exists |
| **2NF** | 1NF + every non-key attribute depends on the **whole** primary key (no partial dependency) |
| **3NF** | 2NF + no non-key attribute depends on another non-key attribute (no transitive dependency) |

---

## 1. Unnormalized form (UNF)

A typical student timesheet before design — one row per worker, repeating jobs, shifts, and expenses in the same row.

| Email | Name | Visa | Jobs | ShiftDays | Hours | Pay | Categories | ExpenseAmounts | Payment |
|-------|------|------|------|-----------|-------|-----|------------|----------------|---------|
| alice@… | Alice Nguyen | Student / 500 / 48 hrs | Cafe Barista $28, Retail $26 | 2026-08-10 07:00–15:00, 2026-08-11 10:00–18:00 | 7.5, 7.25 | 210, 188.50 | Rent, Food | 950, 85.50 | Bank Transfer, Card |

**Problems**

- Repeating groups (`Jobs`, `ShiftDays`, `ExpenseAmounts`)
- Multi-valued cells (`Cafe Barista $28, Retail $26`)
- Insert / update / delete anomalies (changing Alice’s visa or a job rate requires rewriting the whole row)
- No reliable key, no referential integrity

---

## 2. First normal form (1NF)

**Changes**

- One fact per row
- Atomic columns only (split name, split job title from rate, split start/end time)
- Introduce a primary key on every table

### USER

| UserId (PK) | Email | FirstName | LastName | VisaType | VisaStatus | WorkingRights |
|-------------|-------|-----------|----------|----------|------------|---------------|
| U1 | alice@… | Alice | Nguyen | Subclass 500 | Student | Student |

### JOB

| JobId (PK) | UserId | Title | HourlyRate | DateOfJoin |
|------------|--------|-------|------------|------------|
| J1 | U1 | Cafe Barista | 28.00 | 2026-04-01 |
| J2 | U1 | Retail Assistant | 26.00 | 2026-06-01 |

### SHIFT (still **not** 3NF)

| ShiftId (PK) | UserId | JobId | WorkDate | StartTime | EndTime | BreakMinutes | TotalHours | TotalEarned | JobTitle | HourlyRate |
|--------------|--------|-------|----------|-----------|---------|--------------|------------|-------------|----------|------------|
| S1 | U1 | J1 | 2026-08-10 | 07:00 | 15:00 | 30 | 7.50 | 210.00 | Cafe Barista | 28.00 |
| S2 | U1 | J2 | 2026-08-11 | 10:00 | 18:00 | 45 | 7.25 | 188.50 | Retail Assistant | 26.00 |

### EXPENSE (still **not** 3NF)

| ExpenseId (PK) | UserId | CategoryTitle | Amount | Date | ExpenseKind | PaymentMethod |
|----------------|--------|---------------|--------|------|-------------|---------------|
| E1 | U1 | Rent | 950.00 | 2026-08-10 | Fixed | Bank Transfer |
| E2 | U1 | Food | 85.50 | 2026-08-11 | Variable | Card |

**1NF is satisfied:** every value is atomic, each row is unique.

**Still wrong**

- `JobTitle` and `HourlyRate` are copied onto every shift
- `CategoryTitle` is copied onto every expense
- `UserId` is stored on shift/expense even though the job/category already belongs to the user

---

## 3. Second normal form (2NF)

**Rule:** no partial dependency. Every non-key column must depend on the **entire** primary key.

OraEarn domain tables use a **single-column** primary key (`Id`). With a simple key there cannot be a partial dependency, so 1NF relations with simple keys are automatically in 2NF.

If we had used a composite key such as `(UserId, JobId, WorkDate)` on SHIFT, then `FirstName` or `VisaType` sitting on that table would depend only on `UserId` — a 2NF violation. That is why SHIFT has its own `Id` and does not carry user profile columns.

### 2NF tables (after removing copied job/category text)

**SHIFT**

| ShiftId (PK) | UserId | JobId | WorkDate | StartTime | EndTime | BreakMinutes | TotalHours | TotalEarned |
|--------------|--------|-------|----------|-----------|---------|--------------|------------|-------------|
| S1 | U1 | J1 | 2026-08-10 | 07:00 | 15:00 | 30 | 7.50 | 210.00 |

**EXPENSE**

| ExpenseId (PK) | UserId | CategoryId | Amount | Date | ExpenseKind | PaymentMethod |
|----------------|--------|------------|--------|------|-------------|---------------|
| E1 | U1 | C1 | 950.00 | 2026-08-10 | Fixed | Bank Transfer |

**EXPENSE_CATEGORY** (new)

| CategoryId (PK) | UserId | Title |
|-----------------|--------|-------|
| C1 | U1 | Rent |
| C2 | U1 | Food |

Job title and rate now live only on JOB. Category title lives only on EXPENSE_CATEGORY.

---

## 4. Third normal form (3NF)

**Rule:** no transitive dependency.  
If `A → B` and `B → C` and `B` is not a key, then `C` must not sit in the same table as `A`.

### Violations found in the 2NF design

| Table | Non-key attribute | Determined by | Why it is transitive |
|-------|-------------------|---------------|----------------------|
| SHIFT | `UserId` | `JobId` → JOB.`UserId` | Worker is already known from the job |
| EXPENSE | `UserId` | `CategoryId` → CATEGORY.`UserId` | Owner is already known from the category |
| EXPENSE | `PaymentMethod` (free text) | repeats the same name on many rows | Same method (Card, Cash…) is an independent entity |

`ACTIVE_SHIFT_SESSIONS.UserId` is **kept**. It is unique (one live clock-in per user), so `UserId` is a **candidate key**, not a transitive leftover.

`SHIFT.TotalHours` and `SHIFT.TotalEarned` are **kept** as recorded facts of a completed shift (payslip line). They freeze pay if the job rate changes later. They are not used as the source of job rate.

Visa fields stay on `AspNetUsers` because each depends directly on `UserId` (`UserId → VisaType`, `UserId → WorkingRights`). That is already 3NF. Splitting them would be a cohesion choice, not a 3NF requirement.

### 3NF functional dependencies (applied model)

```
AspNetUsers.Id            → Email, FirstName, LastName, visa + approval attributes
AspNetRoles.Id            → Name
AspNetUserRoles.(UserId, RoleId)  → ∅   (all-key relation)
Jobs.Id                   → UserId, Title, HourlyRate, DateOfJoin
Shifts.Id                 → JobId, WorkDate, StartTime, EndTime, BreakMinutes, TotalHours, TotalEarned
ActiveShiftSessions.Id    → UserId, JobId, ShiftDate, StartTime, break fields
ActiveShiftSessions.UserId → Id, JobId, …     (alternate key)
ExpenseCategories.Id      → UserId, Title
PaymentMethods.Id         → Name
Expenses.Id               → CategoryId, PaymentMethodId, Amount, Date, ExpenseKind
SavingsGoals.Id           → UserId, Title, TargetAmount, CurrentAmount, AllocationPercentage, TargetDate
UserNotificationReads.Id  → UserId, NotificationKey, ReadAt
UserNotificationReads.(UserId, NotificationKey) → ReadAt   (alternate key)
```

### Final 3NF relations (implemented)

**JOBS** — unchanged owner

| Id (PK) | UserId (FK) | Title | HourlyRate | DateOfJoin |
|---------|-------------|-------|------------|------------|

**SHIFTS** — `UserId` removed

| Id (PK) | JobId (FK) | WorkDate | StartTime | EndTime | BreakMinutes | TotalHours | TotalEarned |
|---------|------------|----------|-----------|---------|--------------|------------|-------------|

User’s shifts are found by: `SHIFT.JobId → JOB.UserId`.

**EXPENSE_CATEGORIES** — unchanged owner

| Id (PK) | UserId (FK) | Title |
|---------|-------------|-------|

**PAYMENT_METHODS** — new lookup

| Id (PK) | Name |
|---------|------|
| … | Card |
| … | Cash |
| … | Bank Transfer |
| … | Other |

**EXPENSES** — `UserId` removed; payment method is an FK

| Id (PK) | CategoryId (FK) | PaymentMethodId (FK) | Amount | Date | ExpenseKind |
|---------|-----------------|----------------------|--------|------|-------------|

User’s expenses are found by: `EXPENSE.CategoryId → CATEGORY.UserId`.

**ACTIVE_SHIFT_SESSIONS** — `UserId` retained (unique)

| Id (PK) | UserId (FK, unique) | JobId (FK) | ShiftDate | StartTime | BreakMinutesAccumulated | BreakStartedAt |
|---------|---------------------|------------|-----------|-----------|-------------------------|----------------|

**SAVINGS_GOALS**, **USER_NOTIFICATION_READS**, **ASP.NET Identity** tables are already 3NF (each non-key attribute depends only on the key).

---

## 5. How the application uses the 3NF model

| Old query | 3NF query |
|-----------|-----------|
| `Shifts.Where(s => s.UserId == userId)` | `Shifts.Where(s => s.Job.UserId == userId)` |
| `Expenses.Where(e => e.UserId == userId)` | `Expenses.Where(e => e.Category.UserId == userId)` |
| `Expense.PaymentMethod = "Card"` | `Expense.PaymentMethodId` → `PaymentMethods.Name` |

The HTTP API is unchanged for the React app: create/update expense still sends `paymentMethod: "Card"`. The API resolves that name to `PaymentMethodId`.  
`GET /api/expenses/payment-methods` reads the lookup table so the UI is driven by the normalized data.

Delete behaviour (already 3NF-safe)

- Delete user → cascade jobs, categories, goals, sessions
- Delete job → **restricted** while shifts exist
- Delete category → **restricted** while expenses exist

---

## 6. Worked anomaly check

**Update anomaly (fixed):** Alice’s cafe rate changes from $28 to $30. In UNF/1NF-with-copied-rate every past shift row had to be edited. In 3NF only `JOBS.HourlyRate` changes. Past `SHIFTS.TotalEarned` stays as the amount actually earned.

**Insert anomaly (fixed):** A payment method (e.g. PayID) can be inserted into `PAYMENT_METHODS` before any expense uses it.

**Delete anomaly (fixed):** Deleting Alice’s last “Food” expense does not delete the category row or the “Card” payment method.

---

## 7. Files

| File | Purpose |
|------|---------|
| `docs/OraEarn-Normalization-1NF-to-3NF.md` | This walkthrough |
| `docs/OraEarn-ERD-3NF.drawio` | Draw.io ERD **after** 3NF |

Open the draw.io file at [app.diagrams.net](https://app.diagrams.net) → **File → Open from → Device**.
