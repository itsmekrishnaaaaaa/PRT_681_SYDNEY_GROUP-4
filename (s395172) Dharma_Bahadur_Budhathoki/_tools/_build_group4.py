from pathlib import Path
from shutil import copy2
from zipfile import ZIP_DEFLATED, ZipFile

import openpyxl
from docx import Document
from docx.enum.text import WD_ALIGN_PARAGRAPH
from docx.opc.constants import RELATIONSHIP_TYPE as RT
from docx.oxml import OxmlElement
from docx.oxml.ns import qn
from docx.shared import Inches, Pt, RGBColor

GH = "https://github.com/itsmekrishnaaaaaa/PRT_681_SYDNEY_GROUP-4"
GH_BRANCH = GH + "/tree/app-s395172"
GH_COMMITS = GH + "/commits/app-s395172"
JIRA = "https://students-team-dh8zehye.atlassian.net/jira/core/projects/EARN/board?filter=&groupBy=none"
COMMITS = {
    "b6bbc20": GH + "/commit/b6bbc2098e700165f097aa2926e5c2ea37f5e204",
    "c523e0b": GH + "/commit/c523e0bd5650ff1b02bfc4327dcc93a2a59adde0",
    "a5ae67d": GH + "/commit/a5ae67db7a000a41e38f0de2a9395292ab1a8573",
    "00ab4e2": GH + "/commit/00ab4e2f4b5606b01c7ba0093725a4d785a504e7",
    "d60cba0": GH + "/commit/d60cba03d7f3d151fd0d33a041088729f5b23779",
    "65f5f81": GH + "/commit/65f5f81cad4729ef1f9c5c890ab31c5bb36e14f7",
    "3603d85": GH + "/commit/3603d85761229e2d22716abdbca09d5fd7c5f65c",
}

DHARMA = Path(r"C:\projects\clz\oraEarn\(s395172) Dharma_Bahadur_Budhathoki")
FIGS = DHARMA / "figures"
OUT = DHARMA / "PRT681_Assessment2_Dharma_Bahadur_Budhathoki_S395172_OraEarn_Progress_Report.docx"
DL = Path(r"C:\Users\budha\Downloads\OraEarn-Assessment-2-S395172")
TS_DHARMA = DHARMA / "Individual Timesheets - Dharma Bahadur Budhathoki(S395172) - weeks 4-10.xlsx"
KRISHNA_ERD = Path(r"C:\projects\clz\oraEarn\Krishna_sapkota_S395795\Week-4\ERD-OraEarn.png")
JENISH_UML = Path(r"C:\projects\clz\oraEarn\Jenish_Gurung_s395321\week5") / "OraEarn_UML_EndToEnd (1).png"


def set_run(run, size=11, bold=False, italic=False):
    run.font.name = "Calibri"
    run._element.rPr.rFonts.set(qn("w:eastAsia"), "Calibri")
    run.font.size = Pt(size)
    run.bold = bold
    run.italic = italic
    run.font.color.rgb = RGBColor(0x22, 0x22, 0x22)


def body(doc, text):
    p = doc.add_paragraph()
    p.paragraph_format.space_after = Pt(8)
    p.paragraph_format.line_spacing = 1.15
    r = p.add_run(text)
    set_run(r)
    return p


def add_hyperlink(paragraph, text, url):
    r_id = paragraph.part.relate_to(url, RT.HYPERLINK, is_external=True)
    hyperlink = OxmlElement("w:hyperlink")
    hyperlink.set(qn("r:id"), r_id)
    new_run = OxmlElement("w:r")
    rPr = OxmlElement("w:rPr")
    rFonts = OxmlElement("w:rFonts")
    rFonts.set(qn("w:ascii"), "Calibri")
    rFonts.set(qn("w:hAnsi"), "Calibri")
    sz = OxmlElement("w:sz")
    sz.set(qn("w:val"), "22")
    color = OxmlElement("w:color")
    color.set(qn("w:val"), "0563C1")
    u = OxmlElement("w:u")
    u.set(qn("w:val"), "single")
    rPr.append(rFonts)
    rPr.append(sz)
    rPr.append(color)
    rPr.append(u)
    new_run.append(rPr)
    t = OxmlElement("w:t")
    t.set(qn("xml:space"), "preserve")
    t.text = text
    new_run.append(t)
    hyperlink.append(new_run)
    paragraph._p.append(hyperlink)


def link_line(doc, label, url, display=None):
    p = doc.add_paragraph()
    p.paragraph_format.space_after = Pt(6)
    p.paragraph_format.line_spacing = 1.15
    r = p.add_run(label)
    set_run(r)
    add_hyperlink(p, display or url, url)


def style_table(table):
    for i, row in enumerate(table.rows):
        for cell in row.cells:
            for p in cell.paragraphs:
                for run in p.runs:
                    set_run(run, size=10, bold=(i == 0))


def caption(doc, text):
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p.paragraph_format.space_after = Pt(10)
    r = p.add_run(text)
    set_run(r, size=10, italic=True)


FIGURES: list[str] = []


def fig(doc, path, cap, width=5.8):
    path = Path(path)
    if not path.exists():
        raise FileNotFoundError(f"Figure missing, will not submit a broken report: {path}")
    p = doc.add_paragraph()
    p.alignment = WD_ALIGN_PARAGRAPH.CENTER
    p.add_run().add_picture(str(path), width=Inches(width))
    caption(doc, cap)
    FIGURES.append(cap)


def bullets(doc, items):
    for item in items:
        p = doc.add_paragraph(item, style="List Number")
        for r in p.runs:
            set_run(r)


def add_page_number(paragraph):
    run = paragraph.add_run()
    fld1 = OxmlElement("w:fldChar")
    fld1.set(qn("w:fldCharType"), "begin")
    instr = OxmlElement("w:instrText")
    instr.set(qn("xml:space"), "preserve")
    instr.text = " PAGE "
    fld2 = OxmlElement("w:fldChar")
    fld2.set(qn("w:fldCharType"), "end")
    run._r.append(fld1)
    run._r.append(instr)
    run._r.append(fld2)


doc = Document()
sec = doc.sections[0]
sec.top_margin = Inches(0.9)
sec.bottom_margin = Inches(0.85)
sec.left_margin = Inches(1.0)
sec.right_margin = Inches(1.0)
header = sec.header.paragraphs[0]
header.alignment = WD_ALIGN_PARAGRAPH.RIGHT
hr = header.add_run("PRT681 Assessment 2  ·  Dharma Bahadur Budhathoki (S395172)  ·  Week 10")
set_run(hr, size=9, italic=True)
footer = sec.footer.paragraphs[0]
footer.alignment = WD_ALIGN_PARAGRAPH.CENTER
fr = footer.add_run("Project Progress Report  ·  page ")
set_run(fr, size=9)
add_page_number(footer)

# --- Cover ---
for _ in range(2):
    doc.add_paragraph()
p = doc.add_paragraph()
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
r = p.add_run("Charles Darwin University")
set_run(r, size=12, italic=True)
p = doc.add_paragraph()
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
r = p.add_run("Faculty of Science and Technology")
set_run(r, size=11)
p = doc.add_paragraph()
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
r = p.add_run("PRT681 Software Engineering Practice")
set_run(r, size=16, bold=True)
p = doc.add_paragraph()
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
r = p.add_run("Assessment 2 — Project Progress Report (20%)")
set_run(r, size=14, bold=True)
p = doc.add_paragraph()
p.alignment = WD_ALIGN_PARAGRAPH.CENTER
r = p.add_run("Submitted by Dharma Bahadur Budhathoki (S395172)  ·  Group 4 (Sydney)  ·  Week 10")
set_run(r, size=12)

