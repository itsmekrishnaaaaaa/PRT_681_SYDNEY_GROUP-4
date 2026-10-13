"""Ten-slide Assessment 3 deck.

Follows OraEarn_Project_Presentation.pptx.
White page, black text. Facts match this codebase.
"""

from __future__ import annotations

from pathlib import Path

from PIL import Image as PilImage
from pptx import Presentation
from pptx.dml.color import RGBColor
from pptx.enum.shapes import MSO_SHAPE
from pptx.enum.text import MSO_ANCHOR, PP_ALIGN
from pptx.util import Emu, Inches, Pt

ROOT = Path(__file__).resolve().parent
SHOT = ROOT / "shots"
OUTS = [
    ROOT / "OraEarn-Assessment3-Presentation.pptx",
    Path(r"C:\Users\budha\Downloads\OraEarn_Project_Presentation.pptx"),
]

INK = RGBColor(0x11, 0x11, 0x11)
MUTED = RGBColor(0x44, 0x44, 0x44)
LINE = RGBColor(0x11, 0x11, 0x11)
HAIR = RGBColor(0xC5, 0xD5, 0xD4)
SOFT = RGBColor(0xE7, 0xF4, 0xF3)
WHITE = RGBColor(0xFF, 0xFF, 0xFF)
TEAL = RGBColor(0x0E, 0x7C, 0x7B)

W = Inches(13.333)
H = Inches(7.5)


def _set_run(run, size, bold, color, font="Calibri"):
    run.font.size = Pt(size)
    run.font.bold = bold
    run.font.color.rgb = color
    run.font.name = font


def text(slide, l, t, w, h, body, size=18, bold=False, color=INK, align=PP_ALIGN.LEFT, anchor=MSO_ANCHOR.TOP):
    box = slide.shapes.add_textbox(Inches(l), Inches(t), Inches(w), Inches(h))
    tf = box.text_frame
    tf.word_wrap = True
    tf.auto_size = None
    tf.margin_left = Emu(0)
    tf.margin_right = Emu(0)
    tf.margin_top = Emu(0)
    tf.margin_bottom = Emu(0)
    try:
        tf._txBody.bodyPr.set("anchor", {MSO_ANCHOR.TOP: "t", MSO_ANCHOR.MIDDLE: "ctr", MSO_ANCHOR.BOTTOM: "b"}[anchor])
    except Exception:
        pass
    lines = body.split("\n")
    for i, line in enumerate(lines):
        p = tf.paragraphs[0] if i == 0 else tf.add_paragraph()
        p.alignment = align
        p.space_before = Pt(0)
        p.space_after = Pt(0)
        run = p.add_run()
        run.text = line
        _set_run(run, size, bold, color)
    return box


def rect(slide, l, t, w, h, fill, line=None):
    sh = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, Inches(l), Inches(t), Inches(w), Inches(h))
    sh.fill.solid()
    sh.fill.fore_color.rgb = fill
    if line is None:
        sh.line.fill.background()
    else:
        sh.line.color.rgb = line
        sh.line.width = Pt(1)
    sh.shadow.inherit = False
    return sh


def notes(slide, body):
    slide.notes_slide.notes_text_frame.text = body.strip()


def place_picture(slide, filename, l, t, w, h):
    path = SHOT / filename
    with PilImage.open(path) as im:
        iw, ih = im.size
    aspect = iw / ih
    if aspect > w / h:
        dw, dh = w, w / aspect
    else:
        dh, dw = h, h * aspect
    slide.shapes.add_picture(str(path), Inches(l + (w - dw) / 2), Inches(t + (h - dh) / 2), Inches(dw), Inches(dh))


def new(prs):
    slide = prs.slides.add_slide(prs.slide_layouts[6])
    bg = slide.shapes.add_shape(MSO_SHAPE.RECTANGLE, 0, 0, W, H)
    bg.fill.solid()
    bg.fill.fore_color.rgb = WHITE
    bg.line.fill.background()
    sp_tree = slide.shapes._spTree
    sp = bg._element
    sp_tree.remove(sp)
    sp_tree.insert(2, sp)
    return slide


