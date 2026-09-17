# OraEarn — Functional & Non-Functional Requirements

**System Name:** OraEarn  
**Type:** Web application for hourly workers (especially international students in Australia)  
**Version:** 1.0 (Initial)

---

## 1. Functional Requirements

### 1.1 Authentication & User Management

| ID | Requirement |
|----|-------------|
| FR-1.1 | Users shall register with personal details, visa/work-rights information, and identity document upload. |
| FR-1.2 | Users shall log in with email and password. |
| FR-1.3 | The system shall issue JWT access and refresh tokens and support token refresh. |
| FR-1.4 | Users shall log out and invalidate refresh tokens. |
| FR-1.5 | Admin shall approve or reject pending user registrations. |
| FR-1.6 | Admin shall view visa documents and run AI-assisted registration review. |
| FR-1.7 | The system shall support Admin and User roles. |

### 1.2 Jobs Management

| ID | Requirement |
|----|-------------|
| FR-2.1 | Users shall create jobs with title, hourly rate, and start date. |
| FR-2.2 | Users shall view all their jobs. |
| FR-2.3 | Users shall update job details. |
| FR-2.4 | Users shall delete jobs. |
| FR-2.5 | Hourly rate from a job shall be used to calculate shift earnings. |

### 1.3 Shift Tracking

| ID | Requirement |
|----|-------------|
| FR-3.1 | Users shall log shifts manually (date, start/end time, break minutes, job). |
| FR-3.2 | Users shall edit and delete existing shifts. |
| FR-3.3 | The system shall auto-calculate total hours and pay per shift. |
| FR-3.4 | Users shall clock in to a live shift session linked to a job. |
| FR-3.5 | Users shall start and end breaks during a live session. |
| FR-3.6 | Users shall clock out to convert a live session into a completed shift. |
| FR-3.7 | Users shall view shifts on a monthly calendar. |
| FR-3.8 | The calendar shall show completed shift days in green and incomplete/upcoming days in red. |
| FR-3.9 | The system shall push real-time shift updates via SignalR (WebSocket). |
| FR-3.10 | Users shall view a monthly shift summary (count, hours, pay). |

### 1.4 Visa Compliance

| ID | Requirement |
|----|-------------|
| FR-4.1 | The system shall track student visa users' hours in a rolling 14-day fortnight. |
| FR-4.2 | The system shall enforce the 48-hour per fortnight work limit on shift create/update. |
| FR-4.3 | The system shall warn when a user is near or over the visa hour limit. |
| FR-4.4 | Visa compliance status shall appear on the analytics and shifts pages. |

### 1.5 Expense Management

| ID | Requirement |
|----|-------------|
| FR-5.1 | Users shall create expense categories (e.g. Rent, Food, Transport). |
| FR-5.2 | Users shall add expenses with amount, date, category, type, and payment method. |
| FR-5.3 | Users shall edit and delete expenses. |
| FR-5.4 | Users shall filter expenses by category. |
| FR-5.5 | The system shall classify expenses as Fixed, Variable, or One-off. |
| FR-5.6 | Users shall view expense summary and spending by category. |
| FR-5.7 | The system shall provide a budget calculator (income vs expenses, savings rate). |

### 1.6 Savings Goals

| ID | Requirement |
|----|-------------|
| FR-6.1 | Users shall create savings goals with target amount, allocation percentage, and optional target date. |
| FR-6.2 | Users shall contribute manually to goals. |
| FR-6.3 | Users shall update and delete goals. |
| FR-6.4 | The system shall calculate progress percentage, remaining amount, and goal status. |
| FR-6.5 | The system shall estimate hours needed and projected finish date per goal. |
| FR-6.6 | The system shall warn if total allocation exceeds 100% of income. |

### 1.7 Analytics & Reporting

| ID | Requirement |
|----|-------------|
| FR-7.1 | Users shall view a personal analytics dashboard. |
| FR-7.2 | Users shall filter analytics by 7 days, 1 month, or 12 months. |
| FR-7.3 | The dashboard shall show net balance, earnings, expenses, and savings totals. |
| FR-7.4 | The dashboard shall show shift and expense charts by time period. |
| FR-7.5 | The dashboard shall show spending breakdown by category and expense type. |
| FR-7.6 | Admin shall view platform-wide analytics across all users. |

### 1.8 Notifications

| ID | Requirement |
|----|-------------|
| FR-8.1 | The system shall generate alerts for visa limits, negative balance, and savings issues. |
| FR-8.2 | Users shall view notifications from a global bell icon on all pages. |
| FR-8.3 | Users shall mark individual notifications as read. |
| FR-8.4 | Users shall mark all notifications as read. |
| FR-8.5 | Unread count shall be displayed on the notification bell. |

