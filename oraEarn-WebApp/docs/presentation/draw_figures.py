"""ERD, normalization, use case, and sequence diagrams from this codebase."""

from pathlib import Path

from PIL import Image, ImageDraw, ImageFont

OUT = Path(__file__).resolve().parent / "shots"
FONT = Path(r"C:\Windows\Fonts\calibri.ttf")
FONTB = Path(r"C:\Windows\Fonts\calibrib.ttf")

INK = (17, 17, 17)
MUTED = (80, 80, 80)
LINE = (180, 180, 180)
WHITE = (255, 255, 255)
SOFT = (231, 244, 243)
TEAL = (14, 124, 123)


def font(size, bold=False):
    return ImageFont.truetype(str(FONTB if bold else FONT), size)


def save(img, name):
    OUT.mkdir(parents=True, exist_ok=True)
    path = OUT / name
    img.save(path, "PNG")
    return path


def card(d, box, title, lines):
    x1, y1, x2, y2 = box
    d.rounded_rectangle(box, radius=12, fill=WHITE, outline=INK, width=2)
    d.rectangle((x1, y1, x2, y1 + 46), fill=TEAL)
    d.text((x1 + 16, y1 + 10), title, font=font(22, True), fill=WHITE)
    y = y1 + 60
    for line in lines:
        d.text((x1 + 16, y), line, font=font(18), fill=INK)
        y += 28


def arrow(d, x1, x2, y, label):
    if x2 >= x1:
        d.line((x1, y, x2 - 14, y), fill=INK, width=3)
        d.polygon([(x2 - 14, y - 7), (x2 - 14, y + 7), (x2, y)], fill=INK)
    else:
        d.line((x1, y, x2 + 14, y), fill=INK, width=3)
        d.polygon([(x2 + 14, y - 7), (x2 + 14, y + 7), (x2, y)], fill=INK)
    if label:
        f = font(16, True)
        tw = d.textlength(label, font=f)
        cx = (x1 + x2) / 2
        d.rectangle((cx - tw / 2 - 4, y - 26, cx + tw / 2 + 4, y - 4), fill=WHITE)
        d.text((cx - tw / 2, y - 26), label, font=f, fill=INK)


def erd():
    img = Image.new("RGB", (2400, 1100), WHITE)
    d = ImageDraw.Draw(img)
    card(d, (40, 30, 520, 340), "Users", [
        "Email, PasswordHash",
        "Visa type, work rights",
        "Approval status",
        "Refresh token",
    ])
    arrow(d, 520, 620, 160, "1 → *")
    card(d, (620, 30, 1100, 340), "Jobs", [
        "UserId",
        "Title",
        "HourlyRate",
    ])
    arrow(d, 1100, 1200, 160, "1 → *")
    card(d, (1200, 30, 1760, 340), "Shifts", [
        "JobId",
        "Start, end, break",
        "TotalHours",
        "TotalEarned",
    ])
    d.text((1800, 140), "The rate stays\non the job.", font=font(20), fill=MUTED)

    card(d, (40, 390, 520, 700), "Categories", [
        "UserId",
        "Title",
    ])
    arrow(d, 520, 620, 520, "1 → *")
    card(d, (620, 390, 1180, 700), "Expenses", [
        "CategoryId",
        "PaymentMethodId",
        "Amount, date",
    ])
    arrow(d, 1280, 1180, 520, "* ← 1")
    card(d, (1280, 390, 1840, 700), "Pay methods", [
        "Card, Cash",
        "Bank transfer, Other",
    ])

    card(d, (40, 750, 560, 1040), "Savings goals", [
        "UserId",
        "Target, current amount",
        "Allocation percent",
    ])
    card(d, (640, 750, 1240, 1040), "Live shift", [
        "UserId, unique",
        "JobId",
        "One open clock-in",
    ])
    d.text((1300, 820), "Users  1 → *  categories, goals, and jobs.", font=font(22), fill=INK)
    d.text((1300, 870), "Users  1 → 0..1  live shift.", font=font(22), fill=INK)
    d.text((1300, 940), "48 hours is a sum of shift hours.", font=font(22), fill=INK)
    d.text((1300, 990), "It is not a table.", font=font(22), fill=MUTED)
    return save(img, "diagram-erd.png")