def header(slide, title, page):
    rect(slide, 0, 0, 0.12, 7.5, TEAL)
    text(slide, 0.55, 0.32, 11.2, 0.55, title, size=32, bold=True, color=INK)
    rect(slide, 0.55, 0.98, 12.23, 0.035, TEAL)
    text(slide, 11.4, 7.05, 1.4, 0.28, str(page), size=14, bold=True, color=TEAL, align=PP_ALIGN.RIGHT)
    text(slide, 0.55, 7.05, 8, 0.28, "OraEarn   ·   PRT681   ·   Group 4", size=12, color=MUTED)


def card(slide, l, t, w, h):
    rect(slide, l, t, w, h, SOFT, HAIR)
    rect(slide, l, t, w, 0.07, TEAL)


def slide_title(prs):
    s = new(prs)
    rect(s, 0, 0, 13.333, 0.1, TEAL)
    rect(s, 0, 0, 0.12, 7.5, TEAL)
    text(s, 0.6, 0.35, 12, 0.9, "OraEarn", size=54, bold=True)
    text(s, 0.62, 1.3, 12, 0.4, "Earn smart. Stay compliant. Save with purpose.", size=20, color=MUTED)
    text(s, 0.62, 1.75, 12, 0.35, "Assessment 3   ·   Project presentation   ·   PRT681   ·   Group 4", size=16, color=INK)
    people = [
        ("Krishna Sapkota", "S395795", "Database administrator\nTester"),
        ("Dharma Bahadur Budhathoki", "S395172", "Developer\nBusiness analyst"),
        ("Jenish Gurung", "S395321", "Business analyst\nReport writer"),
        ("Suraj Khadka", "S394949", "Data analyst\nDatabase administrator"),
    ]
    x = 0.55
    for name, sid, role in people:
        card(s, x, 2.45, 2.95, 3.15)
        text(s, x + 0.18, 2.7, 2.6, 0.9, name, size=16, bold=True)
        text(s, x + 0.18, 3.65, 2.6, 0.35, sid, size=14, color=MUTED)
        text(s, x + 0.18, 4.15, 2.6, 0.9, role, size=15)
        x += 3.15
    text(
        s,
        0.6,
        5.9,
        12.1,
        0.7,
        "Acknowledgements. Our tutors and lecturers, for guidance and feedback,\nand every teammate whose work built OraEarn.",
        size=15,
        color=MUTED,
    )
    text(s, 0.55, 7.05, 8, 0.28, "OraEarn   ·   PRT681   ·   Group 4", size=12, color=MUTED)
    text(s, 11.4, 7.05, 1.4, 0.28, "1", size=14, bold=True, color=TEAL, align=PP_ALIGN.RIGHT)
    notes(
        s,
        """
About 30 seconds. Read the line under the name, then the four people. Do not read the acknowledgements unless you are thanked.

Krishna: database and tests. Dharma: the build and the business view. Jenish: the rules and the written report. Suraj: the numbers and the tables.

Then: where the product came from.
""",
    )


def slide_origin(prs):
    s = new(prs)
    header(s, "Where we came from", 2)
    cols = [
        ("The challenge", "A student visa allows 48 hours in 14 days.\nShifts, expenses, and savings were tracked in separate places."),
        ("Our constraints", "First time on this stack.\nFive weekly sprints.\nFour people. We did not add a fifth late."),
        ("Our solution", "OraEarn. One account for jobs, shifts, the visa check, expenses, savings, and Aura."),
    ]
    x = 0.55
    for title, body in cols:
        card(s, x, 1.45, 3.9, 4.9)
        text(s, x + 0.28, 1.75, 3.35, 0.7, title, size=22, bold=True)
        text(s, x + 0.28, 2.7, 3.35, 3.1, body, size=18)
        x += 4.1
    notes(
        s,
        """
About 45 seconds. One sentence per column.

Challenge: the 48-hour fortnight, and money split across apps and spreadsheets.
Constraints: new technologies, five sprints from week 5 to week 9, four people. Brooks: adding a person late costs teaching time.
Solution: one login. The visa rule can see the shifts, because the shifts are in the same account.
""",
    )


