# OraEarn — Project Proposal

**Document version:** 1.0  
**Date:** August 2026  
**Project type:** Full-stack web application  
**Prepared for:** Academic / professional submission  

---

## 1. Executive Summary

**OraEarn** is a proposed web-based personal work and financial management platform designed for hourly workers, with a primary focus on international students in Australia. The platform integrates shift tracking, earnings calculation, expense management, savings goal planning, visa compliance monitoring, analytical reporting, and an AI-powered advisory service (Aura) within a single secure application.

International students and casual workers frequently manage employment, budgeting, and regulatory obligations using disconnected tools such as spreadsheets, calendar notes, and banking apps. This fragmentation increases the risk of financial mismanagement and inadvertent non-compliance with visa work restrictions—most notably the **48-hour per fortnight** limit applicable to student visa holders during study periods.

OraEarn addresses this gap by providing a unified, data-driven solution that improves record accuracy, financial visibility, and regulatory awareness. The system will be delivered as a modular full-stack application comprising an ASP.NET Core Web API, a React frontend, a Python AI microservice, and relational database storage.

---

## 2. Introduction

### 2.1 Background

Australia hosts a large population of international students who undertake part-time employment to support living expenses and tuition costs. While employment provides essential financial support, it also introduces administrative complexity. Students must track working hours across multiple jobs, monitor cumulative fortnightly hours against visa conditions, manage day-to-day expenses, and plan for savings goals—all while maintaining academic commitments.

Existing consumer applications typically address only one dimension of this problem (e.g. budgeting or time tracking) and do not incorporate visa-specific compliance logic or contextual advisory features tailored to the user's actual employment and financial data.

### 2.2 Project Title

**OraEarn** — *Shift, Finance, and Visa Compliance Management Platform with AI Advisory*

### 2.3 Motivation

The motivation for this project is to design and implement a practical, scalable system that:

- Reduces manual administrative effort for hourly workers
- Improves financial literacy through structured analytics
- Supports compliance with visa-related work limitations
- Delivers actionable guidance through an integrated AI assistant

---

## 3. Problem Statement

Hourly workers and international students face the following challenges:

| Challenge | Description |
|-----------|-------------|
| **Fragmented record-keeping** | Shifts, expenses, and savings are tracked across multiple tools with no single source of truth. |
| **Visa compliance risk** | Student visa holders must not exceed 48 hours of work per rolling 14-day period; manual tracking is error-prone. |
| **Limited financial insight** | Users lack consolidated views of earnings, spending patterns, and savings progress. |
| **Reactive decision-making** | Without alerts or advisory support, users may only discover problems (e.g. overwork, negative balance) after they occur. |
| **No contextual guidance** | Generic financial apps do not account for visa status, hourly employment, or individual savings goals. |

There is a clear need for an integrated platform that combines employment tracking, financial management, compliance enforcement, and intelligent advisory services in one authenticated environment.

---

## 4. Project Objectives

### 4.1 Primary Objective

To develop a secure, user-friendly web application that enables hourly workers and international students to manage shifts, finances, and visa compliance within a single platform.

### 4.2 Specific Objectives

1. Implement secure user authentication with role-based access (Admin / User).
2. Enable job and shift management with automated earnings calculation.
3. Provide live shift clock-in/clock-out with real-time calendar updates.
4. Enforce and monitor student visa fortnight hour limits (48 hours).
5. Support expense categorisation, budgeting, and savings goal tracking.
6. Deliver analytics dashboards with period filters and Excel/PDF export.
7. Generate proactive notifications for compliance and financial risks.
8. Integrate Aura, a rule-based AI advisor that responds using the user's live data.
9. Provide administrative tools for user approval and platform analytics.

### 4.3 Success Criteria

| Criterion | Measure |
|-----------|---------|
| Functional completeness | All core modules (shifts, expenses, savings, analytics, AI) operational |
| Compliance enforcement | System blocks shift creation/update when visa limit would be exceeded |
| Data accuracy | Hours, earnings, and balances calculated automatically from source records |
| Usability | Responsive UI with guided onboarding and clear error messages |
| Security | JWT authentication, row-level data isolation, admin-only endpoints protected |
| Portability | Project documented and structured for deployment on a new machine |

---

## 5. Scope

### 5.1 In Scope