def normalization():
    img = Image.new("RGB", (2400, 1100), WHITE)
    d = ImageDraw.Draw(img)
    panels = [
        (40, "UNF", "One spreadsheet row", [
            "Name, visa, and jobs in one cell.",
            "Cafe $28 and Retail $26 together.",
            "Shifts and expenses repeat.",
            "A visa change rewrites the row.",
        ]),
        (850, "1NF", "Atomic, still copied", [
            "One fact per row, with a key.",
            "The shift still stored UserId,",
            "the job title, and the rate.",
            "The expense stored names as text.",
        ]),
        (1660, "3NF", "What is running", [
            "Hourly rate lives only on the job.",
            "A shift stores JobId.",
            "An expense stores two keys:",
            "category and payment method.",
        ]),
    ]
    for x, title, sub, lines in panels:
        d.rounded_rectangle((x, 40, x + 700, 900), radius=14, fill=SOFT, outline=INK, width=2)
        d.rectangle((x, 40, x + 700, 130), fill=TEAL)
        d.text((x + 24, 58), title, font=font(32, True), fill=WHITE)
        d.text((x + 24, 160), sub, font=font(22, True), fill=INK)
        y = 230
        for line in lines:
            d.text((x + 24, y), line, font=font(22), fill=INK)
            y += 48
    d.line((750, 470, 834, 470), fill=INK, width=3)
    d.polygon([(836, 470), (820, 462), (820, 478)], fill=INK)
    d.line((1560, 470, 1644, 470), fill=INK, width=3)
    d.polygon([(1646, 470), (1630, 462), (1630, 478)], fill=INK)
    d.text((40, 940), "Migration name: NormalizeTo3NF. A saved shift keeps the pay it was given.", font=font(22), fill=INK)
    d.text((40, 990), "Changing Cafe from $28 to $30 does not rewrite old shifts.", font=font(22), fill=MUTED)
    return save(img, "diagram-normalization.png")


def use_case():
    img = Image.new("RGB", (2400, 1100), WHITE)
    d = ImageDraw.Draw(img)
    d.rounded_rectangle((430, 40, 1970, 1040), radius=16, outline=INK, width=2)
    d.text((520, 60), "OraEarn", font=font(26, True), fill=INK)

    def actor(x, y, name):
        d.ellipse((x + 28, y, x + 72, y + 44), outline=INK, width=3)
        d.line((x + 50, y + 44, x + 50, y + 110), fill=INK, width=3)
        d.line((x + 10, y + 70, x + 90, y + 70), fill=INK, width=3)
        d.line((x + 50, y + 110, x + 15, y + 170), fill=INK, width=3)
        d.line((x + 50, y + 110, x + 85, y + 170), fill=INK, width=3)
        d.text((x + 50, y + 188), name, font=font(22, True), fill=INK, anchor="mt")

    def oval(box, label):
        d.ellipse(box, fill=WHITE, outline=INK, width=2)
        x1, y1, x2, y2 = box
        d.text(((x1 + x2) / 2, (y1 + y2) / 2), label, font=font(18), fill=INK, anchor="mm")

    actor(80, 180, "Student")
    actor(2040, 180, "Admin")

    student = [
        (520, 140, 1100, 230, "Register and sign in"),
        (520, 270, 1100, 360, "Jobs and shifts"),
        (520, 400, 1100, 490, "Watch the 48-hour bar"),
        (520, 530, 1100, 620, "Expenses and savings"),
        (520, 660, 1100, 750, "Dashboard and export"),
        (520, 790, 1100, 880, "Notifications and Aura"),
    ]
    admin = [
        (1280, 220, 1880, 330, "Approve or reject"),
        (1280, 400, 1880, 510, "Open the ID file"),
        (1280, 580, 1880, 690, "Set the role"),
        (1280, 760, 1880, 870, "Platform analytics"),
    ]
    for box in student:
        oval(box[:4], box[4])
        d.line((170, 280, box[0], (box[1] + box[3]) / 2), fill=INK, width=2)
    for box in admin:
        oval(box[:4], box[4])
        d.line((2090, 280, box[2], (box[1] + box[3]) / 2), fill=INK, width=2)
    d.text((520, 960), "Student actions stay on the left. Admin actions stay on the right.", font=font(20), fill=MUTED)
    return save(img, "diagram-usecase.png")