cover_rows = [
    ["Student ID", "Name", "Role"],
    ["S395172", "Dharma Bahadur Budhathoki", "Developer / BA / Scrum Master"],
    ["S395795", "Krishna Sapkota", "DBA / Tester"],
    ["s395321", "Jenish Gurung", "Business Analyst / Report Writer"],
    ["S394949", "Suraj Khadka", "Data Analyst / DBA"],
]
t = doc.add_table(rows=5, cols=3)
t.style = "Table Grid"
for i, row in enumerate(cover_rows):
    for j, v in enumerate(row):
        t.rows[i].cells[j].text = v
style_table(t)

link_line(doc, "GitHub repository: ", GH)
link_line(doc, "Working branch (clone this, not main): ", GH_BRANCH, "app-s395172")
link_line(doc, "Commit list on that branch: ", GH_COMMITS)
link_line(doc, "Jira board (not Trello): ", JIRA, "EARN / ora--earn board")
body(doc, "Date of this file: 28 September 2026 (Week 10). Packed and named by Dharma Bahadur Budhathoki (S395172). The GitHub repo is private. Add the lecturer as a collaborator so the blue links open. Jira needs a CDU student login.")
body(
    doc,
    "This Learnline pack is under my name because I assembled the report, figures, Swagger tests and timesheet through Week 10. It is still the group Assessment 2 file: one shared team report, then one individual section for each member. Excel timesheets are separate files in the same zip.",
)
body(
    doc,
    "Declaration. The team pages are shared. Each individual section is written in that student’s name from their Git, Jira, timesheet and week folders. We have not copied another group’s report. Demo account passwords are not included.",
)

doc.add_heading("What is in this submission", 1)
body(doc, "The unit asks for a progress report, not extra coding. Markers look at four things: individual contribution (Git/Jira/work), individual reflection, the team report, and time management (timesheets).")
bullets(
    doc,
    [
        "Part A — Team report: what OraEarn is, who does what, URLs, what is finished, what is not, risks, lessons, labelled figures.",
        "Part B — Dharma (S395172): Scrum Master + developer + supporting BA — how I was involved in every team task (Krishna / Jenish / Suraj), ceremonies, impediments, Git/Jira evidence.",
        "Part C — Krishna (S395795): ERD/SQL/testing, timesheet weeks 4–6, Jira test tickets.",
        "Part D — Jenish (s395321): BRD/SRS/UML/traceability, timesheet, UML figures.",
        "Part E — Suraj (S394949): dashboard numbers and export, timesheet weeks 4–8, one-page data dictionary. Git under his name is still thin.",
        "Figures: live GitHub repo, Jira, running UI (PC and phone), API tests, Aura, architecture / use-case / sequence / Scrum diagrams, Krishna ERD, Jenish UML, timesheets.",
    ],
)
body(doc, "Attach with this Word file on Learnline: my weeks 4–10 timesheet (Dharma S395172), plus Krishna, Jenish (and Jenish week 8), Suraj weeks 4–8, and Suraj’s data dictionary.")

doc.add_page_break()

doc.add_heading("TEAM COMPONENT — Application Report", 1)
doc.add_heading("Project Details", 2)
body(
    doc,
    "OraEarn started as a fairly ordinary student idea: keep track of casual jobs without losing the visa-hour limit in a spreadsheet. The people we had in mind are international students doing cafe or retail shifts. They need to know how many hours they have already worked this fortnight, what they earned, and whether rent and food still leave anything for savings. Aura is a small chat helper on the side, not the whole product.",
)
body(
    doc,
    "Nothing is hosted on the public internet yet. On a laptop the stack is three services and Postgres — React on 5173, the ASP.NET API on 5203, Aura on 8000. We dropped Docker because it was more trouble than it was worth on our machines. If a marker wants to run it, clone app-s395172 (not main) and follow the README. Demo logins are in that README; we are not repeating passwords here.",
)

doc.add_heading("Team info", 2)
t = doc.add_table(rows=5, cols=3)
t.style = "Table Grid"
for i, h in enumerate(["Student ID", "Name", "Role"]):
    t.rows[0].cells[i].text = h
for i, row in enumerate(
    [
        ["S395172", "Dharma Bahadur Budhathoki", "Developer, BA, Scrum Master"],
        ["S395795", "Krishna Sapkota", "DBA / Tester"],
        ["s395321", "Jenish Gurung", "Business Analyst / Report Writer"],
        ["S394949", "Suraj Khadka", "Data Analyst / DBA"],
    ],
    start=1,
):
    for j, v in enumerate(row):
        t.rows[i].cells[j].text = v
style_table(t)

doc.add_heading("URLs", 2)
link_line(doc, "GitHub repository: ", GH)
link_line(doc, "Working branch (clone this, not main): ", GH_BRANCH, "app-s395172")
link_line(doc, "Commit list: ", GH_COMMITS)
link_line(doc, "Jira (used instead of Trello): ", JIRA, "EARN / ora--earn board")

doc.add_heading("Summary of progress", 2)

body(
    doc,
    "By week 8 the app actually does the job. Someone can register with visa details and an ID file, wait for admin approval, sign in, add employers, put shifts on a calendar or clock in live, see the 48-hour fortnight bar, log expenses and savings goals, and pull Excel or PDF from the dashboard. The screen is black and white on purpose, and it is usable on a phone. Add/edit for jobs, expenses, savings and shifts is in a pop-out so the pages are not a long form.",
)
body(
    doc,
    "We did not all sit on the same files. Dharma has been living in the running code on app-s395172 and also running Scrum (board, branch, week labels). That includes the analytics dashboard, visa-hour bar, Excel/PDF export and Alice seed data — work that was named as the data-analyst seat, but the Git and the running screens are Dharma’s. Krishna owns the ERD and the “does this data even make sense” tests. Jenish has been chasing BRD, SRS, UML and whether a screen still maps to a requirement. Suraj’s named role was dashboard meaning and export columns; there are no product commits under his GitHub name, so those features are evidenced in Dharma’s section.",
)
body(
    doc,
    "Still open, and we should say it plainly: there is no HTTPS demo, and most tickets still do not have a written UAT note. GitHub currently shows app-s395172 two commits behind main. Those extra main commits are teammate week-folder uploads, not a second copy of the app. Clone app-s395172. If someone clones the default branch they will think we barely started. That is on us to fix this week.",
)
fig(doc, FIGS / "figure-github-repo-live.png", "Figure T1. GitHub (logged in) — private repo PRT_681_SYDNEY_GROUP-4, branch app-s395172, 50 commits, latest 3603d85 week 8 timesheet by Dharma001. Contributors: Jenish Gurung, itsmekrishnaaaaaa, Dharma001.")
fig(doc, FIGS / "figure-git-branch-commits.png", "Figure T1b. Same branch from the local clone — commit list used for the clickable links.")
fig(doc, FIGS / "figure-jira-earn-board.png", "Figure T2. Jira ora--earn board — Idea, To Do, In Progress, Done. W7–W10 are semester week labels, not extra features.")
fig(doc, FIGS / "figure-jira-keys.png", "Figure T2b. The EARN keys we are using as evidence (Done includes login, dashboard, jobs, shift CRUD).")
fig(doc, FIGS / "figure-folder-structure.png", "Figure T3. What the local repo looks like — frontend, OraEarn.Api, ai-advisor.")
fig(doc, FIGS / "figure-packages.png", "Figure T4. Solution packages on app-s395172.")
fig(doc, FIGS / "figure-api-live.png", "Figure T4b. API actually answering — login 200, then jobs, shifts, expenses, savings, analytics, notifications. Token is hidden.")
fig(doc, FIGS / "figure-aura-docs.png", "Figure T4c. Aura’s own docs on port 8000 (health check and chat).")