def slide_product(prs):
    s = new(prs)
    header(s, "Product overview", 3)
    text(s, 0.55, 1.2, 12, 0.4, "Why OraEarn. One login covers the work, the limit, and the money.", size=18, color=MUTED)
    items = [
        ("Live shifts", "Clock in and out.\nPay comes from the job rate.\nThe calendar updates live."),
        ("Visa guard", "Rolling 14 days.\n48 hours for a student.\nOver the limit, the shift is not saved."),
        ("Money tools", "Expenses by category.\nSavings goals from leftover pay."),
        ("Aura and exports", "A small advisor beside the product.\nExcel and PDF from the same figures."),
    ]
    x = 0.55
    for title, body in items:
        card(s, x, 1.9, 2.95, 4.4)
        text(s, x + 0.22, 2.2, 2.5, 0.9, title, size=20, bold=True)
        text(s, x + 0.22, 3.3, 2.5, 2.5, body, size=16)
        x += 3.15
    notes(
        s,
        """
About 50 seconds. Point across the four cards.

Live shifts: SignalR hub /hubs/shifts. The hourly rate is on the job, not copied onto the shift.
Visa guard: VisaComplianceService. Student work rights only. Window is today minus 13 days through today. Limit is 48. The check runs before the insert.
Money: leftover pay is earnings minus expenses, never below zero.
Aura: Python on port 8000. If it is down, the rest of the app still runs. Exports are Excel and PDF of the same dashboard numbers.
""",
    )


def logo_tile(slide, l, t, w, h, filename, title, caption):
    card(slide, l, t, w, h)
    place_picture(slide, f"logos/{filename}", l + 0.2, t + 0.22, w - 0.4, 1.15)
    text(slide, l + 0.08, t + 1.45, w - 0.16, 0.32, title, size=14, bold=True, align=PP_ALIGN.CENTER)
    text(slide, l + 0.08, t + 1.78, w - 0.16, 0.45, caption, size=12, color=MUTED, align=PP_ALIGN.CENTER)


def slide_tech(prs):
    s = new(prs)
    header(s, "Technologies and tools", 4)
    logos = [
        ("react.png", "React 19", "The view. Port 5173."),
        ("typescript.png", "TypeScript", "The page code."),
        ("vite.png", "Vite", "The dev server."),
        ("dotnet.png", ".NET 10", "The API. Port 5203."),
        ("postgres.png", "PostgreSQL", "The database."),
        ("python.png", "Python", "Aura. Port 8000."),
    ]
    x = 0.4
    for filename, title, caption in logos:
        logo_tile(s, x, 1.22, 2.05, 2.4, filename, title, caption)
        x += 2.14
    text(
        s,
        0.55,
        3.75,
        12.2,
        0.35,
        "The page calls the API. The API calls PostgreSQL and, when needed, Aura.",
        size=15,
        color=MUTED,
    )
    card(s, 0.55, 4.25, 6.05, 2.5)
    text(s, 0.78, 4.45, 5.6, 0.35, "JWT", size=18, bold=True)
    text(
        s,
        0.78,
        4.9,
        5.6,
        1.6,
        "JwtBearer checks issuer, audience, lifetime, and the signing key.\n"
        "A good sign-in returns an access token and a refresh token.\n"
        "No token on a protected route returns 401.",
        size=14,
    )
    card(s, 6.8, 4.25, 5.95, 2.5)
    text(s, 7.05, 4.45, 5.5, 0.35, "Password is hashed", size=18, bold=True)
    text(
        s,
        7.05,
        4.9,
        5.5,
        1.6,
        "Identity writes PasswordHash. Sign-in compares the hash.\n"
        "At least 8 characters, with upper, lower, and a digit.\n"
        "Five failures lock the account for 15 minutes.",
        size=14,
    )
    notes(
        s,
        """
About 50 seconds. Walk the figure left to right, then the two cards.

React on 5173 is the view. It sends HTTP and the JWT to ASP.NET Core on 5203. Controllers call services. Services use EF Core. EF Core talks to PostgreSQL. The browser never opens the database.

SignalR hub is /hubs/shifts and uses the same token. Aura is Python on port 8000. The API calls it. The product still runs if Aura is off.

JWT: AddJwtBearer. Checks issuer, audience, lifetime, and the signing key. Access token plus refresh token. Logout revokes the refresh token.

Password: UserManager.CreateAsync stores PasswordHash. CheckPasswordSignInAsync compares the hash. Length 8, digit, lower, upper. Lockout is 5 failures, then 15 minutes.
""",
    )