- User registration, login, JWT authentication, and admin approval workflow
- Job CRUD and shift CRUD (manual entry and live sessions)
- Monthly shift calendar with visual completion indicators
- Visa compliance tracking and fortnight hour enforcement
- Expense categories, expense records, and budget calculator
- Savings goals with allocation percentages and progress tracking
- Analytics dashboard (7 days / 1 month / 12 months)
- Notification system with read/unread state
- Aura AI chat widget (rule-based Python microservice)
- Admin panel for users, roles, and platform analytics
- Excel and PDF export of analytics reports
- SignalR real-time shift updates

### 5.2 Out of Scope (Initial Release)

- Native mobile applications (iOS / Android)
- Integration with external payroll or banking APIs
- Large language model (LLM) / OpenAI integration
- Multi-currency or multi-country visa rule engines
- Automated tax reporting or official immigration submissions
- Offline-first mobile sync

*Note: The architecture supports future LLM integration in the AI advisor without frontend changes.*

---

## 6. Target Users

### 6.1 Primary Users

**International students (User role)**  
Employed on a part-time or casual basis while studying in Australia. Require shift tracking, visa compliance monitoring, expense management, and savings planning.

### 6.2 Secondary Users

**Hourly / casual workers (User role)**  
Individuals who need structured employment and financial tracking without visa restrictions.

### 6.3 Administrative Users

**Platform administrators (Admin role)**  
Responsible for approving registrations, managing user roles, reviewing visa documents, and viewing aggregate platform analytics.

---

## 7. Proposed Solution

### 7.1 System Overview

OraEarn will be implemented as a three-tier web application:

```
┌─────────────────┐     ┌──────────────────────┐     ┌─────────────────┐
│  React Frontend │ ──► │  ASP.NET Core API    │ ──► │  SQLite /       │
│  (Vite + TS)    │     │  (Auth, CRUD, Rules) │     │  PostgreSQL     │
└────────┬────────┘     └──────────┬───────────┘     └─────────────────┘
         │                         │
         │                         ▼
         │              ┌──────────────────────┐
         └────────────► │  Python FastAPI      │
           Aura chat    │  (AI Advisor)        │
                        └──────────────────────┘
```

### 7.2 Core Modules

| Module | Description |
|--------|-------------|
| **Authentication** | Registration with visa details, JWT tokens, refresh tokens, admin approval |
| **Jobs & Shifts** | Job profiles, shift logging, live clock-in/out, calendar view, SignalR updates |
| **Visa Compliance** | Rolling 14-day hour calculation, limit enforcement, status display |
| **Expenses** | Categories, fixed/variable/one-off types, summaries, budget calculator |
| **Savings Goals** | Targets, allocation %, contributions, progress and completion status |
| **Analytics** | Dashboard stats, timeline charts, category breakdown, period filters, export |
| **Notifications** | Visa, balance, savings, and allocation alerts with persistent read state |
| **Aura AI** | Floating chat widget; answers based on user's visa, shifts, goals, and balance |
| **Administration** | User management, registration review, platform-wide analytics |

### 7.3 Aura AI Advisor

Aura is a standalone Python microservice that receives a structured JSON context from the .NET API (user profile, visa status, savings goals, notifications) and returns short, practical replies using rule-based logic. This approach:

- Requires no external AI API keys
- Keeps user data within the project environment
- Can be upgraded to an LLM backend in future without changing the UI

---

## 8. Technology Stack

| Layer | Technology | Justification |
|-------|------------|---------------|
| **Frontend** | React 19, Vite, TypeScript, Tailwind CSS, shadcn/ui | Modern, type-safe, responsive UI with reusable components |
| **Backend API** | ASP.NET Core 10, Entity Framework Core | Robust REST API, Identity, JWT, strong typing |
| **Real-time** | SignalR (WebSockets) | Live shift calendar updates across browser sessions |
| **Database** | SQLite (dev), PostgreSQL (prod) | Lightweight local dev; production-grade relational storage |
| **AI Service** | Python 3, FastAPI, Pydantic | Lightweight microservice for advisory logic |
| **Auth** | ASP.NET Identity + JWT Bearer | Industry-standard authentication and authorisation |
| **Export** | xlsx, jsPDF (client-side) | Analytics download without additional server dependencies |
| **Containerisation** | Docker Compose | Optional PostgreSQL for production-like environments |

---

## 9. System Architecture

### 9.1 Backend Structure

```
Controllers/  →  Services/  →  Entities/ + DTOs/
                      ↓
                 Data/ (DbContext, Migrations, Seeders)
```

### 9.2 Frontend Structure