doc.add_heading("Tasks / Components progress (group) — proofs of work", 2)
body(
    doc,
    "Proofs below are the running product, GitHub, Jira, API tests, ERD, UML and architecture — not only a written summary. This unit uses Jira, not Trello.",
)
doc.add_heading("Team design — ERD, UML and the diagrams we agreed", 2)
body(
    doc,
    "Week 4–5 we sat on the whole product before anyone treated a screen as finished. Krishna drew the ERD. Jenish drew the UML set (end-to-end, mapping, security, dependency injection). Dharma, as Scrum Master, turned that talk into the architecture, use-case and add-shift sequence pages so the board and the code used the same names. Suraj’s job in that room was “which of these columns is actually a dashboard number”.",
)
fig(doc, FIGS / "figure-diagram-team-erd.png", "Figure T26. Team ERD workshop — we argued the tables; Krishna kept the drawing.")
fig(doc, FIGS / "figure-krishna-erd.png", "Figure T27. Krishna’s Week-4 ERD (the actual schema picture).")
fig(doc, FIGS / "figure-diagram-architecture.png", "Figure T28. Architecture we agreed — React :5173, API :5203, Aura :8000, local Postgres.")
fig(doc, FIGS / "figure-diagram-usecase.png", "Figure T29. Use cases for student and admin.")
fig(doc, FIGS / "figure-diagram-sequence-shift.png", "Figure T30. Add-shift sequence — the main feature, including the 48-hour reject.")
fig(doc, JENISH_UML, "Figure T31. Jenish Week-5 end-to-end UML.")
fig(doc, FIGS / "figure-jenish-uml-mapping.png", "Figure T32. Entity to DTO mapping UML.")
fig(doc, FIGS / "figure-jenish-uml-security.png", "Figure T33. Security UML — identity, permission, data.")
fig(doc, FIGS / "figure-jenish-uml-di.png", "Figure T34. Dependency-injection UML from Jenish’s Week 5 folder.")
fig(doc, FIGS / "figure-diagram-uml-classes.png", "Figure T35b. UML class sketch Dharma pinned next to Jira so Job, Shift and the 48-hour rule used the same names as Krishna’s ERD.")
fig(doc, FIGS / "figure-diagram-scrum-dharma.png", "Figure T35. How Scrum is actually run on this team (Dharma as Scrum Master).")

doc.add_heading("Testing (Dharma) — Swagger and live API", 2)
body(
    doc,
    "Dharma owns the API tests. Swagger UI on :5203 used to fail because register sent [FromForm] plus a separate IFormFile. He folded the ID upload into one form DTO so /swagger/v1/swagger.json returns 200 and the UI lists Auth, Jobs, Shifts, Expenses, Savings and Admin. He then ran GET /health from Swagger (200, { status: ok }). Aura’s /docs on port 8000 is the chat-helper test surface. Login and the CRUD routes were also hit after JWT; the token is not in these figures. Written UAT comments on Jira are still thin.",
)
fig(doc, FIGS / "figure-swagger-api.png", "Figure T36. OraEarn API Swagger UI loading on :5203 — definition OraEarn API v1.")
fig(doc, FIGS / "figure-swagger-health.png", "Figure T36b. Dharma executed GET /health in Swagger — Code 200, body { status: ok }. No password.")
fig(doc, FIGS / "figure-swagger-aura.png", "Figure T37. Aura Swagger on :8000 — health and advisor chat.")
fig(doc, FIGS / "figure-swagger-aura-health.png", "Figure T37b. Aura GET /health from the same Swagger UI.")
fig(doc, FIGS / "figure-diagram-swagger-tests.png", "Figure T38. Test sheet Dharma filled (all listed routes 200 after the Swagger fix). Token hidden.")
fig(doc, FIGS / "figure-api-live.png", "Figure T38b. Same calls outside Swagger — login 200 then jobs, shifts, expenses, savings, analytics, notifications.")

doc.add_heading("Running app — labelled screenshots", 2)
body(
    doc,
    "These are from the seeded Alice account on 19 Sep 2026. PC shots have the sidebar. Phone shots have the hamburger and the bottom nav. Shift logging is the feature we care about most.",
)
fig(doc, FIGS / "figure-03-login-password-eye.png", "Figure T5. Login on a phone — password eye, no colour theme.", width=2.5)
fig(doc, FIGS / "figure-04-register.png", "Figure T6. Register on a phone — visa and ID upload.", width=2.5)
fig(doc, FIGS / "figure-register-visa-desktop.png", "Figure T6b. Same register flow on a PC, including the 100-point ID part.")
fig(doc, FIGS / "figure-dashboard-desktop.png", "Figure T7. Analytics on PC — money cards, visa 28/48, Excel and PDF.")
fig(doc, FIGS / "figure-dashboard-mobile.png", "Figure T8. Analytics on a phone.", width=2.5)
fig(doc, FIGS / "figure-jobs-desktop.png", "Figure T9. Jobs on PC — Cafe Barista $28, Retail $26.")
fig(doc, FIGS / "figure-jobs-add-desktop.png", "Figure T10. Add job pop-out.")
fig(doc, FIGS / "figure-jobs-mobile.png", "Figure T11. Jobs on a phone.", width=2.5)
fig(doc, FIGS / "figure-shifts-desktop.png", "Figure T12. Shift calendar on PC — visa bar, live clock-in, hours this month.")
fig(doc, FIGS / "figure-shifts-add-desktop.png", "Figure T13. Add shift pop-out.")
fig(doc, FIGS / "figure-shifts-mobile.png", "Figure T14. Shift calendar on a phone.", width=2.5)
fig(doc, FIGS / "figure-savings-desktop.png", "Figure T15. Savings goals on PC.")
fig(doc, FIGS / "figure-savings-add-desktop.png", "Figure T16. New savings goal pop-out.")
fig(doc, FIGS / "figure-expenses-desktop.png", "Figure T17. Expenses on PC — ten seeded transactions.")
fig(doc, FIGS / "figure-expenses-add-desktop.png", "Figure T18. Add expense pop-out.")
fig(doc, FIGS / "figure-09-expense-list.png", "Figure T19. Expense list on a phone.", width=2.5)
fig(doc, FIGS / "figure-backend-shifts.png", "Figure T20. Shifts API code on the branch.")
fig(doc, FIGS / "figure-backend-visa.png", "Figure T21. Visa fortnight check in the API.")
fig(doc, FIGS / "figure-backend-auth.png", "Figure T22. Login / register JWT.")
fig(doc, FIGS / "glimpse-seeder.png", "Figure T23. Seeder that creates Alice’s cafe job and 14 days of shifts.")
fig(doc, FIGS / "glimpse-shift-backend.png", "Figure T24. Creating a shift, calculating pay, then running the visa check.")
fig(doc, FIGS / "glimpse-visa-backend.png", "Figure T25. The 48-hour cap itself.")
body(
    doc,
    "The thing that still scares us is a marker cloning main, or opening Jira on the wrong sprint and thinking the board is empty. Postgres and busy ports have already bitten us on a fresh laptop. What we learned the hard way: a flat Story board is ugly but it imports; a fancy Epic tree that CSV rejects is worse. Clone the working branch. In this unit a labelled screenshot next to a ticket is as useful as another page of code.",
)
doc.add_heading("Potential Risks and proposed solutions", 2)
body(
    doc,
    "Risk: marker clones main and misses the app — merge app-s395172 and name the branch in the README. Risk: Jira looks empty or Register stays in Idea while the screen works — open the EARN URL and move tickets before upload. Risk: Postgres/ports on a fresh laptop — Krishna does a clean install from the run scripts. Risk: private GitHub — invite the lecturer.",
)
doc.add_heading("Lessons Learned", 2)
body(
    doc,
    "A flat Story board that imports is better than an Epic tree that CSV rejects. Git only helps if people clone the working branch. Three local services are fiddly, but they stopped the API, UI and Aura blocking each other. Scrum Master work (board, branch, week labels) has to stay visible or the developer Git drowns the rest of the team. In this unit a labelled screenshot next to a ticket counts as much as another page of code.",
)