def sequence():
    img = Image.new("RGB", (2400, 1100), WHITE)
    d = ImageDraw.Draw(img)
    names = ["React", "ShiftsController", "VisaCompliance", "PostgreSQL"]
    xs = [220, 820, 1460, 2060]
    for x, name in zip(xs, names):
        d.rounded_rectangle((x - 150, 30, x + 150, 100), radius=10, fill=TEAL)
        d.text((x, 65), name, font=font(20, True), fill=WHITE, anchor="mm")
        d.line((x, 100, x, 980), fill=LINE, width=2)

    steps = [
        (0, 1, "1  POST /api/Shifts  +  JWT"),
        (1, 1, "2  Shift.Calculate. Rate is HourlyRate."),
        (1, 2, "3  Student? Sum hours over 14 days."),
        (2, 3, "4  Read this user's shifts."),
        (3, 2, "5  Hours already saved."),
        (2, 1, "6  Over 48: stop. Nothing is inserted."),
        (1, 3, "7  Under 48: insert the shift."),
        (3, 0, "8  201. Savings goals recalculated."),
    ]
    y = 160
    for a, b, label in steps:
        x1, x2 = xs[a], xs[b]
        if a == b:
            d.line((x1, y, x1 + 80, y), fill=INK, width=3)
            d.line((x1 + 80, y, x1 + 80, y + 28), fill=INK, width=3)
            d.line((x1 + 80, y + 28, x1 + 12, y + 28), fill=INK, width=3)
            d.polygon([(x1 + 14, y + 21), (x1 + 14, y + 35), (x1, y + 28)], fill=INK)
            d.text((x1 + 96, y + 8), label, font=font(18), fill=INK)
            y += 90
            continue
        if x2 > x1:
            d.line((x1, y, x2 - 14, y), fill=INK, width=3)
            d.polygon([(x2 - 14, y - 7), (x2 - 14, y + 7), (x2, y)], fill=INK)
            d.text((min(x1, x2) + 16, y - 28), label, font=font(18), fill=INK)
        else:
            d.line((x1, y, x2 + 14, y), fill=INK, width=3)
            d.polygon([(x2 + 14, y - 7), (x2 + 14, y + 7), (x2, y)], fill=INK)
            f = font(18)
            tw = d.textlength(label, font=f)
            cx = (x1 + x2) / 2
            d.rectangle((cx - tw / 2 - 6, y - 30, cx + tw / 2 + 6, y - 6), fill=WHITE)
            d.text((cx - tw / 2, y - 28), label, font=f, fill=INK)
        y += 100
    return save(img, "diagram-sequence.png")


def paste_logo(img, name, x, y, size=72):
    path = OUT / "logos" / name
    if not path.exists():
        return
    logo = Image.open(path).convert("RGBA")
    logo.thumbnail((size, size))
    base = img.convert("RGBA")
    base.paste(logo, (int(x), int(y)), logo)
    img.paste(base.convert("RGB"))


def _flow_boxes(d, boxes, y, h):
    """boxes: (x, w, title, lines). Arrows sit in the gaps."""
    for x, w, title, lines in boxes:
        d.rounded_rectangle((x, y, x + w, y + h), radius=12, fill=WHITE, outline=INK, width=2)
        d.rectangle((x, y, x + w, y + 52), fill=TEAL)
        d.text((x + w / 2, y + 26), title, font=font(22, True), fill=WHITE, anchor="mm")
        yy = y + 72
        for line in lines:
            d.text((x + 18, yy), line, font=font(18), fill=INK)
            yy += 30
    for i in range(len(boxes) - 1):
        x1 = boxes[i][0] + boxes[i][1]
        x2 = boxes[i + 1][0]
        mid = y + h / 2
        d.line((x1, mid, x2 - 14, mid), fill=INK, width=3)
        d.polygon([(x2 - 14, mid - 8), (x2 - 14, mid + 8), (x2, mid)], fill=INK)


