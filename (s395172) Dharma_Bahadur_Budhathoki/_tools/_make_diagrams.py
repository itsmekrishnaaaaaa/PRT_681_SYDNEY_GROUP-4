"""Black-and-white academic diagrams for Assessment 2 (no extra packages)."""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

OUT = Path(r"C:\projects\clz\oraEarn\(s395172) Dharma_Bahadur_Budhathoki\figures")
W, H = 1400, 900
BG, INK, MUTED, BOX, ACCENT = (255, 255, 255), (28, 28, 28), (90, 90, 90), (245, 245, 245), (40, 40, 40)


def font(size, bold=False):
    for name in (
        "C:/Windows/Fonts/calibrib.ttf" if bold else "C:/Windows/Fonts/calibri.ttf",
        "C:/Windows/Fonts/arialbd.ttf" if bold else "C:/Windows/Fonts/arial.ttf",
    ):
        try:
            return ImageFont.truetype(name, size)
        except OSError:
            continue
    return ImageFont.load_default()


F14, F16, F18, F20, F22, F26 = font(14), font(16), font(18), font(20), font(22), font(26)
FB16, FB18, FB20, FB24, FB28 = font(16, True), font(18, True), font(20, True), font(24, True), font(28, True)


def new_img():
    im = Image.new("RGB", (W, H), BG)
    return im, ImageDraw.Draw(im)


def box(d, xy, fill=BOX, width=2):
    d.rounded_rectangle(xy, radius=10, outline=INK, fill=fill, width=width)


def text(d, xy, s, f=F16, fill=INK):
    d.text(xy, s, font=f, fill=fill)


def center(d, x0, x1, y, s, f=FB18):
    tw = d.textlength(s, font=f)
    d.text(((x0 + x1 - tw) / 2, y), s, font=f, fill=INK)


def arrow(d, a, b):
    d.line([a, b], fill=INK, width=2)
    x, y = b
    d.polygon([(x, y), (x - 10, y - 6), (x - 10, y + 6)], fill=INK)


def save(im, name):
    path = OUT / name
    im.save(path, "PNG")
    print("wrote", path, path.stat().st_size)


def architecture():
    im, d = new_img()
    text(d, (40, 24), "OraEarn — system architecture (team workshop, week 5–6)", FB28)
    text(d, (40, 64), "Drawn after the group talked through the whole product. Three local services + Postgres. No Docker.", F16, MUTED)

    actors = [(80, 140, 300, 280, "International student", "phone / laptop browser"), (80, 320, 300, 460, "Admin (university staff)", "approve ID / visa")]
    for x0, y0, x1, y1, t, sub in actors:
        box(d, (x0, y0, x1, y1))
        center(d, x0, x1, y0 + 36, t, FB20)
        center(d, x0, x1, y0 + 78, sub, F16)

    layers = [
        (420, 140, 820, 300, "Frontend  :5173", "Vite + React (folder frontend)\nLogin, jobs, shifts, expenses,\nsavings, dashboard, Aura widget"),
        (420, 340, 820, 520, "OraEarn.Api  :5203", "ASP.NET + JWT + SignalR\nJobs / Shifts / Expenses /\nSavings / Analytics / Auth"),
        (420, 560, 820, 760, "Aura advisor  :8000", "FastAPI chat helper\n(not the whole product)"),
    ]
    for x0, y0, x1, y1, t, sub in layers:
        box(d, (x0, y0, x1, y1), fill=(236, 236, 236))
        center(d, x0, x1, y0 + 16, t, FB20)
        for i, line in enumerate(sub.split("\n")):
            center(d, x0, x1, y0 + 56 + i * 26, line, F16)

    box(d, (940, 280, 1320, 520), fill=(228, 228, 228))
    center(d, 940, 1320, 320, "PostgreSQL (local)", FB20)
    center(d, 940, 1320, 370, "Windows service, not Docker", F16)
    center(d, 940, 1320, 404, "Users, Jobs, Shifts, Breaks,", F16)
    center(d, 940, 1320, 432, "Expenses, SavingsGoals, Visa", F16)

    arrow(d, (300, 210), (420, 210))
    arrow(d, (300, 390), (420, 390))
    arrow(d, (820, 220), (940, 360))
    arrow(d, (820, 430), (940, 400))
    arrow(d, (820, 640), (940, 480))
    text(d, (40, 820), "Scrum Master note (Dharma): keep the three ports and the working branch written on the board so nobody clones main and thinks the app is missing.", F16, MUTED)
    save(im, "figure-diagram-architecture.png")