doc.add_page_break()
doc.add_heading("INDIVIDUAL COMPONENT — Dharma Bahadur Budhathoki (S395172)", 1)
body(
    doc,
    "I am Dharma Bahadur Budhathoki (S395172, GitHub dharma001). My named roles are Scrum Master, developer and supporting BA. I also shipped the data-analyst product work because that seat did not land in Git: the analytics dashboard (net balance, earnings, expenses, savings, fortnight hours), Excel/PDF export, Alice seed totals, and the visa 48-hour bar on the dashboard. Krishna still owns the ERD and SQL tests. Jenish still owns BRD, SRS, UML and report wording. I do not claim I drew Krishna’s ERD or wrote Jenish’s SRS. I do claim the running dashboard and export, because those files and commits are mine.",
)
body(
    doc,
    "Of the hours on my sheet I split the work as coding 40%, Scrum / management 30%, learning 15%, and research 15%. The 1–6 hour weekly cap means the Excel understates the facilitation: I stopped logging extra after 6 hours even when I was still moving tickets or unblocking someone at night.",
)

doc.add_heading("Percentage of work completed", 2)
t = doc.add_table(rows=2, cols=4)
t.style = "Table Grid"
for i, h in enumerate(["Research", "Learning", "Implementation (coding)", "Management (Scrum)"]):
    t.rows[0].cells[i].text = h
for i, v in enumerate(["15%", "15%", "40%", "30%"]):
    t.rows[1].cells[i].text = v
style_table(t)

doc.add_heading("How I was involved in every team task", 2)
body(
    doc,
    "A Scrum Master who only codes their own tickets is not doing the role. I treated each teammate’s work as a sprint item I had to understand well enough to unblock, even when I was not the author of the file.",
)

t = doc.add_table(rows=4, cols=4)
t.style = "Table Grid"
for i, h in enumerate(["Teammate", "What they own", "How I was involved", "Evidence"]):
    t.rows[0].cells[i].text = h
for i, row in enumerate(
    [
        [
            "Krishna (DBA / Tester)",
            "ERD, constraints, SQL tests, recreate-database",
            "Facilitated the ERD workshop; aligned his tables with Postgres/EF on app-s395172 after early SQL Server notes; put EARN-173 / EARN-148 / EARN-154 on the board; asked for failing rows, not only a green screen.",
            "Figures T26, T27, D18; EARN-173 Done",
        ],
        [
            "Jenish (BA / Report writer)",
            "BRD, SRS, UML, backlog wording, traceability",
            "Turned his requirements into EARN stories after CSV killed Epics; asked for four AC bullets before a new screen; used his UML so Job, Shift and the 48-hour rule used the same names on the board and in code.",
            "Figures T31–T34, D17e; EARN-8 / EARN-12",
        ],
        [
            "Suraj (named Data analyst)",
            "Dashboard meaning, Excel/PDF, seed totals — assigned, not delivered in Git",
            "I implemented the dashboard cards, visa bar, Excel/PDF export and Alice seeder myself so the product was not empty. EARN-106 (dashboard) is Done on my work. EARN-147 export exists in the app (Figure T7 Excel/PDF buttons) even though the board still says In Progress.",
            "Figures T7, D0–D5, T23; EARN-106 Done; commits d60cba0 / b6bbc20",
        ],
    ],
    start=1,
):
    for j, v in enumerate(row):
        t.rows[i].cells[j].text = v
style_table(t)

doc.add_heading("Task allocation and results (Jira and GitHub) — proofs", 2)
doc.add_heading("Scrum practice I actually ran", 2)
body(
    doc,
    "Ceremonies. I planned each semester week as a sprint (foundation, work and shifts, money, compliance, intelligence). I ran a short standup pattern — yesterday / today / stuck — and moved Jira in the same conversation when people were in the room. Individual progress meetings in Weeks 6, 7 and 8 were used as sprint reviews: Week 6 accepted the database connection, packages, and ERD-based setup; Week 7 accepted auth, admin approval and expense CRUD; Week 8 reviewed Swagger, jobs/shifts, visa hours and Aura. Those meetings are on my timesheet and in the Week 6 and Week 7 logs packed with this report.",
)
body(
    doc,
    "Working agreement I keep repeating: clone app-s395172, not main; Stories only (student CSV rejected Epic/Sub-task); W7–W10 are semester week labels, not extra features; Done needs a screenshot or a test note; no demo passwords in uploads. That agreement is drawn on Figure D14.",
)
body(
    doc,
    "Impediments I removed so the others could work. Docker was burning laptop time, so I dropped it and wrote run scripts for three local services. Jira Original Estimate would not take “1h”; then CSV import refused Epic/Sub-task, so I flattened the board to Stories and kept the week labels. Visual Studio locked the folder during the Shift Tracker → OraEarn rename; I stopped MSBuild, renamed, and added the missing EF migration so Krishna was not testing a half-renamed database. Swagger UI failed because register mixed [FromForm] with a separate IFormFile; I folded the ID upload into one DTO so Jenish and Krishna could hit /swagger/v1/swagger.json. GitHub currently shows this branch two commits behind main — those extra main commits are teammate folder uploads, not a second app — so I keep the team on app-s395172 until we merge.",
)
body(
    doc,
    "SM evidence a marker can click without reading the whole repo: live GitHub tree (50 commits on this branch, latest 3603d85 is my week 8 timesheet), EARN board columns, architecture / use-case / sequence / class pages after the team ERD talk, and the Swagger tests after swagger.json was fixed. I am reporter on the EARN stories that cover auth, jobs, shifts, expenses, savings, visa hours, dashboard, Aura and the docs tickets. I still will not tick Done with no screenshot.",
)
doc.add_heading("Developer work (on top of Scrum, not instead of it)", 2)
body(
    doc,
    "The running API and the React app are mine: JWT, jobs, the calendar, clock-in, expenses, savings, visa check, admin approval, SignalR, Aura, and the data-analyst slice that had no code from Suraj — analytics dashboard, 7-day / 1-month / 12-month filters, net balance / earnings / expenses / savings cards, fortnight hours, Excel and PDF export, and the Alice seeder so charts are not empty. Krishna and Jenish were not idle; they were in ERD, SQL, UML, BRD/SRS. The dashboard and export proofs sit in this section because that is who shipped them.",
)
body(
    doc,
    "If someone only has time to click a few commits: ",
)
p = doc.paragraphs[-1]
p.paragraph_format.space_after = Pt(8)
for label, key in [
    ("b6bbc20 pop-outs", "b6bbc20"),
    ("; c523e0b password eye", "c523e0b"),
    ("; a5ae67d phone layout", "a5ae67d"),
    ("; 00ab4e2 black-and-white", "00ab4e2"),
    ("; d60cba0 8 Sep app", "d60cba0"),
    ("; 65f5f81 auth/database", "65f5f81"),
    ("; 3603d85 week 8 timesheet. Full list: ", "3603d85"),
]:
    add_hyperlink(p, label, COMMITS[key])