def slide_architecture(prs):
    s = new(prs)
    header(s, "Architecture", 5)
    place_picture(s, "diagram-architecture.png", 0.35, 1.12, 12.6, 5.75)
    notes(
        s,
        """
About 60 seconds. This is service-based MVC. Point left to right, then the return arrow, then Aura.

View: the React pages. There is no Razor page. The browser is the view.
Controller: AuthController, JobsController, ShiftsController, ExpensesController, SavingsGoalsController, AnalyticsController, NotificationsController, AdminController. A protected action has Authorize. The JWT is checked before the method runs.
Service: the controller calls a service. ShiftService, JobService, AuthService, ExpenseService, VisaComplianceService. The 48-hour check is in the service, before any insert.
Model: the entity classes and ApplicationDbContext. EF Core is the data access.
Database: PostgreSQL.

The return arrow is the JSON response, back to the same page.

Aura hangs off the service layer. PythonAdvisorClient calls port 8000. The page does not call Python itself.

SignalR is on the same API. The hub is authorized with the same JWT.
""",
    )


def figure_slide(prs, title, page, filename, spoken):
    s = new(prs)
    header(s, title, page)
    place_picture(s, filename, 0.35, 1.12, 12.6, 5.75)
    notes(s, spoken)
    return s


def slide_data(prs):
    s = new(prs)
    header(s, "Database design and integrity", 6)
    text(s, 0.55, 1.18, 12.2, 0.35, "Third normal form. Each fact is stored once.", size=16, color=MUTED)

    def box(l, t, w, label):
        rect(s, l, t, w, 0.7, WHITE, INK)
        text(s, l, t + 0.16, w, 0.4, label, size=16, bold=True, align=PP_ALIGN.CENTER)

    box(0.55, 1.75, 2.3, "Users")
    text(s, 2.9, 1.9, 0.7, 0.4, "1 → *", size=14, color=MUTED, align=PP_ALIGN.CENTER)
    box(3.6, 1.75, 2.5, "Jobs")
    text(s, 6.15, 1.9, 0.7, 0.4, "1 → *", size=14, color=MUTED, align=PP_ALIGN.CENTER)
    box(6.9, 1.75, 2.4, "Shifts")
    text(s, 9.5, 1.85, 3.3, 0.55, "Rate stays on the job.", size=14, color=MUTED)

    box(0.55, 2.85, 2.3, "Users")
    text(s, 2.85, 3.0, 0.75, 0.4, "1 → *", size=14, color=MUTED, align=PP_ALIGN.CENTER)
    box(3.6, 2.85, 2.7, "Categories")
    text(s, 6.35, 3.0, 0.7, 0.4, "1 → *", size=14, color=MUTED, align=PP_ALIGN.CENTER)
    box(7.05, 2.85, 2.3, "Expenses")
    text(s, 9.4, 3.0, 0.85, 0.4, "* ← 1", size=14, color=MUTED, align=PP_ALIGN.CENTER)
    box(10.25, 2.85, 2.5, "Pay methods")

    box(0.55, 3.95, 1.9, "Users")
    text(s, 2.45, 4.1, 0.7, 0.4, "1 → *", size=14, color=MUTED, align=PP_ALIGN.CENTER)
    box(3.15, 3.95, 2.55, "Savings goals")
    box(6.15, 3.95, 1.9, "Users")
    text(s, 8.05, 4.1, 0.9, 0.4, "1 → 0..1", size=14, color=MUTED, align=PP_ALIGN.CENTER)
    box(8.95, 3.95, 2.5, "Live shift")

    card(s, 0.55, 5.05, 12.2, 1.7)
    text(
        s,
        0.8,
        5.28,
        11.7,
        1.3,
        "An expense stores a category and a payment method. The category belongs to the user. A shift stores the job.\n"
        "Foreign keys and delete rules are in ApplicationDbContext. The hourly rate stays on the job.\n"
        "48 hours is a sum of shift hours over the last 14 days. It is not a column.",
        size=16,
    )
    notes(
        s,
        """
About 60 seconds. Walk the three lines, then the sentence at the bottom.

Line 1: a user has jobs. A job has shifts. The shift stores JobId, not a copied hourly rate, and not a UserId. Pay is calculated from the job's rate at save time. Editing the job rate later does not rewrite old shifts.

Line 2: this is the correction if someone remembers an older diagram. An expense does not link to a job. It links to ExpenseCategory and PaymentMethod. The category belongs to the user, so the expense is still that user's money. Payment methods are Card, Cash, Bank Transfer, and Other. Delete is Restrict while expenses exist.

Line 3: savings goals belong to the user. Allocation is a percent of leftover pay. The live shift is ActiveShiftSession. Its UserId is unique, so only one clock-in is open.

The migration is named NormalizeTo3NF. The 48-hour cap is VisaComplianceService, a query, not a column.
""",
    )


