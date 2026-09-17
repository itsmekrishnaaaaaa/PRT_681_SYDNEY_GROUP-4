# PRT681 Software Engineering Practice
# Assessment 2 — Project Progress Report

**Group:** Group 4 (Sydney)  
**Project:** OraEarn  
**Student writing the individual part:** Dharma Bahadur Budhathoki (S395172)

---

# Part A — Team report

OraEarn is a web app for hourly workers, mainly international students. It logs jobs and shifts, watches the student-visa 48-hour fortnight, tracks expenses and savings, and has a small AI helper called Aura. We run three services on the laptop, no Docker: React on port 5173, ASP.NET (`OraEarn.Api`) on 5203, and Python Aura on 8000, with PostgreSQL as the database. There is no public URL yet. Demo accounts after seed are `alice@oraearn.local` / `User123!` and `admin@oraearn.local` / `Admin123!`.

| Student ID | Name | Role |
|------------|------|------|
| S395172 | Dharma Bahadur Budhathoki | Developer, Business Analyst |
| S395795 | Krishna Sapkota | DBA / Tester |
| s395321 | Jenish Gurung | Business Analyst / Report Writer |
| S394949 | Suraj Khadka | Data Analyst / DBA |

We use **Jira**, not Trello.

- GitHub: https://github.com/itsmekrishnaaaaaa/PRT_681_SYDNEY_GROUP-4
- Branch: `app-s395172` — https://github.com/itsmekrishnaaaaaa/PRT_681_SYDNEY_GROUP-4/tree/app-s395172
- Jira (EARN): https://students-team-dh8zehye.atlassian.net/jira/core/projects/EARN/board?filter=&groupBy=none

**Figure 1.** GitHub showing branch `app-s395172`. *[paste screenshot]*  
**Figure 2.** Jira EARN board. *[paste screenshot]*

By week 8–9 the product actually runs. A user can register and log in with JWT, an admin can approve accounts, and a worker can manage jobs, shifts, expenses, and savings. The dashboard shows money and visa hours and can export Excel/PDF. Notifications and Aura are in the UI. Late work was look-and-feel: one black-and-white theme, a phone layout, a password eye on login, and CRUD forms in pop-out dialogs so the pages are not a mess. Proposal, FR/NFR, ERD, and sprint notes are in the repo.

Still open is mostly process. `app-s395172` is not merged to `main`, so the default GitHub view does not show the app. We have no hosted demo. Jira only shows the active sprint unless you open the right board, and we had to make every ticket a Story after Epic/Sub-task import failed. Formal UAT notes and a written test log still need to sit on the tickets.

**Figure 3.** Login (eye icon). *[paste]*  
**Figure 4.** Dashboard. *[paste]*  
**Figure 5.** Add job or Add expense dialog. *[paste]*

**Risks.** If a marker clones `main`, they will miss the app — merge this week and say so in the README. If Jira looks empty, open the URL above with the sprint selected. If Postgres or ports fail, Krishna should do a clean install from the run scripts. Git is uneven because most application commits are on Dharma’s branch; the others should commit tests, schema notes, and report files there too.

**Lesson.** A flat Story board beats a broken hierarchy. Git only helps if people clone the working branch. Three local services are fiddly, but they stopped the API, UI, and Aura blocking each other. For this unit, screenshots and ticket status count as much as code.

---

# Part B — Individual report  
## Dharma Bahadur Budhathoki (S395172)

GitHub: **dharma001** (commits as `Dharma001 <budhathokidharma8@gmail.com>`). Branch: **app-s395172**.

I am the developer and one of the BAs. I wrote most of the running app, set up Jira, and kept my weekly folder. I am not saying I did the whole group project. Krishna, Jenish, and Suraj still own testing, the write-up, and data/DBA work. Of **my** hours:

| Research | Learning | Coding | Management |
|---------:|---------:|-------:|-----------:|
| 15% | 20% | 50% | 15% |

**Git.** https://github.com/itsmekrishnaaaaaa/PRT_681_SYDNEY_GROUP-4/commits/app-s395172