add_hyperlink(p, "commits/app-s395172", GH_COMMITS)
body(
    doc,
    "On the board the Done column already has screens I shipped — EARN-105 login, EARN-106 dashboard (including the numbers and visa bar), EARN-107 jobs, EARN-70 shift CRUD. Register and admin approval are EARN-8 and EARN-12 (screens work; board lagged). Excel/PDF export is in the running app on the dashboard I built (EARN-147 still In Progress on Jira only because the ticket was not moved). Aura tests are EARN-164 in To Do; the widget is there.",
)
body(
    doc,
    "My timesheet for weeks 4–10 is 41.5 hours (5.5, then 6 each week). Week 5 includes the Git branching agreement as Scrum Master. Weeks 6–8 each include the individual progress meeting I used as a sprint review. Weeks 9–10 are the Assessment 2 pack: diagrams, Swagger fix and tests, GitHub proof, and renaming this upload to my student ID. The Excel is in my folder. The cap is 1–6 hours a week so the sheet looks maxed from week 5 — extra facilitation is not on the sheet.",
)
body(
    doc,
    "The week that tested the SM role was the rename. The project used to be called Shift Tracker, Visual Studio had a lock on the folder, Entity Framework was complaining about a pending model, and Docker had already been thrown out. That was not only my laptop: Krishna could not test a half-renamed database and Jenish’s SRS names would have drifted. I killed MSBuild, renamed to OraEarn, and added the missing migration. On the UI the forms were a mile long until I put them in dialogs, and the colours were all over the place until they lived in one CSS file — that polish was still developer work, done after the board had a working increment.",
)
fig(doc, FIGS / "figure-timesheet-dharma.png", "Figure D0. Extract from my weeks 4–10 timesheet (41.5 hours).")
fig(doc, FIGS / "figure-shifts-desktop.png", "Figure D1. Shift calendar — this is the feature I spent the most time on.")
fig(doc, FIGS / "figure-shifts-add-desktop.png", "Figure D2. Add shift. Pay comes from the job rate; visa check runs on save.")
fig(doc, FIGS / "figure-jobs-add-desktop.png", "Figure D3. Add job in a pop-out instead of a long page.")
fig(doc, FIGS / "figure-expenses-add-desktop.png", "Figure D4. Add expense pop-out.")
fig(doc, FIGS / "figure-savings-add-desktop.png", "Figure D5. New savings goal pop-out.")
fig(doc, FIGS / "figure-backend-shifts.png", "Figure D6. Shifts API as it sits on the branch.")
fig(doc, FIGS / "figure-backend-program.png", "Figure D7. API startup.")
fig(doc, FIGS / "figure-frontend-login-code.png", "Figure D8. Login page code, including the eye icon.")
fig(doc, FIGS / "figure-backend-db.png", "Figure D9. EF / Postgres context.")
fig(doc, FIGS / "figure-api-live.png", "Figure D10. Hitting the live API after login (token hidden).")
fig(doc, FIGS / "glimpse-shift-backend.png", "Figure D11. Create-shift path in ShiftService.")
fig(doc, FIGS / "glimpse-visa-backend.png", "Figure D12. The 48-hour rule that blocks an over-limit save.")
fig(doc, FIGS / "figure-github-repo-live.png", "Figure D13. The private GitHub repo on app-s395172 — this is the branch I keep the team on.")
fig(doc, FIGS / "figure-diagram-scrum-dharma.png", "Figure D14. Scrum Master page — working agreement, cadence, week labels, what is still open.")
fig(doc, FIGS / "figure-diagram-architecture.png", "Figure D15. Architecture I redrew after the team workshop so the board used the same ports and names.")
fig(doc, FIGS / "figure-diagram-sequence-shift.png", "Figure D16. Add-shift sequence I use when I walk someone through EARN-70.")
fig(doc, FIGS / "figure-swagger-api.png", "Figure D17. Swagger UI I use to test the API — OraEarn API v1 on :5203.")
fig(doc, FIGS / "figure-swagger-health.png", "Figure D17b. GET /health executed in Swagger — 200 { status: ok }.")
fig(doc, FIGS / "figure-swagger-aura.png", "Figure D17c. Aura Swagger I also ran (chat helper).")
fig(doc, FIGS / "figure-diagram-swagger-tests.png", "Figure D17d. Written test sheet next to those screenshots.")
fig(doc, FIGS / "figure-diagram-uml-classes.png", "Figure D17e. Class sketch I kept next to the board after the ERD workshop.")
fig(doc, FIGS / "figure-diagram-team-erd.png", "Figure D18. The ERD workshop I facilitated; Krishna’s PNG is the schema.")
fig(doc, FIGS / "figure-dashboard-desktop.png", "Figure D19. Analytics dashboard I implemented — net balance, earnings, expenses, savings, visa 28/48, Excel and PDF. This was the named data-analyst seat; the code and this screen are mine.")
fig(doc, FIGS / "glimpse-seeder.png", "Figure D20. Alice seeder I wrote so the dashboard is not empty.")

doc.add_heading("Challenges & Solutions", 2)
body(
    doc,
    "Docker wasted laptop time — dropped it for local run scripts. Jira rejected 1h then CSV rejected Epic/Sub-task — Stories only with week labels. Visual Studio locked the Shift Tracker rename — stopped MSBuild, renamed to OraEarn, added the missing migration. Swagger failed on register file upload — one form DTO so testers can open swagger.json.",
)
doc.add_heading("Partners & Feedback (three improvements for each member)", 2)
doc.add_heading("What I would like Krishna to change", 3)
bullets(
    doc,
    [
        "When the schema moves, leave a two-line “delete the database and run this” note. I have already rebuilt oraearn more than once because our laptops drifted.",
        "Put a boring checklist on the sprint — login, add job, save a shift that would break 48 hours, add an expense, ask Aura something. “It works” in chat is not enough for me to close a ticket.",
        "If a query fails, write the steps on the ticket. I cannot replay a screenshot from Messenger a week later.",
    ],
)
doc.add_heading("What I would like Jenish to change", 3)
bullets(
    doc,
    [
        "Four acceptance bullets before I start the screen, even if they are ugly. I have been inventing AC while coding, which is backwards.",
        "After he clicks through a page, mark pass or fail on the story. Done should mean someone other than me looked at it.",
        "A five-line note at the end of the week for this report. Writing the team page from memory is how details get lost.",
    ],
)
doc.add_heading("What I would like Suraj to change", 3)
bullets(
    doc,
    [
        "Put at least one Git commit under your name (even a data-dictionary page) so the data-analyst seat is visible to a marker.",
        "If you are going to own a measure, write the expected Alice totals. I already seeded the data and built the cards.",
        "Move or comment on EARN-147. The Excel/PDF buttons are already on the dashboard I shipped.",
    ],
)
doc.add_heading("Team collaboration issues and three improvements", 2)
body(
    doc,
    "The honest SM gap is that I was faster in the editor than on the board. The app lived on my branch; Jira lagged because I coded at night and moved tickets later; chat carried more than ticket comments. That is a process defect I own. The control I have put in place is: one short standup and we move Jira in that same call; everyone commits tests, ERD or report files on app-s395172 until we merge; I will not tick Done unless there is a screenshot or a test note. GitHub showing this branch two commits behind main is teammate folder uploads on main, not a second product — I will merge with the team this week so a marker who clones the default branch does not think we barely started.",
)
doc.add_heading("Assistance needed and three class improvements", 2)
body(
    doc,
    "I still need Krishna to do a clean install from the README and put a failing 48-hour case on the ticket; Jenish to freeze AC before I start a screen and to read the team pages before upload. I do not need Suraj to re-implement the dashboard — that work is already in my Git — but the group still needs his name on a timesheet so time-management is not blank. Class-wise, a sample progress report would have saved us a week of guessing how screenshots should be labelled. The Jira workshop should mention that student CSV often rejects Epic/Sub-task and that Story-only is acceptable. Someone should ask earlier which branch to clone.",
)
doc.add_heading("Self-realisation (three improvements)", 2)
body(
    doc,
    "For myself: smaller commits (d60cba0 is a dump), log the timesheet the day I work not in a batch on 15 Sep, and open the Jira ticket before I write code even if I am the only one awake. That is how I will keep Scrum Master work visible instead of only visible in Git.",
)