def slide_security(prs):
    s = new(prs)
    header(s, "Security and quality assurance", 11)
    steps = [
        ("1  Sign in", "Identity stores PasswordHash.\nThe typed password is not saved.\nA good sign-in returns a JWT."),
        ("2  Approval", "New accounts are Pending.\nPending and Rejected get no token.\nAn admin approves first."),
        ("3  Role", "Most routes need a token.\nAdmin routes also need\nthe Admin role."),
        ("4  Own data", "A query is limited to that user.\nOne student cannot read\nanother student's pay."),
    ]
    x = 0.55
    for title, body in steps:
        card(s, x, 1.25, 2.95, 2.55)
        text(s, x + 0.16, 1.42, 2.65, 0.4, title, size=16, bold=True)
        text(s, x + 0.16, 1.9, 2.65, 1.7, body, size=13)
        x += 3.15
    card(s, 0.55, 4.05, 6.0, 2.7)
    text(s, 0.8, 4.25, 5.5, 0.4, "Testing", size=18, bold=True)
    text(
        s,
        0.8,
        4.75,
        5.5,
        1.8,
        "29 tests passed. None failed. None skipped.\n"
        "Bad login returns 401.\n"
        "No token on /me, or on jobs, returns 401.\n"
        "A valid token returns the profile.",
        size=15,
    )
    card(s, 6.75, 4.05, 6.0, 2.7)
    text(s, 7.0, 4.25, 5.5, 0.4, "What we still owe", size=18, bold=True)
    text(
        s,
        7.0,
        4.75,
        5.5,
        1.8,
        "Repeated bad passwords lock the account.\n"
        "Logout revokes the refresh token.\n"
        "Gap: no test yet posts a shift that\nwould cross 48 hours.",
        size=15,
    )
    notes(
        s,
        """
About 70 seconds. This is the security story. Do not claim a hash algorithm by name. Say ASP.NET Identity writes PasswordHash when the user is created. CheckPasswordSignInAsync compares that hash. Lockout is on.

Approval: Register sets ApprovalStatus to Pending. A non-admin who is Pending or Rejected never reaches a token. Admin can sign in to do the approval.

Unauthorised tests, black box: Login_WithInvalidCredentials returns 401. Me without a token returns 401. GetJobs without a token returns 401. A good login returns an access token. Me with that token returns the profile.

Be straight about the gap. The 48-hour refusal is in the service. The test project does not yet post a shift that crosses 48 and expect a rejection. That is the next test. Alice on the live screen is at 7.5 of 48, so do not say she is already over.
""",
    )


def slide_analytics(prs):
    s = new(prs)
    header(s, "Data, analytics, and Aura", 12)
    cols = [
        ("Shifts and expenses", "Hours, income, and spending.\nPay is hours times the job rate, after the break."),
        ("Analytics", "Same formula.\nWindows of 7 days, 30 days,\n1 month, and 12 months."),
        ("Goal tracking", "Leftover pay is earnings minus expenses, never below zero.\nProgress caps at 100 percent."),
        ("Aura", "Answers a savings question from that user's figures.\nIf Aura is off, the app still runs."),
    ]
    x = 0.55
    for title, body in cols:
        card(s, x, 1.4, 2.95, 5.15)
        text(s, x + 0.2, 1.7, 2.55, 1.1, title, size=20, bold=True)
        text(s, x + 0.2, 3.0, 2.55, 3.1, body, size=16)
        x += 3.15
    notes(
        s,
        """
About 50 seconds. Suraj owns these numbers if he is speaking. Use only the live screen.

Alice, student, last 7 days, 29 Sep 2026 to 5 Oct 2026: one shift, 7.5 hours, earnings $210, expenses $365, net −$155. 7.5 times $28 is $210, so that shift was priced from Cafe Barista. Visa bar: 7.5 of 48, 40.5 left, window 22 Sep 2026 to 5 Oct 2026. Savings on screen: targets about $9,722, 65 percent allocated, about $1,144 saved. Point at Holiday trip and Emergency fund. Ignore the short test names.

Two sums: the 7-day card is not the savings total. Goals use all earnings minus all expenses.

Hours still needed is remaining dollars divided by the hourly rate. If allocation percents pass 100, they are scaled back to 100.
""",
    )