| Date | Work | Link |
|------|------|------|
| 18 Sep | CRUD pop-outs for jobs, expenses, savings | [b6bbc20](https://github.com/itsmekrishnaaaaaa/PRT_681_SYDNEY_GROUP-4/commit/b6bbc2098e700165f097aa2926e5c2ea37f5e204) |
| 18 Sep | Login password eye, responsive layout, black-and-white theme | [c523e0b](https://github.com/itsmekrishnaaaaaa/PRT_681_SYDNEY_GROUP-4/commit/c523e0bd5650ff1b02bfc4327dcc93a2a59adde0), [a5ae67d](https://github.com/itsmekrishnaaaaaa/PRT_681_SYDNEY_GROUP-4/commit/a5ae67db7a000a41e38f0de2a9395292ab1a8573), [00ab4e2](https://github.com/itsmekrishnaaaaaa/PRT_681_SYDNEY_GROUP-4/commit/00ab4e2f4b5606b01c7ba0093725a4d785a504e7) |
| 8 Sep | Main application (pages + API) | [d60cba0](https://github.com/itsmekrishnaaaaaa/PRT_681_SYDNEY_GROUP-4/commit/d60cba03d7f3d151fd0d33a041088729f5b23779) |
| 1 Sep | Auth DTOs, User, database connection | [65f5f81](https://github.com/itsmekrishnaaaaaa/PRT_681_SYDNEY_GROUP-4/commit/65f5f81cad4729ef1f9c5c890ab31c5bb36e14f7) |
| 23 Aug | Frontend packages, EF/Postgres/Swagger, week 5 proofs | [64bec00](https://github.com/itsmekrishnaaaaaa/PRT_681_SYDNEY_GROUP-4/commit/64bec00dab89e128ab7ec5ba7d70028eb61c87a6), [03bfdeb](https://github.com/itsmekrishnaaaaaa/PRT_681_SYDNEY_GROUP-4/commit/03bfdeb34e6830f8c611f5012882f337354294ff) |
| 17 Aug | Proposal, problem/solution, stack research | [efdea1e](https://github.com/itsmekrishnaaaaaa/PRT_681_SYDNEY_GROUP-4/commit/efdea1ef9b1f0410423b773fecdfd0e9fd49af06) |

**Figure 6.** Commit list on `app-s395172`. *[paste]*

**Jira.** Same board URL. I am reporter on the EARN stories. Mine cover auth, jobs/shifts, expenses/savings, visa hours, dashboard, Aura, and the docs tickets. Krishna has schema and test, Jenish has AC and the report, Suraj has analytics data and DBA support.

**Figure 7.** Jira filter for my tickets. *[paste]*

**Time.** Timesheet: `Individual Timesheets - Dharma Bahadur Budhathoki(S395172) - weeks 4-8.xlsx` in my student folder (the older file stopped at Week 07 and Week 05 hours were in the wrong column; that is fixed). The sheet cap is 1–6 hours a week. Week 7 meeting log (1 Sep) accepted auth, admin, and expense CRUD and pointed Week 8 at Swagger and Aura, which matches the 8 Sep commit.

| Week | Dates | Work on the timesheet | Hours |
|------|-------|------------------------|------:|
| 04 | 11–17 Aug | Project concept, canvas, visa-hour idea, Aura/savings flow | 5.5 |
| 05 | 17–23 Aug | Requirements to tech tasks, Git as Scrum Master, ERD/normalisation, project setup, EF models | 6 |
| 06 | 24–30 Aug | Postgres/EF connection, frontend/backend setup, schema from ERD, Aura backend start, Week 6 meeting | 6 |
| 07 | 31 Aug – 4 Sep | JWT auth, admin approval dashboard, expense CRUD, Swagger, Week 7 meeting | 6 |
| 08 | 7–11 Sep | Finish Swagger; jobs/shifts/calendar; visa 48-hour + Excel/PDF export; Aura UI/backend; Week 8 review | 6 |
| | | **Total (Week 04–08)** | **29.5** |

Later UI work (black-and-white theme, phone layout, password eye, CRUD pop-outs on 18 Sep) is on Git but sits after this Assessment 2 timesheet.

**Challenges.** Docker was a pain on a student laptop, so we dropped it and used Postgres as a Windows service. EF then complained about pending model changes, and renaming the backend from ShiftTracker to OraEarn failed while Visual Studio was still building. I stopped MSBuild, renamed, and added the missing migration. Jira would not take “1h” for Original Estimate (it wants seconds), then CSV import rejected Epic/Sub-task, so everything became a Story. On the UI, colour was all over the place until I put tokens in `index.css`. CRUD pages were too long, so those forms are dialogs now. Phone layout needed a header, drawer, and bottom nav, not a squeezed desktop sidebar.

**Krishna (DBA/Tester) — three improvements**

1. Keep a short recreate-database note next to schema changes so our local `oraearn` databases match.  
2. Put a test checklist on the sprint (login, job, shift over 48 hours, expense, Aura) instead of only saying it works.  
3. When something fails, write the steps on the Jira ticket, not only in chat.

**Jenish (BA/Report writer) — three improvements**

1. Put acceptance criteria on the story before I start the screen, even if it is four bullets.  
2. After UAT, mark pass/fail on the ticket so Done means checked, not just coded.  
3. Send a five-line weekly summary for the group report so we are not writing this from memory.

**Suraj (Data analyst/DBA) — three improvements**

1. Own what the dashboard numbers mean (net balance, fortnight hours, savings rate) so charts match the tables.  
2. When the ERD changes, say what that does to analytics.  
3. Mark which seed rows are demo data so testers are not staring at empty charts.

**Collaboration.** We did not fight; we drifted. The app lives on my branch while `main` still has older folder uploads. Jira existed, but tickets were not moved every day, so the board lagged the code. Chat was used more than ticket comments. From here: one short standup a week (yesterday / today / stuck) and move Jira in the same meeting; everyone commits tests, ERD, or report files on `app-s395172` until we merge; no Done without a screenshot or a test note.

**Help I still need.** Krishna to do a clean install and a test pass. Jenish to tidy the shared team wording. Suraj to check analytics against the database. Someone to merge to `main` with me so this is not only my branch.

**Three ways to improve the class.** Give a sample progress report so we can see how screenshots should be labelled. Warn in the Jira workshop that student CSV import often rejects Epic/Sub-task and that Story-only is fine. Check earlier whether the working branch is the one people clone — `main` looking empty cost us time.

**Three ways I will improve.** Smaller commits and cleaner messages; I dumped “added application” and some messages have typos. Log the timesheet the same day, not in a batch like 15 Sep. Open the Jira ticket before I code, even if I am working alone that night. I am faster in the editor than on the board, which is why Git is ahead of EARN.

---

Paste screenshots under Figures 1–7. Do not show passwords. Phone-width layout is optional but helps.