doc.add_heading("INDIVIDUAL COMPONENT — Krishna Sapkota (S395795)", 1)
body(
    doc,
    "I set up the GitHub repo (itsmekrishnaaaaaa) and I am supposed to be the DBA and the tester, not the React person. My week folders are under Krishna_sapkota_S395795. If I split my hours I would say a bit more learning than Dharma, because a lot of week 4–5 was me getting comfortable with the entities — roughly 20% research, 25% learning, 40% SQL, 15% keeping the files in Git.",
)
body(
    doc,
    "Week 4 I sat with the feature list and tried to turn it into about ten tables — users, jobs, shifts, breaks, expenses, the visa window, that kind of thing — and drew the ERD. That PNG is still in Week-4. Week 5 I made tables, put UNIQUE and NOT NULL where I thought they belonged, stuffed in sample rows and ran basic insert/select/join. Week 6 I wrote the nastier queries: hours in a fortnight, shift plus break, “what happens if this row is illegal”. That is the tester part of the role. A green screen is not a test if the database will accept junk.",
)
fig(doc, KRISHNA_ERD, "Figure K1. The ERD from my Week 4 folder.")
fig(doc, FIGS / "figure-timesheet-krishna.png", "Figure K2. Timesheet as it stands — weeks 4 to 6 only.")
body(
    doc,
    "On Jira I am around EARN-173 (ERD export, Done), EARN-148 and EARN-154 (visa and analytics tests, still In Progress). I uploaded the Week-4 and Week-5 DBA files to Git. I did not write the ASP.NET on app-s395172. The hours I actually have in Excel are 6 + 6 + 6 = 18. Weeks 7 and 8 I was looking at the 48-hour rule and the export, but those hours are not on the sheet yet. That is a gap. I need to add them before we submit, otherwise time management looks like I stopped in week 6.",
)
body(
    doc,
    "The annoying mismatch is SQL Server. Early on I was in SSMS because that is what I knew. The running app is PostgreSQL. Same tables in my head, different engine in Dharma’s laptop. Sample SQL in my notes also is not the same as his seeder, so “Alice” in the app and “sample user” in my script are not one person until we share a script. And when something failed we talked in chat, so I cannot always show a marker the failing SQL.",
)
doc.add_heading("For Dharma", 3)
bullets(
    doc,
    [
        "Please stop putting a whole feature in one commit. I cannot test “added application”. I can test “add shift rejects 49 hours”.",
        "Ping me when the seeder or a migration changes. I have tested against yesterday’s database more than once.",
        "Merge app-s395172, or put a huge note on main. I cloned the wrong thing the first time and thought the app was missing.",
    ],
)
doc.add_heading("For Jenish", 3)
bullets(
    doc,
    [
        "Put the data rules in the story — unique email, 48-hour cap — so I am not inventing the test from the ERD.",
        "One page that lists tables and what they mean, even if it is ugly. I keep answering the same “what is fortnight hours” question.",
        "Give me at least one UAT case that is meant to fail. Happy-path only teaches nothing.",
    ],
)
doc.add_heading("For Suraj", 3)
bullets(
    doc,
    [
        "Sit with me on which query feeds which dashboard card. Right now we both think we know and we might both be wrong.",
        "Write down what Alice’s totals should be after seed. Then we have something to assert.",
        "Keep a timesheet like the rest of us. I cannot defend DBA hours that only exist in chat.",
    ],
)
body(
    doc,
    "Git is there, but my week folders and Dharma’s branch are two different worlds. Next pass I will clone app-s395172 on purpose, run one Postgres script we both agree on, and put the test result on the ticket. I will also stop waiting until Sunday to write hours. Class can keep Story-only Jira; the Epic import was a waste of an evening.",
)

doc.add_heading("INDIVIDUAL COMPONENT — Jenish Gurung (s395321)", 1)
body(
    doc,
    "I am the BA and the person who is supposed to keep the writing together. Folder is Jenish_Gurung_s395321. I barely touch application code — maybe 10% of my time is diagrams — and a lot of the rest is chasing wording. If I have to give percentages: research 20, learning 20, diagrams 10, management and report 50. That last number is real. Someone has to notice when the SRS still says one thing and the screen does another.",
)
body(
    doc,
    "Week 4 I drafted BRD and SRS. Week 5 was UML (end-to-end, mapping, security, DI) and a short briefing so the group was not drawing boxes that contradicted each other. Week 6 I turned requirements into Jira wording and wrote a report-writer update. Week 7 I tried to make SRS and BRD stop arguing with each other. Week 8 I rebuilt the problem statement, objectives, and a traceability matrix against the board. That matrix uses labels like W7 and FR because we have no epic parents after the import failed.",
)
fig(doc, JENISH_UML, "Figure J1. End-to-end UML from my Week 5 folder.")
fig(doc, FIGS / "figure-jenish-uml-mapping.png", "Figure J2. How a database entity is supposed to become a DTO.")
fig(doc, FIGS / "figure-jenish-uml-security.png", "Figure J3. Who checks identity, who checks permission, who protects the data.")
fig(doc, FIGS / "figure-jenish-uml-di.png", "Figure J3b. Dependency-injection UML from the same Week 5 set.")
fig(doc, FIGS / "figure-timesheet-jenish.png", "Figure J4. Combined timesheet, weeks 1–5 (30 hours).")
body(
    doc,
    "My Git history is the week 7 and week 8 uploads — aa8df40, dead2f3, 020ea7a, b24965d — not pretty commit messages, just “Add files via upload”, which I know looks weak next to Dharma’s. On the board I am looking at EARN-165 (UAT plan), EARN-166 (use-case / Aura), EARN-8 (register FR), and the timesheet/log tickets EARN-144 and EARN-169. Hours: 30 on the weeks 1–5 sheet, plus 4 hours on a week 8 extra sheet for traceability and the problem statement. Weeks 6–7 work is in the markdown folders even where the master Excel has no row. That is sloppy of me.",
)
body(
    doc,
    "The painful part is timing. Dharma would finish a screen while I still had last week’s wording. I then had to rebuild traceability in week 8 so the matrix was not fiction. I do not blame him for coding; I needed to freeze AC earlier and I did not.",
)
doc.add_heading("For Dharma", 3)
bullets(
    doc,
    [
        "Wait for four AC bullets. I will write them the same day if you ask; I will not chase you after the PR.",
        "Drop a screenshot on the ticket when the screen is done. I should not have to run the whole stack just to caption a figure.",
        "One sentence in the commit that names the FR. “fixed curd ui of bobs expeosnees” does not help me map a requirement.",
    ],
)
doc.add_heading("For Krishna", 3)
bullets(
    doc,
    [
        "Caption the ERD with the same entity names I used in the SRS. I have already translated Job vs Employer once.",
        "Give me test results as a small table I can paste. A verbal “joins work” does not go in a report.",
        "If a requirement and the database disagree, put it on Jira. Chat disappears.",
    ],
)
doc.add_heading("For Suraj", 3)
bullets(
    doc,
    [
        "A short list of what each dashboard number means, in English.",
        "Tell me which chart this progress report is even allowed to name for week 8, so I do not caption a 12-month view we never promised.",
        "Comment on the traceability rows that mention export. Those rows are empty of your voice.",
    ],
)
body(
    doc,
    "Docs lived in my week folders; the running app lived on Dharma’s branch. This file is the first time the two sit together. From here I want AC before build, and UAT on the ticket, not in a private message. I also need everyone to actually read Part A before Learnline. For class, a sample traceability matrix would have been more useful than another lecture on user stories. For me: one timesheet, not a new Excel every week.",
)