def usecase():
    im, d = new_img()
    text(d, (40, 24), "OraEarn — use cases (student + admin)", FB28)
    text(d, (40, 64), "From the week-5 BA workshop. Boxes are the stories we actually built or still own on Jira.", F16, MUTED)
    box(d, (320, 110, 1080, 820), fill=BG, width=3)
    center(d, 320, 1080, 124, "OraEarn", FB24)

    student = [(80, 200, 260, 340, "Student"), (80, 560, 260, 700, "Admin")]
    for x0, y0, x1, y1, t in student:
        d.ellipse((x0 + 40, y0, x1 - 40, y0 + 70), outline=INK, width=2)
        d.line((x0 + 90, y0 + 70, x0 + 90, y1 - 40), fill=INK, width=2)
        d.line((x0 + 40, y0 + 110, x1 - 40, y0 + 110), fill=INK, width=2)
        d.line((x0 + 90, y1 - 40, x0 + 40, y1), fill=INK, width=2)
        d.line((x0 + 90, y1 - 40, x1 - 40, y1), fill=INK, width=2)
        center(d, x0, x1, y1 + 8, t, FB18)

    cases = [
        (400, 180, "Register + upload ID / visa"),
        (400, 250, "Sign in (JWT)"),
        (400, 320, "Add / edit job"),
        (400, 390, "Log shift / clock-in (main)"),
        (400, 460, "See 48-hour visa bar"),
        (400, 530, "Log expense / savings goal"),
        (400, 600, "Dashboard + Excel / PDF"),
        (400, 670, "Ask Aura a money question"),
        (760, 250, "Approve pending users"),
        (760, 320, "Review ID / visa file"),
    ]
    for x, y, label in cases:
        box(d, (x, y, x + 280, y + 48))
        center(d, x, x + 280, y + 12, label, F16)

    for y in (224, 294, 364, 434, 504, 574, 644, 714):
        d.line((260, 270, 400, y), fill=INK, width=1)
    d.line((260, 630, 760, 274), fill=INK, width=1)
    d.line((260, 630, 760, 344), fill=INK, width=1)
    save(im, "figure-diagram-usecase.png")


def sequence():
    im, d = new_img()
    text(d, (40, 24), "Sequence — add a shift (the main feature)", FB28)
    text(d, (40, 64), "What happens when Alice saves a shift. Visa check is the step that can reject the save.", F16, MUTED)
    cols = [("Student UI", 160), ("Auth / JWT", 430), ("ShiftService", 720), ("Postgres", 1030), ("Visa rule", 1280)]
    for name, x in cols:
        center(d, x - 90, x + 90, 120, name, FB18)
        d.line((x, 160, x, 820), fill=MUTED, width=2)

    steps = [
        (200, 160, 430, "1. POST /api/shifts + token"),
        (250, 430, 720, "2. load job rate, build shift"),
        (310, 720, 1030, "3. sum hours this fortnight"),
        (370, 1030, 1280, "4. hours + new shift <= 48?"),
        (430, 1280, 720, "5. ok / reject over-limit"),
        (500, 720, 1030, "6. insert shift + pay"),
        (570, 1030, 160, "7. 201 + calendar refresh"),
    ]
    y = 200
    for label, xa, xb in [(s[0], s[1], s[2]) for s in [
        ("1. POST /api/shifts + token", 160, 430),
        ("2. load job, compute pay", 430, 720),
        ("3. read fortnight hours", 720, 1030),
        ("4. apply 48-hour cap", 1030, 1280),
        ("5. pass or reject", 1280, 720),
        ("6. save shift row", 720, 1030),
        ("7. 201 back to calendar", 1030, 160),
    ]]:
        d.line((xa, y, xb, y), fill=INK, width=2)
        if xb > xa:
            d.polygon([(xb, y), (xb - 8, y - 5), (xb - 8, y + 5)], fill=INK)
        else:
            d.polygon([(xb, y), (xb + 8, y - 5), (xb + 8, y + 5)], fill=INK)
        mx = (xa + xb) / 2
        tw = d.textlength(label, font=F16)
        text(d, (mx - tw / 2, y - 28), label, F16)
        y += 90
    text(d, (40, 840), "Owned on Jira as EARN-70 (shift CRUD, Done). Visa rule is the same check shown in the backend glimpse figures.", F16, MUTED)
    save(im, "figure-diagram-sequence-shift.png")