```
src/
├── api/          # One module per API resource
├── pages/        # Route-level screens
├── components/   # UI, charts, modals, Aura chat
├── context/      # Authentication state
├── hooks/        # SignalR hub connection
├── types/        # Shared TypeScript interfaces
└── utils/        # Dates, forms, export helpers
```

### 9.3 Key Entities (ERD Summary)

- **Application User** → Jobs, Shifts, Expenses, Savings Goals, Notifications
- **Job** → Shifts, Active Shift Sessions
- **Expense Category** → Expenses

---

## 10. Methodology

The project will follow an **iterative development approach**:

| Phase | Activities |
|-------|------------|
| **Phase 1 — Foundation** | Database design, authentication, user roles, admin approval |
| **Phase 2 — Core features** | Jobs, shifts, expenses, savings goals |
| **Phase 3 — Compliance & analytics** | Visa enforcement, dashboard, charts, period filters |
| **Phase 4 — Intelligence** | Notifications, Aura AI advisor, export functionality |
| **Phase 5 — Polish & documentation** | UI refinement, testing, README, project proposal, requirements docs |

Each phase delivers working, testable functionality before proceeding to the next.

---

## 11. Deliverables

| # | Deliverable | Description |
|---|-------------|-------------|
| 1 | **Source code** | Complete backend, frontend, and AI advisor repositories |
| 2 | **Database schema** | EF Core migrations and seed data |
| 3 | **Web application** | Deployable React frontend with all user-facing features |
| 4 | **REST API** | Documented endpoints with JWT authentication |
| 5 | **AI microservice** | Python FastAPI advisor with README |
| 6 | **Documentation** | README, requirements, project proposal |
| 7 | **Demo accounts** | Seeded admin and user profiles for demonstration |
| 8 | **Export capability** | Analytics download in Excel and PDF formats |

---

## 12. Expected Outcomes and Benefits

### 12.1 For Users

- Single platform for shifts, money, and visa compliance
- Automated calculations reduce manual errors
- Timely alerts prevent overwork and financial surprises
- Analytics support informed budgeting and savings decisions
- Aura provides instant, data-driven answers without external tools

### 12.2 For Administrators

- Centralised user approval and role management
- Platform-wide analytics for oversight
- AI-assisted registration review workflow

### 12.3 Technical Outcomes

- Demonstrates full-stack development across .NET, React, and Python
- Shows microservice integration, real-time communication, and secure API design
- Produces a maintainable, documented codebase suitable for portfolio or further development

---

## 13. Risks and Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| Visa rule changes | Compliance logic may become outdated | Configurable hour limits; document assumption of 48 hr/fortnight |
| AI service offline | Chat unavailable | Graceful fallback message; core app functions independently |
| Data accuracy | Incorrect earnings or hours | Server-side calculation; validation on shift create/update |
| Security breach | Unauthorised data access | JWT auth, row-level isolation, admin role checks, HTTPS in production |
| Scope creep | Delayed delivery | Clear in/out of scope; phased implementation |

---

## 14. Assumptions and Constraints

### 14.1 Assumptions

- Users have access to a modern web browser and internet connection
- Student visa work limit of 48 hours per rolling fortnight applies to target users
- Hourly rates are defined per job and used for earnings calculation
- Administrators review and approve new registrations before access is granted

### 14.2 Constraints

- Initial release is web-only (no native mobile app)
- AI advisor uses rule-based logic, not a commercial LLM
- Currency displayed as configured in the application (USD in current implementation)
- System provides guidance only; it does not constitute legal or immigration advice

---

## 15. Conclusion

OraEarn is proposed as a comprehensive solution to the fragmented and high-risk nature of personal work and financial management faced by hourly workers and international students. By unifying shift tracking, expense management, savings planning, visa compliance monitoring, analytics, notifications, and AI advisory services, the platform addresses a clearly defined problem with measurable objectives and a practical technology stack.

The project is feasible within a structured development timeline, delivers tangible value to its target users, and demonstrates professional full-stack engineering practices suitable for academic assessment, portfolio presentation, or future commercial development.

---

## Appendix A — Demo Credentials

| Role | Email | Password |
|------|-------|----------|
| Admin | admin@shifttracker.local | Admin123! |
| User (visa demo) | alice@shifttracker.local | User123! |
| User | bob@shifttracker.local | User123! |

## Appendix B — Service URLs (Development)

| Service | URL |
|---------|-----|
| Frontend | http://localhost:5173 |
| API | http://localhost:5203 |
| AI Advisor | http://localhost:8000 |
| SignalR Hub | http://localhost:5203/hubs/shifts |

---

*End of document*
