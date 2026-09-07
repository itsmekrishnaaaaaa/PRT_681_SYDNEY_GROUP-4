# Week 6 – Business Analyst Contribution
**Jira Backlog: PRT681_S2026_Group_4**
**Jenish Gurung – Business Analyst**

## Backlog Items (derived from OraEarn SRS/BRD)

### Epic: User Management
- As a user, I can register and log in so that I can access my account securely
- As a user, I can set up my profile with visa/work restriction details

### Epic: Job & Shift Tracking
- As a user, I can add an employer/job so that I can track where I work
- As a user, I can log a shift (start/end time) so that my pay is calculated automatically
- As a user, I can view an active shift session in real time

### Epic: Visa Hour Monitoring
- As a user, I can see my total hours worked in the current fortnight
- As a user, I receive an alert when I approach the 48-hour/fortnight visa limit

### Epic: Expense & Savings Tracking
- As a user, I can categorise and log expenses
- As a user, I can set a savings goal and track progress toward it

### Epic: Reporting & Analytics
- As a user, I can view a dashboard summarising pay, hours, and expenses
- As a user, I can view "Aura" AI advisor suggestions based on my activity

## Architecture Review Notes (from WebApplication3tierApp)
- Reviewed the cloned 3-tier sample app structure: Presentation Layer (UI/screens), Business Logic Layer (services/rules), Data Access Layer (repository/DB)
- Compared against OraEarn's planned architecture — confirmed our ERD entities (Application_User, Job, Shift, Active_Shift_Session, Expense_Category, Expense, Savings_Goal) map cleanly to a similar 3-tier structure
- No major gaps identified in BRD at this stage; validation rules (e.g. shift overlap checks, 48-hour limit) should be documented more explicitly in the next BRD revision