def scrum():
    im, d = new_img()
    text(d, (40, 24), "Scrum on this team — Dharma as Scrum Master", FB28)
    text(d, (40, 64), "Not a full-time SM role. I still code. The SM work is keeping the board, the branch, and the week labels honest.", F16, MUTED)

    boxes = [
        (50, 130, 340, 340, "Working agreement", "• Clone app-s395172, not main\n• Stories only (CSV killed Epics)\n• W7–W10 = semester weeks\n• Done needs a screenshot or note\n• No demo passwords in uploads"),
        (370, 130, 680, 340, "What I actually do", "• Import / tidy EARN stories\n• Move tickets when a screen ships\n• Keep the three ports written down\n• Ask for AC before a new page\n• Pack timesheets for the report"),
        (710, 130, 1040, 340, "Cadence we agreed", "• Short standup: yday / today / stuck\n• Move Jira in the same call\n• Krishna tests data, not just UI\n• Jenish freezes AC same day\n• Suraj signs export columns"),
        (1070, 130, 1350, 340, "Evidence", "• GitHub 50 commits on branch\n• Latest: 3603d85 week 8 sheet\n• EARN-105/106/107/70 Done\n• Week 5 Git / timesheet fix\n• This progress report pack"),
    ]
    for x0, y0, x1, y1, title, body in boxes:
        box(d, (x0, y0, x1, y1))
        center(d, x0, x1, y0 + 14, title, FB20)
        text(d, (x0 + 16, y0 + 56), body, F16)

    text(d, (50, 370), "Sprint weeks on the board (semester labels, not extra features)", FB20)
    weeks = [
        ("W4", "ERD + BRD start"),
        ("W5", "UML + first API"),
        ("W6", "SQL tests + UI"),
        ("W7", "Rename OraEarn"),
        ("W8", "Traceability + report"),
        ("W9", "Merge / UAT"),
        ("W10", "Demo polish"),
    ]
    for i, (w, note) in enumerate(weeks):
        x0 = 50 + i * 190
        box(d, (x0, 420, x0 + 175, 560))
        center(d, x0, x0 + 175, 440, w, FB24)
        center(d, x0, x0 + 175, 490, note, F16)
        if i < 6:
            d.line((x0 + 175, 490, x0 + 190, 490), fill=INK, width=2)

    box(d, (50, 600, 1350, 840))
    text(d, (70, 620), "Honest SM status (19 Sep 2026)", FB20)
    text(
        d,
        (70, 670),
        "app-s395172 is the running app (50 commits, Dharma001 latest = week 8 timesheet). GitHub currently shows that branch\n"
        "2 commits behind main — those extra main commits are teammate folder uploads, not a second copy of the app.\n"
        "Most tickets still lack a written UAT comment. I will not mark Done without a screenshot. Until we merge, everyone\n"
        "commits on app-s395172. Invite the lecturer as a GitHub collaborator or the private repo looks empty.",
        F18,
    )
    save(im, "figure-diagram-scrum-dharma.png")


def team_erd():
    im, d = new_img()
    text(d, (40, 24), "Team workshop — we talked through the whole app, then drew the ERD", FB28)
    text(d, (40, 64), "Week 4–5. Krishna drew the PNG. The rest of us argued the tables. This is the discussion, not a second ERD.", F16, MUTED)

    talk = [
        (50, 130, 340, 310, "Jenish (BA)", "Which screens need a table?\nRegister, job, shift, expense,\nsavings, visa window, export."),
        (370, 130, 680, 310, "Dharma (dev / SM)", "What the API already stores:\nUser, Job, Shift, Expense,\nSavingsGoal, notification."),
        (710, 130, 1020, 310, "Krishna (DBA)", "Normalise it. Unique email.\nShift belongs to Job.\nFortnight hours is a query."),
        (1050, 130, 1350, 310, "Suraj (data)", "Which columns become a\ndashboard number? Do not\ninvent extra export fields."),
    ]
    for x0, y0, x1, y1, t, b in talk:
        box(d, (x0, y0, x1, y1))
        center(d, x0, x1, y0 + 16, t, FB18)
        text(d, (x0 + 16, y0 + 64), b, F16)

    d.line((695, 310, 695, 360), fill=INK, width=2)
    d.polygon([(695, 360), (689, 348), (701, 348)], fill=INK)

    ents = [
        (80, 400, "User"),
        (300, 400, "Job"),
        (520, 400, "Shift"),
        (740, 400, "Expense"),
        (960, 400, "SavingsGoal"),
        (1180, 400, "Visa window"),
    ]
    for x, y, name in ents:
        box(d, (x, y, x + 180, y + 80), fill=(236, 236, 236))
        center(d, x, x + 180, y + 24, name, FB18)
    for x in (260, 480, 700, 920, 1140):
        d.line((x, 440, x + 40, 440), fill=INK, width=2)

    box(d, (80, 540, 1320, 820))
    text(d, (100, 560), "What we agreed in the room", FB20)
    text(
        d,
        (100, 610),
        "1. One user can have many jobs; a shift always belongs to one job (rate comes from the job).\n"
        "2. Visa 48 hours is not a stored “cap table” — it is a fortnight query over Shift hours.\n"
        "3. Expenses and savings are separate from pay so the dashboard can show net balance without double-counting.\n"
        "4. Krishna keeps the ERD PNG; Dharma’s EF model has to stay in the same language (Job not Employer).\n"
        "5. Suraj’s export columns must match these tables. If a column is not here, it does not go in the Excel.\n"
        "The actual ERD drawing is Krishna’s Week-4 file (Figure K1 / team ERD figure). This page is the workshop around it.",
        F18,
    )
    save(im, "figure-diagram-team-erd.png")