def slide_process(prs):
    s = new(prs)
    header(s, "Development process", 13)
    weeks = [
        ("Week 5", "Foundation", "Account and database."),
        ("Week 6", "Shifts", "Jobs, clock-in, SignalR."),
        ("Week 7", "Money", "Expenses and goals."),
        ("Week 8", "Compliance", "48-hour rule, analytics, exports."),
        ("Week 9", "Aura", "Notifications and Aura."),
    ]
    x = 0.55
    for week, title, body in weeks:
        card(s, x, 1.25, 2.35, 2.7)
        text(s, x + 0.14, 1.42, 2.05, 0.3, week, size=13, color=MUTED)
        text(s, x + 0.14, 1.75, 2.05, 0.55, title, size=18, bold=True)
        text(s, x + 0.14, 2.4, 2.05, 1.3, body, size=14)
        x += 2.52
    bottom = [
        ("Agile / Scrum", "Weekly sprints.\nStories in Jira.\nA phase was done when you could use it."),
        ("Task allocation", "Split by the roles on the title slide.\nFour people. The same four through week 9."),
        ("Collaboration", "Git history tied to the work.\nWe planned the team early.\nThat is the mythical man-month."),
    ]
    x = 0.55
    for title, body in bottom:
        card(s, x, 4.15, 3.95, 2.55)
        text(s, x + 0.22, 4.35, 3.5, 0.4, title, size=18, bold=True)
        text(s, x + 0.22, 4.9, 3.5, 1.5, body, size=15)
        x += 4.15
    notes(
        s,
        """
About 45 seconds.

The order is deliberate. The visa check is a sum of shifts, so shifts existed before the rule. Aura has nothing to say until goals and hours exist.

We stayed four people. Adding someone in the last weeks would have been spent on teaching. A week moved a story. It did not grow the team.

Some older Git uploads only say "Add files via upload". From the working practice, messages are feat: or fix:. Do not invent hour totals from a timesheet on this slide.
""",
    )


def slide_future(prs):
    s = new(prs)
    header(s, "Strengths, improvements, and future", 14)
    cols = [
        ("Key strengths", "The visa rule blocks the save.\nThe database is in third normal form.\nA clock-out updates other open tabs.\nThe demo data can be seeded again.\n29 tests are green."),
        ("Improve next", "A test that posts a shift past 48 hours.\nMore tests around approval and lockout.\nA pass over slow dashboard queries."),
        ("Future features", "A phone app on the same API.\nOther visa types, not only the student 48.\nA bank feed, so expenses are not typed.\nA deeper Aura, same panel."),
    ]
    x = 0.55
    for title, body in cols:
        card(s, x, 1.4, 3.95, 5.2)
        text(s, x + 0.25, 1.7, 3.45, 0.7, title, size=22, bold=True)
        text(s, x + 0.25, 2.6, 3.45, 3.6, body, size=16)
        x += 4.15
    notes(
        s,
        """
About 40 seconds. Left is what works today. Middle is the honest gap. Right is later, not this assessment.

The missing test matters because the rule is real in VisaComplianceService and the screen shows Alice at 7.5 of 48. The suite proves hour maths, pay, savings, and 401s. It does not yet prove the refusal.

Future: the API is already there for a phone client. Aura stays a separate Python service.
""",
    )