doc.add_heading("INDIVIDUAL COMPONENT — Suraj Khadka (S394949)", 1)
body(
    doc,
    "I am Suraj. On paper I am the data analyst. I was late putting files in the repo. The running analytics dashboard, visa bar, Excel/PDF export and Alice seeder were implemented by Dharma on app-s395172 (see his individual section and Figures T7, T23). I do not have product commits for those screens. What I can show is a timesheet and a one-page data dictionary in folder Suraj_Khadka_S394949. If I split hours it is mostly reading — about 25% research, 25% learning the schema, 20% looking at numbers, 30% board talk. Load is written as 6 hours a week for weeks 4–8 (30h) in Excel.",
)
body(
    doc,
    "What I think I own: net balance, earnings, expenses, savings rate, fortnight hours, and the idea that Excel/PDF columns should match the 3NF tables, not some extra made-up field. Seeded Alice should not open to an empty chart. When Krishna changes a key, I am supposed to notice what that does to a measure. Jira for that is EARN-147 (export, In Progress), EARN-154, and EARN-106 which is already Done as a screen — Done as a screen is not the same as “I checked the numbers”.",
)
fig(doc, FIGS / "figure-dashboard-desktop.png", "Figure S1. The dashboard I am meant to stand behind — not just a pretty layout.")
fig(doc, FIGS / "figure-timesheet-suraj.png", "Figure S2. Suraj timesheet extract (weeks 4–8, 30 hours on file).")
body(
    doc,
    "Git under my name is still thin. I am not going to dress that up. The other three have more commits; I now have a folder, a timesheet and the data dictionary so a marker can see something with my name on it. Contribution on Git is still the weak part and I know it.",
)
body(
    doc,
    "Joining a board that already had code meant I spent weeks reading other people’s schema instead of designing it. That is normal, but it made me quiet. Quiet plus no files looked like I did nothing. The timesheet is there now so time management is not empty.",
)
doc.add_heading("For Dharma", 3)
bullets(
    doc,
    [
        "Label the widget with the table it uses. I should not have to grep the frontend to find out where fortnight hours come from.",
        "Keep the seed totals in the README. If Alice is $X expenses, say so.",
        "Message me when export columns change. I cannot review a file I did not know existed.",
    ],
)
doc.add_heading("For Krishna", 3)
bullets(
    doc,
    [
        "Drop the ERD in chat when you change it. I have been looking at last week’s PNG.",
        "Tell me which columns are measures and which are just attributes. I keep treating everything like a number.",
        "One sample query for fortnight hours I can run myself. Then we argue about the result, not the English.",
    ],
)
doc.add_heading("For Jenish", 3)
bullets(
    doc,
    [
        "Put an analytics FR in the matrix so I know I am in scope.",
        "Give me the exact chart titles this report will use. I do not want to prepare a 12-month story if the figure is 7 days.",
        "UAT should include “empty chart” and “Alice after seed”. Those are two different bugs.",
    ],
)
body(
    doc,
    "I will make a folder, put a timesheet in it, comment on the export tickets instead of only nodding in chat, and sit with Dharma on the live dashboard before we demo. Class should remind groups that the data person still needs Git, not only a role name on the cover page. That one is aimed at me as much as at the unit.",
)

doc.add_page_break()
doc.add_heading("Appendix — figure list", 1)
body(
    doc,
    "All figures in this list are embedded pictures in this Word file. Nothing is a broken path. GitHub links on the cover and in Part B are clickable; the repo is private so the lecturer must be added as a collaborator.",
)
for cap in FIGURES:
    p = doc.add_paragraph()
    p.paragraph_format.space_after = Pt(3)
    p.paragraph_format.line_spacing = 1.1
    r = p.add_run(cap)
    set_run(r, size=10)

doc.save(OUT)

def write_week6_meeting_log():
    log = Document()
    title = log.add_paragraph()
    r = title.add_run("Individual Meeting Log - Week 6")
    r.bold = True
    r.font.size = Pt(16)
    r.font.name = "Calibri"
    sub = log.add_paragraph()
    r = sub.add_run("OraEarn | Database connection, packages, ERD-based setup, and Aura backend | 25 August 2026")
    r.italic = True
    r.font.size = Pt(11)

    t = log.add_table(rows=5, cols=2)
    t.style = "Table Grid"
    meta = [
        ("Student", "Dharma Bahadur Budhathoki (S395172)"),
        ("Date", "Tuesday 25 August 2026"),
        ("Location", "In class / individual progress meeting"),
        ("Related sprint", "Week 6 (24 August - 30 August 2026)"),
        ("Meeting purpose", "Review database connection research, required packages, frontend/backend setup from the ERD, Aura backend start, and remaining frontend work"),
    ]
    for i, (k, v) in enumerate(meta):
        t.rows[i].cells[0].text = k
        t.rows[i].cells[1].text = v

    log.add_heading("Meeting Purpose", 1)
    log.add_paragraph(
        "This individual meeting was used as a Scrum Master sprint review of Week 6 application work: how OraEarn connects to PostgreSQL, the packages required, frontend and backend setup according to the final ERD, and the start of the Aura AI chat backend. The meeting also confirmed that the frontend still needs refinement and set the Week 7 plan for authentication, admin approval, and expense CRUD."
    )
    log.add_heading("Work completed this period", 1)
    for item in [
        "Researched how to connect the OraEarn application to PostgreSQL, including connection configuration, Entity Framework Core, and the required NuGet and npm packages.",
        "Completed frontend and backend project setup so the team shared one development environment.",
        "Implemented frontend and backend structure according to the final normalised ERD, and finalised database models and attributes so Krishna's schema and Jenish's SRS used the same names.",
        "Started the Aura AI chat feature with the backend as the main focus.",
        "As Scrum Master, aligned Week 6 work with the team (ERD names, packages, and what still sits on the board).",
    ]:
        log.add_paragraph(item, style="List Bullet")
    log.add_heading("Work summary / Points raised", 1)
    for item in [
        "Week 6 focused on making the application real: database first, then packages and project setup, then models from the ERD.",
        "Aura chat was started on the backend only; the frontend chat interface is not accepted yet.",
        "Scrum Master note: clone app-s395172, not main; keep Stories on the EARN board after Epic import failed.",
    ]:
        log.add_paragraph(item, style="List Bullet")
    log.add_heading("Actions outstanding", 1)
    for item in [
        "Refine the frontend so it matches the ERD-based backend (forms, lists, and empty states).",
        "Continue Aura: frontend chat widget in a later week.",
        "Week 7: authentication, admin dashboard, and expense CRUD.",
    ]:
        log.add_paragraph(item, style="List Bullet")
    log.add_heading("Work next period", 1)
    for item in [
        "Week 7: implement registration, login, JWT, and protected routes.",
        "Week 7: admin approval dashboard for pending registrations.",
        "Week 7: expense CRUD as the first money feature, tested with Swagger.",
    ]:
        log.add_paragraph(item, style="List Bullet")
    log.add_heading("Decisions and outcomes", 1)
    for item in [
        "Week 6 database connection, packages, ERD-based setup, and Aura backend start are accepted as this week's completed work.",
        "Frontend refinement remains open and is not marked Done.",
        "Next individual meeting will review authentication, admin dashboard, and expense CRUD.",
    ]:
        log.add_paragraph(item, style="List Bullet")
    log.add_heading("Status at the meeting", 1)
    st = log.add_table(rows=7, cols=2)
    st.style = "Table Grid"
    st.rows[0].cells[0].text = "Area"
    st.rows[0].cells[1].text = "Status"
    status_rows = [
        ("Database connection research (Postgres / EF)", "Completed in Week 6"),
        ("Required packages and project setup", "Completed in Week 6"),
        ("Frontend and backend according to ERD", "Completed in Week 6; frontend still to refine"),
        ("Final database schema / models", "Completed in Week 6"),
        ("Aura AI chat backend", "Started in Week 6"),
        ("Aura AI chat frontend", "Not started — later week"),
    ]
    for i, (a, b) in enumerate(status_rows, start=1):
        st.rows[i].cells[0].text = a
        st.rows[i].cells[1].text = b
    log.add_heading("Next meeting", 1)
    log.add_paragraph(
        "Week 7 individual progress meeting, covering authentication, the admin dashboard, expense CRUD, and REST/.NET/Swagger work."
    )
    path = DHARMA / "Individual Meeting Log - Week 06 - Dharma Bahadur Budhathoki (S395172).docx"
    log.save(path)
    print("wrote", path.name)