def uml_classes():
    im, d = new_img()
    text(d, (40, 24), "UML class sketch — core OraEarn entities (team + Dharma)", FB28)
    text(d, (40, 64), "Matches Krishna’s ERD names and the EF model on app-s395172. Visa cap is a rule, not a table.", F16, MUTED)

    def klass(x, y, w, h, title, lines):
        box(d, (x, y, x + w, y + h), fill=(248, 248, 248))
        d.rectangle((x, y, x + w, y + 36), outline=INK, fill=(230, 230, 230), width=2)
        center(d, x, x + w, y + 8, title, FB18)
        text(d, (x + 12, y + 48), lines, F16)

    klass(40, 120, 260, 220, "User", "+ email (unique)\n+ first / last name\n+ visa type / expiry\n+ approval status\n+ roles")
    klass(360, 120, 260, 220, "Job", "+ title / employer\n+ hourly rate\n+ userId\n1 user → many jobs")
    klass(680, 120, 300, 220, "Shift", "+ start / end\n+ break minutes\n+ pay (from job rate)\n+ jobId\n1 job → many shifts")
    klass(1040, 120, 300, 220, "Expense", "+ amount / category\n+ spentAt\n+ userId")
    klass(360, 400, 260, 200, "SavingsGoal", "+ name / target\n+ saved so far\n+ userId")
    klass(680, 400, 300, 200, "VisaRule", "query fortnight hours\nfrom Shift\nreject save if > 48")
    klass(1040, 400, 300, 200, "Notification", "+ message\n+ SignalR push\n+ userId")

    d.line((300, 230, 360, 230), fill=INK, width=2)
    d.line((620, 230, 680, 230), fill=INK, width=2)
    d.line((170, 340, 170, 500), fill=INK, width=2)
    d.line((170, 500, 360, 500), fill=INK, width=2)
    d.line((170, 340, 170, 180), fill=INK, width=1)
    d.line((170, 180, 1040, 180), fill=INK, width=1)
    text(d, (40, 780), "Scrum Master (Dharma): this sketch is what we pinned next to Jira so Job ≠ Employer and fortnight hours stay a query. Krishna’s PNG is the full ERD.", F16, MUTED)
    save(im, "figure-diagram-uml-classes.png")


def swagger_tests():
    im, d = new_img()
    text(d, (40, 24), "API testing sheet — Dharma (Swagger + live calls)", FB28)
    text(d, (40, 64), "19 Sep 2026. swagger.json was 500 (Register [FromForm] + IFormFile). Dharma fixed the form DTO; Swagger UI now loads. Token hidden.", F16, MUTED)

    headers = ["#", "Method", "Path", "What it proves", "Result"]
    rows = [
        ["1", "GET", "/health", "API process is up", "200"],
        ["2", "GET", "/swagger/v1/swagger.json", "Official Swagger file", "200"],
        ["3", "POST", "/api/auth/login", "JWT for seeded Alice", "200"],
        ["4", "GET", "/api/jobs", "Cafe / retail jobs", "200"],
        ["5", "GET", "/api/shifts", "Calendar / fortnight hours", "200"],
        ["6", "GET", "/api/expenses", "Seeded spend rows", "200"],
        ["7", "GET", "/api/SavingsGoals", "Savings list", "200"],
        ["8", "GET", "/api/analytics/me", "Dashboard cards + export", "200"],
        ["9", "GET", "/api/notifications", "SignalR inbox", "200"],
        ["10", "GET", "Aura :8000/docs", "Chat helper OpenAPI", "200"],
    ]
    xcols = [50, 110, 250, 620, 1180, 1350]
    y = 120
    box(d, (50, y, 1350, y + 46), fill=(230, 230, 230))
    for i, h in enumerate(headers):
        text(d, (xcols[i] + 8, y + 12), h, FB16)
    y = 166
    for i, row in enumerate(rows):
        fill = (250, 250, 250) if i % 2 == 0 else (240, 240, 240)
        box(d, (50, y, 1350, y + 58), fill=fill)
        for j, cell in enumerate(row):
            text(d, (xcols[j] + 8, y + 16), cell, FB16 if j == 4 else F16)
        y += 58
    text(d, (50, 820), "Jira: EARN-105 login, EARN-107 jobs, EARN-70 shifts, EARN-106 dashboard. Written UAT comments still thin (EARN-148 / EARN-154).", F16, MUTED)
    save(im, "figure-diagram-swagger-tests.png")


if __name__ == "__main__":
    architecture()
    usecase()
    sequence()
    scrum()
    team_erd()
    uml_classes()
    swagger_tests()