### 1.9 AI Advisor (Aura)

| ID | Requirement |
|----|-------------|
| FR-9.1 | Users shall chat with the Aura AI advisor via a floating widget. |
| FR-9.2 | Aura shall use user context (visa, shifts, goals, balance) to give relevant answers. |
| FR-9.3 | Aura shall provide short, practical advice on hours, savings, and compliance. |

---

## 2. Non-Functional Requirements

### 2.1 Performance

| ID | Requirement |
|----|-------------|
| NFR-1.1 | API responses for standard CRUD operations shall complete within 2 seconds under normal load. |
| NFR-1.2 | Dashboard analytics shall load within 3 seconds. |
| NFR-1.3 | Real-time shift updates shall propagate within 1 second via WebSocket. |
| NFR-1.4 | The frontend shall use efficient data fetching to avoid unnecessary API calls. |

### 2.2 Security

| ID | Requirement |
|----|-------------|
| NFR-2.1 | All user APIs shall require JWT authentication (except login/register). |
| NFR-2.2 | Passwords shall meet complexity rules (minimum 8 characters, upper, lower, digit). |
| NFR-2.3 | Users shall only access their own data (row-level isolation by UserId). |
| NFR-2.4 | Admin endpoints shall be restricted to the Admin role. |
| NFR-2.5 | Refresh tokens shall expire and be invalidated on logout. |
| NFR-2.6 | Visa documents shall be stored securely on the server filesystem. |
| NFR-2.7 | HTTPS shall be used in production. |

### 2.3 Usability & User Experience

| ID | Requirement |
|----|-------------|
| NFR-3.1 | The UI shall be responsive (mobile, tablet, desktop). |
| NFR-3.2 | Forms shall show validation errors below each input field. |
| NFR-3.3 | Empty states shall guide users (e.g. "Add a job first"). |
| NFR-3.4 | Key actions shall use icons and step-by-step onboarding hints. |
| NFR-3.5 | The calendar shall use colour coding (green = done, red = not done). |
| NFR-3.6 | Error messages from the API shall be displayed clearly to the user. |

### 2.4 Reliability & Availability

| ID | Requirement |
|----|-------------|
| NFR-4.1 | The system shall persist all data in a relational database (SQLite for development, PostgreSQL for production). |
| NFR-4.2 | Failed save operations shall keep the user on the form with error details visible. |
| NFR-4.3 | SignalR shall auto-reconnect on connection loss. |
| NFR-4.4 | Auth token refresh shall prevent unnecessary logouts. |

### 2.5 Scalability & Maintainability

| ID | Requirement |
|----|-------------|
| NFR-5.1 | The backend shall follow a layered architecture (Controllers, Services, Data). |
| NFR-5.2 | The frontend shall use reusable components (forms, charts, modals). |
| NFR-5.3 | The AI advisor shall run as a separate Python microservice. |
| NFR-5.4 | The system shall support switching database provider via configuration. |

### 2.6 Compatibility

| ID | Requirement |
|----|-------------|
| NFR-6.1 | The frontend shall run on modern browsers (Chrome, Edge, Firefox, Safari). |
| NFR-6.2 | The API shall expose REST JSON endpoints consumable by web clients. |
| NFR-6.3 | CORS shall allow the configured frontend origin. |

### 2.7 Legal & Compliance

| ID | Requirement |
|----|-------------|
| NFR-7.1 | The system shall support the Australian student visa 48-hour per fortnight work rule. |
| NFR-7.2 | Registration shall capture visa type, status, working rights, and expiry date. |
| NFR-7.3 | Admin approval shall be required before new users can access the system. |

### 2.8 Audit & Administration

| ID | Requirement |
|----|-------------|
| NFR-8.1 | Admin shall view all users and assign roles. |
| NFR-8.2 | Admin shall view platform analytics (users, shifts, earnings, expenses). |
| NFR-8.3 | Registration approval actions shall be recorded with approver and timestamp. |

---

## 3. Summary

| Type | Modules | Focus |
|------|---------|--------|
| Functional | 9 modules, approximately 45 requirements | What the system must do |
| Non-Functional | 8 categories, approximately 30 requirements | How well the system must perform |

**Problem Solved:** OraEarn helps hourly workers — especially student visa holders — track shifts, stay visa-compliant, manage expenses, and reach savings goals in one integrated platform.

---

*Document generated for OraEarn project.*