def slide_demo(prs):
    s = new(prs)
    header(s, "Live demo", 15)
    text(s, 0.55, 1.18, 12, 0.35, "OraEarn in action. Alice is the student. Bob is not on the 48-hour rule.", size=16, color=MUTED)
    cols = [
        ("Shifts", "Dharma", "Sign in as Alice.\nAdd a shift.\nThe calendar updates on the open tab."),
        ("Money", "Dharma", "Add an expense.\nAdd a savings goal.\nLeftover pay moves the goal."),
        ("Visa guard", "Krishna", "A shift that would pass 48 hours is refused.\nBob, a citizen, is not on that rule."),
        ("Aura and export", "Krishna", "Open analytics.\nExport.\nAsk Aura.\nThen questions."),
    ]
    x = 0.55
    for title, who, body in cols:
        card(s, x, 1.75, 2.95, 4.85)
        text(s, x + 0.2, 2.0, 2.55, 0.7, title, size=20, bold=True)
        text(s, x + 0.2, 2.75, 2.55, 0.4, who, size=14, bold=True, color=MUTED)
        text(s, x + 0.2, 3.3, 2.55, 2.9, body, size=15)
        x += 3.15
    notes(
        s,
        """
Demo accounts. Do not read the passwords to the room unless the marker asks you to sign in.

Alice Nguyen, student, already approved: alice@oraearn.local / User123!
Bob, citizen, full work rights: bob@oraearn.local / User123!
Admin: admin@oraearn.local / Admin123!

Alice's visa bar is 7.5 of 48, window 22 Sep 2026 to 5 Oct 2026. She is not over the limit. To show the guard, enter a shift that would take the fortnight over 48. The API refuses and does not insert. Then sign in as Bob and show that the same rule does not apply.

Ports: frontend 5173, API 5203, Aura 8000. Swagger is /swagger if they ask for the lock on each route.

Jobs to point at: Cafe Barista $28, Retail Assistant $26. Ignore a test job named dfdf. Savings: Holiday trip and Emergency fund. Ignore a test goal named asd.

Close on questions. The next test we would write is the one that expects the 48-hour refusal.
""",
    )


def build():
    prs = Presentation()
    prs.slide_width = W
    prs.slide_height = H
    prs.core_properties.title = "OraEarn — Assessment 3"
    prs.core_properties.subject = "PRT681 Group 4"
    prs.core_properties.category = "Assessment 3"
    import draw_figures

    draw_figures.main()
    slide_title(prs)
    slide_origin(prs)
    slide_product(prs)
    slide_tech(prs)
    slide_architecture(prs)
    slide_data(prs)
    figure_slide(
        prs,
        "Entity relationship diagram",
        7,
        "diagram-erd.png",
        """
About 50 seconds. This is the database the code uses.

Users hold the email, PasswordHash, visa fields, and approval. A user has many jobs. A job has many shifts. The shift stores JobId. The hourly rate stays on the job.

Categories belong to the user. An expense stores CategoryId and PaymentMethodId. Payment methods are Card, Cash, Bank transfer, and Other.

Savings goals belong to the user. The live shift is one open clock-in, unique on UserId.

48 hours is a sum of shift hours over 14 days. It is not a table.
""",
    )
    figure_slide(
        prs,
        "Normalization",
        8,
        "diagram-normalization.png",
        """
About 40 seconds. Walk left to right.

UNF was one spreadsheet row: visa, jobs, shifts, and expenses packed together.
1NF made each fact a row, but the shift still copied the user, the job title, and the rate.
3NF is what is running. The rate lives on the job. The shift stores JobId. The expense stores two foreign keys.

The migration is named NormalizeTo3NF. A shift already saved keeps the pay it was given when Cafe's rate changes.
""",
    )
    figure_slide(
        prs,
        "Use case",
        9,
        "diagram-usecase.png",
        """
About 35 seconds. Two actors.

The student registers, signs in, works with jobs and shifts, watches the 48-hour bar, logs money, exports, and can ask Aura.
The admin approves or rejects, opens the ID file, sets the role, and can see platform analytics.

A student action and an admin action are separate. Admin routes need the Admin role.
""",
    )
    figure_slide(
        prs,
        "Sequence: saving a shift",
        10,
        "diagram-sequence.png",
        """
About 50 seconds. Follow the numbers.

1. React posts /api/Shifts with the JWT.
2. Shift.Calculate uses the job's HourlyRate. Hours are end minus start minus break.
3. VisaCompliance runs only for a student. It sums hours over the last 14 days.
4 and 5. That sum is a query on this user's shifts.
6. Over 48, the save stops. Nothing is inserted.
7. Under 48, the shift row is inserted.
8. The API returns 201 and the savings goals are recalculated.

The check is in ShiftService.CreateAsync, before SaveChanges.
""",
    )
    slide_security(prs)
    slide_analytics(prs)
    slide_process(prs)
    slide_future(prs)
    slide_demo(prs)
    for path in OUTS:
        path.parent.mkdir(parents=True, exist_ok=True)
        prs.save(path)
        print(f"Saved {path}")
    print(f"Slides: {len(prs.slides)}")


if __name__ == "__main__":
    build()