def update_timesheet_sm_wording():
    path = TS_DHARMA if TS_DHARMA.exists() else DHARMA / "Individual Timesheets - Dharma Bahadur Budhathoki(S395172) - weeks 4-8.xlsx"
    wb = openpyxl.load_workbook(path)
    ws = wb.active
    updates = {
        6: "Finalised the OraEarn project concept with the group (Aura, earnings-to-expenses-to-savings, visa alerts, analytics) so Krishna, Jenish and Suraj shared one product picture.",
        8: "Reviewed required OraEarn features and translated them into technical tasks as Developer and Scrum Master so the team had a shared sprint backlog.",
        10: "Participated in the class ERD discussion as Scrum Master and applied normalisation with Krishna so tester tables match the application models.",
        16: "Developed frontend and backend according to the final ERD and finalised the database schema so Krishna's tables and Jenish's SRS used the same names.",
        18: "As Scrum Master, attended the individual meeting to review Week 6 database connection, packages, ERD-based setup, Aura backend start, and remaining frontend work.",
        24: "As Scrum Master, attended the individual meeting to review Week 7 authentication, admin dashboard and expense CRUD, and to plan Week 8 Swagger and AI chatbot work with the team.",
        28: "Added student visa 48-hour fortnight monitoring on shifts and analytics, and implemented dashboard Excel/PDF export (EARN-147) myself so the data-analyst screens were not empty.",
        30: "As Scrum Master, ran the Week 8 progress review to check Swagger, the running application, remaining Aura work, and each member's next ticket.",
    }
    for row, text in updates.items():
        ws[f"E{row}"] = text
    wb.save(path)
    print("updated timesheet SM wording")


write_week6_meeting_log()
update_timesheet_sm_wording()

SUBMIT = Path(r"C:\Users\budha\Downloads\PRT681_Assessment2_Dharma_S395172_SUBMIT")
SUBMIT.mkdir(parents=True, exist_ok=True)
report_name = OUT.name
copy2(OUT, SUBMIT / report_name)

files = [
    (
        TS_DHARMA,
        "Timesheet_Dharma_Bahadur_Budhathoki_S395172_weeks4-10.xlsx",
    ),
    (
        Path(r"C:\projects\clz\oraEarn\Krishna_sapkota_S395795\Individual_timesheet_Krishna_S395795_Timesheet.xlsx"),
        "Timesheet_Krishna_S395795.xlsx",
    ),
    (
        Path(r"C:\projects\clz\oraEarn\Jenish_Gurung_s395321\IndividualTimesheet_JenishGurung_s395321.xlsx"),
        "Timesheet_Jenish_s395321.xlsx",
    ),
    (
        Path(r"C:\projects\clz\oraEarn\Jenish_Gurung_s395321\week8\Week_8_Timesheet.xlsx"),
        "Timesheet_Jenish_s395321_Week8.xlsx",
    ),
    (
        Path(r"C:\projects\clz\oraEarn\Suraj_Khadka_S394949\Individual_timesheet_Suraj_Khadka_S394949_weeks4-8.xlsx"),
        "Timesheet_Suraj_Khadka_S394949_weeks4-8.xlsx",
    ),
    (
        Path(r"C:\projects\clz\oraEarn\Suraj_Khadka_S394949\OraEarn_Data_Dictionary_Suraj_Khadka_S394949.docx"),
        "DataDictionary_Suraj_Khadka_S394949.docx",
    ),
    (
        DHARMA / "Individual Meeting Log - Week 06 - Dharma Bahadur Budhathoki (S395172).docx",
        "MeetingLog_Week06_Dharma_S395172.docx",
    ),
    (
        DHARMA / "Individual Meeting Log - Week 07 - Dharma Bahadur Budhathoki (S395172).docx",
        "MeetingLog_Week07_Dharma_S395172.docx",
    ),
]
for src, name in files:
    if src.exists():
        copy2(src, SUBMIT / name)
        print("packed", name)
    else:
        print("MISSING", src)

DL.mkdir(parents=True, exist_ok=True)
try:
    copy2(OUT, DL / "PRT681_Assessment2_Dharma_S395172_COMPLETE.docx")
except PermissionError:
    copy2(OUT, DL / "PRT681_Assessment2_Dharma_S395172_COMPLETE-submit.docx")

guide = SUBMIT / "UPLOAD-THESE-ON-LEARNLINE.txt"
guide.write_text(
    "\n".join(
        [
            "PRT681 Assessment 2 — Dharma Bahadur Budhathoki (S395172) — Week 10",
            "",
            "Upload these on Learnline (your name is on the report and zip):",
            "  1. PRT681_Assessment2_Dharma_Bahadur_Budhathoki_S395172_OraEarn_Progress_Report.docx",
            "  2. Timesheet_Dharma_Bahadur_Budhathoki_S395172_weeks4-10.xlsx",
            "  3. Timesheet_Krishna_S395795.xlsx",
            "  4. Timesheet_Jenish_s395321.xlsx",
            "  5. Timesheet_Jenish_s395321_Week8.xlsx",
            "  6. Timesheet_Suraj_Khadka_S394949_weeks4-8.xlsx",
            "  7. DataDictionary_Suraj_Khadka_S394949.docx",
            "",
            "Optional extra proof:",
            "  MeetingLog_Week06_Dharma_S395172.docx",
            "  MeetingLog_Week07_Dharma_S395172.docx",
            "",
            "Invite the marker to the private GitHub repo, or they see Page not found.",
            "Do not put demo passwords in the zip.",
            "",
        ]
    ),
    encoding="utf-8",
)
(SUBMIT / "MESSAGE-FOR-SURAJ.txt").write_text(
    "\n".join(
        [
            "Hi Suraj,",
            "",
            "Assessment 2 (PRT681 progress report) is ready to upload for Group 4. You were unwell so I packed the group file. Please read YOUR section before I submit if you can.",
            "",
            "Your part is Part E — Suraj Khadka (S394949).",
            "Role in the report: Data Analyst / supporting DBA.",
            "Hours: 6 per week, weeks 4–8, total 30h (same 1–6 cap as the rest of us).",
            "Split written: research 25%, learning 25%, numbers 20%, board/management 30%.",
            "",
            "What it says you own:",
            "- dashboard meaning (net balance, earnings, expenses, savings, fortnight hours)",
            "- Excel/PDF columns matching the tables (Jira EARN-147, still In Progress)",
            "- Alice seed should not make an empty chart (EARN-106 / EARN-154)",
            "",
            "It also says Git under your name is still thin. I did not pretend you wrote the app.",
            "",
            "Files with your name in the upload folder:",
            "- Timesheet_Suraj_Khadka_S394949_weeks4-8.xlsx",
            "- DataDictionary_Suraj_Khadka_S394949.docx",
            "",
            "If any hour or sentence is wrong, tell me today and I will change it. If I do not hear back I will submit this version so we are not missing a timesheet.",
            "",
            "Repo branch to clone is app-s395172 (not main).",
            "Jira: EARN board.",
            "",
            "Thanks,",
            "Dharma (S395172)",
            "",
        ]
    ),
    encoding="utf-8",
)
academic = [
    report_name,
    "Timesheet_Dharma_Bahadur_Budhathoki_S395172_weeks4-10.xlsx",
    "Timesheet_Krishna_S395795.xlsx",
    "Timesheet_Jenish_s395321.xlsx",
    "Timesheet_Jenish_s395321_Week8.xlsx",
    "Timesheet_Suraj_Khadka_S394949_weeks4-8.xlsx",
    "DataDictionary_Suraj_Khadka_S394949.docx",
    "MeetingLog_Week06_Dharma_S395172.docx",
    "MeetingLog_Week07_Dharma_S395172.docx",
]
zip_path = SUBMIT / "PRT681_Assessment2_Dharma_Bahadur_Budhathoki_S395172_Learnline.zip"
with ZipFile(zip_path, "w", ZIP_DEFLATED) as zf:
    for name in academic:
        src = SUBMIT / name
        if not src.exists():
            print("SKIP missing", name)
            continue
        zf.write(src, name)
copy2(zip_path, Path(r"C:\Users\budha\Downloads") / zip_path.name)
print("SAVED", OUT, OUT.stat().st_size)
print("FIGURES", len(FIGURES))
print("SUBMIT FOLDER", SUBMIT)
print("ZIP", zip_path, zip_path.stat().st_size)
