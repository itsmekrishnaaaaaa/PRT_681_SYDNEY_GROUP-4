import json, base64, textwrap
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont

FIGS = Path(r"C:\projects\clz\oraEarn\(s395172) Dharma_Bahadur_Budhathoki\figures")
FONT_UI = ImageFont.truetype(r"C:\Windows\Fonts\segoeui.ttf", 16)
FONT_SM = ImageFont.truetype(r"C:\Windows\Fonts\segoeui.ttf", 12)
FONT_CODE = ImageFont.truetype(r"C:\Windows\Fonts\consola.ttf", 13)

# Decode add-shift screenshot
src = Path(r"C:\Users\budha\.cursor\browser-logs\cdp-response-Page.captureScreenshot-2026-09-17T21-43-06-333Z.json")
raw = json.loads(src.read_text(encoding="utf-8"))
(FIGS / "figure-shifts-add-modal.png").write_bytes(base64.b64decode(raw["data"]))

# Crop tiled calendar to one panel
cal = Image.open(FIGS / "figure-shifts-ui.png")
w, h = cal.size
cal.crop((0, 0, w // 2, h // 2)).save(FIGS / "figure-shifts-ui.png")
print("calendar crop", Image.open(FIGS / "figure-shifts-ui.png").size)
print("modal", (FIGS / "figure-shifts-add-modal.png").stat().st_size)


def glimpse(path: Path, title: str, subtitle: str, code: str):
    lines = []
    for line in code.replace("\t", "    ").splitlines():
        if len(line) <= 100:
            lines.append(line)
        else:
            lines.extend(textwrap.wrap(line, 100, replace_whitespace=False) or [""])
    lines = lines[:20]
    w, pad, header, lh = 980, 22, 72, 18
    img = Image.new("RGB", (w, header + pad + len(lines) * lh + pad), (18, 18, 18))
    d = ImageDraw.Draw(img)
    d.rectangle((0, 0, w, header), fill=(0, 0, 0))
    d.text((pad, 10), title, font=FONT_UI, fill=(255, 255, 255))
    d.text((pad, 34), subtitle, font=FONT_SM, fill=(160, 200, 220))
    d.text((pad, 52), "Branch app-s395172  ·  Dharma001  ·  S395172", font=FONT_SM, fill=(140, 140, 140))
    y = header + 8
    for line in lines:
        d.text((pad, y), line, font=FONT_CODE, fill=(225, 225, 225))
        y += lh
    img.save(path)
    print("glimpse", path.name)


glimpse(
    FIGS / "glimpse-seeder.png",
    "Backend glimpse — Demo seeder",
    "DemoDataSeeder.cs  ·  seeds jobs, 14-day shifts, expenses, savings",
    """private static async Task SeedAliceAsync(...)
{
    var cafe = new Job { Title = "Cafe Barista", HourlyRate = 28m, ... };
    var retail = new Job { Title = "Retail Assistant", HourlyRate = 26m, ... };
    db.Jobs.AddRange(cafe, retail);

    // Seed every day in the rolling 14-day visa window
    var fortnightStart = today.AddDays(-13);
    for (var offset = 0; offset < 14; offset++)
    {
        var workDate = fortnightStart.AddDays(offset);
        var pattern = dailyPattern[offset % dailyPattern.Length];
        AddShift(db, workDate, pattern.Job, pattern.Start, pattern.End, pattern.BreakMinutes);
    }
    AddCategories(db, user.Id, ["Rent", "Food", "Transport", "Utilities", "Entertainment"]);
}""",
)

glimpse(
    FIGS / "glimpse-shift-backend.png",
    "Backend glimpse — Shift CRUD",
    "ShiftService.cs  ·  create shift, calculate pay, run visa 48-hour check",
    """public async Task<ShiftResponse> CreateAsync(string userId, ShiftRequest request)
{
    ValidateTimes(request.StartTime, request.EndTime);
    var job = await jobService.GetEntityAsync(userId, request.JobId);

    var (hours, earned) = Shift.Calculate(
        request.WorkDate, request.StartTime, request.EndTime,
        request.BreakMinutes, job.HourlyRate);

    await visaComplianceService.ValidateShiftAsync(userId, hours, request.WorkDate);

    db.Shifts.Add(new Shift { JobId = request.JobId, TotalHours = hours, TotalEarned = earned });
    await db.SaveChangesAsync();
    return Map(shift);
}""",
)

glimpse(
    FIGS / "glimpse-shift-frontend.png",
    "Frontend glimpse — Shift calendar UI",
    "ShiftsPage.tsx  ·  Add shift opens a pop-out; calendar + clock-in",
    """<PageHeader
  title="Shift calendar"
  description="Log hours on the calendar, clock in live, or edit past shifts"
  action={
    <Button onClick={() => { setEditingShift(null); setModalOpen(true); }}>
      <Plus /> Add shift
    </Button>
  }
/>
<ShiftCalendar viewDate={viewDate} shifts={shifts} onDayClick={...} />
<ShiftModal open={modalOpen} jobs={jobs} onSubmit={...} onDelete={...} />""",
)

glimpse(
    FIGS / "glimpse-jobs-backend.png",
    "Backend glimpse — Jobs API",
    "JobsController.cs  ·  authorised CRUD for the signed-in user only",
    """[ApiController]
[Authorize]
[Route("api/[controller]")]
public class JobsController(JobService jobService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<JobResponse>>> GetAll()
        => Ok(await jobService.GetAllAsync(GetUserId()));

    [HttpPost]
    public async Task<ActionResult<JobResponse>> Create(JobRequest request)
        => CreatedAtAction(nameof(GetById), new { id = job.Id },
            await jobService.CreateAsync(GetUserId(), request));
}""",
)

glimpse(
    FIGS / "glimpse-jobs-frontend.png",
    "Frontend glimpse — Jobs pop-out",
    "JobModal.tsx  ·  create/edit job in a dialog, not a long page form",
    """export function JobModal({ open, job, onClose, onSubmit }: JobModalProps) {
  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent>
        <DialogTitle>{job ? "Edit job" : "Add a job"}</DialogTitle>
        <Input label="Job title" />
        <Input label="Hourly rate ($)" />
        <Input label="Start date" type="date" />
        <Button>Save job</Button>
      </DialogContent>
    </Dialog>
  );
}""",
)

glimpse(
    FIGS / "glimpse-auth-backend.png",
    "Backend glimpse — Login JWT",
    "AuthController.cs  ·  register, login, refresh, logout",
    """[HttpPost("login")]
[AllowAnonymous]
public async Task<ActionResult<AuthResponse>> Login(LoginRequest request)
{
    var (success, response, error) = await authService.LoginAsync(request);
    if (!success) return Unauthorized(new { message = error });
    return Ok(response);  // accessToken + refreshToken
}

[HttpPost("register")]
[AllowAnonymous]
public async Task<ActionResult<RegisterResponse>> Register(
    [FromForm] RegisterRequest request, [FromForm] IFormFile visaDocument)""",
)

glimpse(
    FIGS / "glimpse-auth-frontend.png",
    "Frontend glimpse — Password eye",
    "LoginPage.tsx  ·  show/hide password on the login form",
    """const [showPassword, setShowPassword] = useState(false);

<Input
  type={showPassword ? "text" : "password"}
  value={password}
  onChange={(e) => setPassword(e.target.value)}
/>
<button type="button" onClick={() => setShowPassword((v) => !v)}
        aria-label={showPassword ? "Hide password" : "Show password"}>
  {showPassword ? <EyeOff /> : <Eye />}
</button>""",
)

glimpse(
    FIGS / "glimpse-visa-backend.png",
    "Backend glimpse — 48-hour visa rule",
    "VisaComplianceService.cs  ·  student fortnight cap used when saving a shift",
    """public const decimal StudentFortnightHourLimit = 48m;

public async Task ValidateShiftAsync(string userId, decimal shiftHours, DateOnly workDate)
{
    var hours = await SumFortnightHoursAsync(userId, fortnightStart, today);
    var projected = hours + shiftHours;
    if (projected > StudentFortnightHourLimit)
        throw new InvalidOperationException("This shift would go over the 48-hour fortnight limit.");
}""",
)

print("OK")