def architecture():
    img = Image.new("RGB", (2400, 1100), WHITE)
    d = ImageDraw.Draw(img)
    d.text((40, 16), "Service-based MVC", font=font(28, True), fill=INK)
    d.text((40, 58), "The controller never opens the database. A service does.", font=font(20), fill=MUTED)

    boxes = [
        (40, 400, "View", ["React pages", "Jobs, shifts, money", "Port 5173"]),
        (520, 400, "Controller", ["ASP.NET controllers", "JWT on the route", "Port 5203"]),
        (1000, 400, "Service", ["ShiftService", "AuthService", "VisaCompliance"]),
        (1480, 400, "Model", ["Entity classes", "EF Core", "DbContext"]),
        (1960, 400, "Database", ["PostgreSQL", "The tables", "SQL"]),
    ]
    _flow_boxes(d, boxes, 120, 300)
    paste_logo(img, "react.png", 40 + 400 - 88, 186, 64)
    paste_logo(img, "dotnet.png", 520 + 400 - 88, 186, 64)
    paste_logo(img, "postgres.png", 1960 + 400 - 88, 186, 64)
    roles = ["MVC  ·  View", "MVC  ·  Controller", "Service layer", "MVC  ·  Model", "Data"]
    for (x, w, _, _), role in zip(boxes, roles):
        d.text((x + w / 2, 440), role, font=font(16, True), fill=MUTED, anchor="mm")

    labels = [(480, "HTTP + JWT"), (960, "calls"), (1440, "uses"), (1920, "SQL")]
    for x, label in labels:
        f = font(15, True)
        tw = d.textlength(label, font=f)
        d.rectangle((x - tw / 2 - 4, 248, x + tw / 2 + 4, 272), fill=WHITE)
        d.text((x - tw / 2, 248), label, font=f, fill=INK)

    d.line((1320, 420, 1320, 560), fill=INK, width=3)
    d.polygon([(1312, 546), (1328, 546), (1320, 560)], fill=INK)
    d.rounded_rectangle((980, 560, 1420, 760), radius=12, fill=SOFT, outline=INK, width=2)
    paste_logo(img, "python.png", 1004, 628, 72)
    d.text((1090, 600), "Aura", font=font(22, True), fill=INK)
    d.text((1090, 648), "Python, port 8000", font=font(18), fill=INK)
    d.text((1090, 688), "The API calls it.", font=font(18), fill=MUTED)

    d.line((2360, 270, 2384, 270), fill=INK, width=2)
    d.line((2384, 270, 2384, 830), fill=INK, width=2)
    d.line((2384, 830, 254, 830), fill=INK, width=2)
    d.polygon([(268, 822), (268, 838), (240, 830)], fill=INK)
    f = font(18, True)
    label = "JSON returns to the page"
    tw = d.textlength(label, font=f)
    d.text((1100 - tw / 2, 788), label, font=f, fill=INK)

    d.text((40, 900), "1  The page sends the request with the JWT.", font=font(22), fill=INK)
    d.text((40, 948), "2  The controller calls a service.  3  The service uses EF Core.  4  PostgreSQL answers.", font=font(22), fill=INK)
    d.text((40, 1004), "SignalR uses the same API and the same token. Hub: /hubs/shifts.", font=font(20), fill=MUTED)
    return save(img, "diagram-architecture.png")


def tech_flow():
    img = Image.new("RGB", (2400, 780), WHITE)
    d = ImageDraw.Draw(img)
    boxes = [
        (30, 400, "React 19", ["TypeScript, Vite", "The view", "Port 5173"]),
        (510, 400, "ASP.NET Core 10", ["Controllers", "JWT on the call", "Port 5203"]),
        (990, 400, "Services", ["Business rules", "Auth, shifts, visa"]),
        (1470, 400, "EF Core", ["The model", "Migrations", "NormalizeTo3NF"]),
        (1950, 400, "PostgreSQL", ["The database", "Tables and queries"]),
    ]
    _flow_boxes(d, boxes, 30, 250)
    labels = [(470, "HTTP + JWT"), (950, "calls"), (1430, "uses"), (1910, "SQL")]
    for x, label in labels:
        f = font(16, True)
        tw = d.textlength(label, font=f)
        d.rectangle((x - tw / 2 - 4, 150, x + tw / 2 + 4, 176), fill=WHITE)
        d.text((x - tw / 2, 150), label, font=f, fill=INK)

    d.rounded_rectangle((40, 400, 1160, 720), radius=12, fill=SOFT, outline=INK, width=2)
    d.text((70, 440), "SignalR", font=font(24, True), fill=INK)
    d.text((70, 490), "Live shift updates on /hubs/shifts. Same JWT.", font=font(20), fill=INK)

    d.rounded_rectangle((1240, 400, 2360, 720), radius=12, fill=SOFT, outline=INK, width=2)
    d.text((1270, 440), "Python Aura", font=font(24, True), fill=INK)
    d.text((1270, 490), "FastAPI on port 8000. The API calls it. The app runs if it is off.", font=font(20), fill=INK)

    d.text((40, 740), "Tools: Visual Studio, Git, Jira, pgAdmin.", font=font(20), fill=MUTED)
    return save(img, "diagram-tech.png")


def main():
    for fn in (erd, normalization, use_case, sequence, architecture, tech_flow):
        print(fn())


if __name__ == "__main__":
    main()
