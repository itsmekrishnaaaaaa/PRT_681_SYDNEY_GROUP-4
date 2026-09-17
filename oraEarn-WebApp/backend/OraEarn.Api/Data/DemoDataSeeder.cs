using Microsoft.AspNetCore.Identity;
using OraEarn.Api.Constants;
using OraEarn.Api.Expenses;
using OraEarn.Api.Jobs;
using OraEarn.Api.SavingsGoals;
using OraEarn.Api.Shifts;
using OraEarn.Api.Users;

namespace OraEarn.Api.Data;

public static class DemoDataSeeder
{
    /// <summary>Marker user — if present, demo data was already seeded.</summary>
    private const string MarkerEmail = "alice@oraearn.local";
    private const string DefaultPassword = "User123!";

    public static async Task SeedDemoDataAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        if (await userManager.FindByEmailAsync(MarkerEmail) is not null)
        {
            return;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var weekStart = GetWeekStartMonday(today);

        // --- Demo users (visa / work-rights profiles match registration business rules) ---

        var alice = await CreateDemoUserAsync(userManager, new DemoUserProfile
        {
            Email = "alice@oraearn.local",
            FirstName = "Alice",
            LastName = "Nguyen",
            Password = DefaultPassword,
            Role = Roles.User,
            DateOfBirth = new DateOnly(2002, 3, 15),
            Nationality = "Australian",
            PhoneNumber = "+61450321630",
            VisaType = "Subclass 500 – Student",
            VisaStatus = "Student",
            WorkingRights = "Student",
            VisaExpiryDate = today.AddYears(2),
            IdentityDocumentType = IdentityDocumentType.VisaGrantNotice,
            ApprovalStatus = AccountApprovalStatus.Approved,
            ApprovedAt = DateTime.UtcNow.AddDays(-85),
            AiReviewSummary = "Student visa (500) with matching Student work rights. ID verified.",
        });

        var bob = await CreateDemoUserAsync(userManager, new DemoUserProfile
        {
            Email = "bob@oraearn.local",
            FirstName = "Bob",
            LastName = "Smith",
            Password = DefaultPassword,
            Role = Roles.User,
            DateOfBirth = new DateOnly(1995, 7, 22),
            Nationality = "Australian",
            PhoneNumber = "+61412345678",
            VisaType = "Australian passport",
            VisaStatus = "Citizen",
            WorkingRights = "Full Time",
            VisaExpiryDate = null,
            IdentityDocumentType = IdentityDocumentType.Passport,
            ApprovalStatus = AccountApprovalStatus.Approved,
            ApprovedAt = DateTime.UtcNow.AddDays(-120),
            AiReviewSummary = "Australian citizen — full unrestricted work rights.",
        });

        var carol = await CreateDemoUserAsync(userManager, new DemoUserProfile
        {
            Email = "carol@oraearn.local",
            FirstName = "Carol",
            LastName = "Lee",
            Password = DefaultPassword,
            Role = Roles.User,
            DateOfBirth = new DateOnly(1990, 11, 8),
            Nationality = "Australian",
            PhoneNumber = "+61498765432",
            VisaType = "Subclass 189 – Skilled (Permanent)",
            VisaStatus = "TR",
            WorkingRights = "Full Time",
            VisaExpiryDate = today.AddYears(5),
            IdentityDocumentType = IdentityDocumentType.Passport,
            ApprovalStatus = AccountApprovalStatus.Approved,
            ApprovedAt = DateTime.UtcNow.AddDays(-200),
            AiReviewSummary = "Permanent resident (TR) — full work rights confirmed.",
        });

        // Pending registration for admin approval demo (cannot log in)
        await CreateDemoUserAsync(userManager, new DemoUserProfile
        {
            Email = "david.pending@oraearn.local",
            FirstName = "David",
            LastName = "Tran",
            Password = DefaultPassword,
            Role = Roles.User,
            DateOfBirth = new DateOnly(2003, 1, 10),
            Nationality = "Australian",
            PhoneNumber = "+61411222333",
            VisaType = "Subclass 500 – Student",
            VisaStatus = "Student",
            WorkingRights = "Student",
            VisaExpiryDate = today.AddYears(3),
            IdentityDocumentType = IdentityDocumentType.VisaGrantNotice,
            ApprovalStatus = AccountApprovalStatus.Pending,
            AiReviewSummary = "Awaiting manual review — student visa documents uploaded.",
        });

        await SeedAliceAsync(db, alice, today, weekStart);
        await SeedBobAsync(db, bob, weekStart);
        await SeedCarolAsync(db, carol, weekStart);

        await db.SaveChangesAsync();
    }

    private sealed class DemoUserProfile
    {
        public required string Email { get; init; }
        public required string FirstName { get; init; }
        public required string LastName { get; init; }
        public required string Password { get; init; }
        public required string Role { get; init; }
        public DateOnly DateOfBirth { get; init; }
        public string Nationality { get; init; } = "Australian";
        public string PhoneNumber { get; init; } = "+61400000000";
        public string VisaType { get; init; } = string.Empty;
        public string VisaStatus { get; init; } = string.Empty;
        public string WorkingRights { get; init; } = string.Empty;
        public DateOnly? VisaExpiryDate { get; init; }
        public IdentityDocumentType IdentityDocumentType { get; init; } = IdentityDocumentType.Passport;
        public AccountApprovalStatus ApprovalStatus { get; init; } = AccountApprovalStatus.Approved;
        public DateTime? ApprovedAt { get; init; }
        public string? AiReviewSummary { get; init; }
    }

    private static async Task<ApplicationUser> CreateDemoUserAsync(
        UserManager<ApplicationUser> userManager,
        DemoUserProfile profile)
    {
        var user = new ApplicationUser
        {
            UserName = profile.Email,
            Email = profile.Email,
            FirstName = profile.FirstName,
            LastName = profile.LastName,
            EmailConfirmed = true,
            CreatedAt = DateTime.UtcNow.AddDays(-90),
            DateOfBirth = profile.DateOfBirth,
            Nationality = profile.Nationality,
            PhoneNumber = profile.PhoneNumber,
            VisaType = profile.VisaType,
            VisaStatus = profile.VisaStatus,
            WorkingRights = profile.WorkingRights,
            VisaExpiryDate = profile.VisaExpiryDate,
            IdentityDocumentType = profile.IdentityDocumentType,
            ApprovalStatus = profile.ApprovalStatus,
            ApprovedAt = profile.ApprovedAt,
            AiReviewSummary = profile.AiReviewSummary,
            VisaDocumentFileName = $"{profile.LastName.ToLowerInvariant()}-id.pdf",
        };

        var result = await userManager.CreateAsync(user, profile.Password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create demo user {profile.Email}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        await userManager.AddToRoleAsync(user, profile.Role);
        return user;
    }

    /// <summary>
    /// Alice — student visa: heavy shifts in rolling 14-day window (~86 hrs, over 48 hr limit).
    /// </summary>
    private static async Task SeedAliceAsync(
        ApplicationDbContext db,
        ApplicationUser user,
        DateOnly today,
        DateOnly weekStart)
    {
        var cafe = new Job
        {
            UserId = user.Id,
            Title = "Cafe Barista",
            HourlyRate = 28m,
            DateOfJoin = weekStart.AddDays(-120),
        };
        var retail = new Job
        {
            UserId = user.Id,
            Title = "Retail Assistant",
            HourlyRate = 26m,
            DateOfJoin = weekStart.AddDays(-60),
        };
        db.Jobs.AddRange(cafe, retail);

        var dailyPattern = new (Job Job, TimeOnly Start, TimeOnly End, int BreakMinutes)[]
        {
            (cafe, new TimeOnly(7, 0), new TimeOnly(15, 0), 30),
            (retail, new TimeOnly(10, 0), new TimeOnly(18, 0), 45),
            (cafe, new TimeOnly(8, 0), new TimeOnly(16, 0), 30),
            (retail, new TimeOnly(12, 0), new TimeOnly(20, 0), 30),
            (cafe, new TimeOnly(7, 30), new TimeOnly(15, 30), 30),
            (retail, new TimeOnly(9, 0), new TimeOnly(17, 0), 45),
            (cafe, new TimeOnly(8, 0), new TimeOnly(14, 0), 15),
        };

        // Seed every day in the rolling 14-day compliance window (visa business rule)
        var fortnightStart = today.AddDays(-13);
        for (var offset = 0; offset < 14; offset++)
        {
            var workDate = fortnightStart.AddDays(offset);
            if (workDate > today)
            {
                continue;
            }

            var pattern = dailyPattern[offset % dailyPattern.Length];
            AddShift(db, workDate, pattern.Job, pattern.Start, pattern.End, pattern.BreakMinutes);
        }

        var categories = AddCategories(db, user.Id, ["Rent", "Food", "Transport", "Utilities", "Entertainment"]);
        AddExpenses(db, categories, weekStart,
        [
            ("Rent", 950m, ExpenseKind.Fixed, "Bank Transfer", 0),
            ("Food", 85.50m, ExpenseKind.Variable, "Card", 1),
            ("Transport", 45m, ExpenseKind.Variable, "Card", 2),
            ("Utilities", 120m, ExpenseKind.Fixed, "Bank Transfer", 0),
            ("Entertainment", 35m, ExpenseKind.Variable, "Cash", 3),
            ("Food", 62.30m, ExpenseKind.Variable, "Card", 4),
            ("Transport", 28m, ExpenseKind.Variable, "Card", 5),
            ("Entertainment", 55m, ExpenseKind.OneOff, "Card", 6),
            ("Food", 41.20m, ExpenseKind.Variable, "Cash", 2),
            ("Utilities", 18.50m, ExpenseKind.Variable, "Card", 1),
        ]);

        db.SavingsGoals.AddRange(
            new SavingsGoal
            {
                UserId = user.Id,
                Title = "Emergency fund",
                TargetAmount = 5000m,
                CurrentAmount = 1850m,
                AllocationPercentage = 35m,
                TargetDate = today.AddDays(120),
            },
            new SavingsGoal
            {
                UserId = user.Id,
                Title = "Holiday trip",
                TargetAmount = 2500m,
                CurrentAmount = 920m,
                AllocationPercentage = 20m,
                TargetDate = today.AddDays(90),
            });

        await Task.CompletedTask;
    }

    /// <summary>Bob — Australian citizen, full work rights, ~43 hrs/week (legal).</summary>
    private static async Task SeedBobAsync(ApplicationDbContext db, ApplicationUser user, DateOnly weekStart)
    {
        var warehouse = new Job
        {
            UserId = user.Id,
            Title = "Warehouse Picker",
            HourlyRate = 32m,
            DateOfJoin = weekStart.AddDays(-200),
        };
        var delivery = new Job
        {
            UserId = user.Id,
            Title = "Delivery Driver",
            HourlyRate = 30m,
            DateOfJoin = weekStart.AddDays(-45),
        };
        db.Jobs.AddRange(warehouse, delivery);

        AddWeekShifts(db, weekStart,
        [
            (warehouse, new TimeOnly(6, 0), new TimeOnly(14, 0), 30),
            (delivery, new TimeOnly(14, 0), new TimeOnly(22, 0), 20),
            (warehouse, new TimeOnly(6, 30), new TimeOnly(14, 30), 30),
            (delivery, new TimeOnly(13, 0), new TimeOnly(21, 0), 30),
            (warehouse, new TimeOnly(7, 0), new TimeOnly(15, 0), 45),
            (delivery, new TimeOnly(10, 0), new TimeOnly(18, 0), 30),
            (warehouse, new TimeOnly(8, 0), new TimeOnly(16, 0), 30),
        ]);

        var categories = AddCategories(db, user.Id, ["Rent", "Food", "Transport", "Insurance", "Subscriptions"]);
        AddExpenses(db, categories, weekStart,
        [
            ("Rent", 1100m, ExpenseKind.Fixed, "Bank Transfer", 0),
            ("Insurance", 95m, ExpenseKind.Fixed, "Bank Transfer", 0),
            ("Food", 78m, ExpenseKind.Variable, "Card", 1),
            ("Transport", 65m, ExpenseKind.Variable, "Card", 2),
            ("Subscriptions", 42m, ExpenseKind.Fixed, "Card", 1),
            ("Food", 54.80m, ExpenseKind.Variable, "Card", 3),
            ("Transport", 38m, ExpenseKind.Variable, "Cash", 4),
            ("Food", 91.20m, ExpenseKind.Variable, "Card", 5),
            ("Insurance", 22m, ExpenseKind.OneOff, "Card", 6),
            ("Subscriptions", 15m, ExpenseKind.Variable, "Card", 2),
        ]);

        db.SavingsGoals.AddRange(
            new SavingsGoal
            {
                UserId = user.Id,
                Title = "New car deposit",
                TargetAmount = 12000m,
                CurrentAmount = 4200m,
                AllocationPercentage = 30m,
                TargetDate = weekStart.AddDays(180),
            },
            new SavingsGoal
            {
                UserId = user.Id,
                Title = "Laptop upgrade",
                TargetAmount = 1800m,
                CurrentAmount = 650m,
                AllocationPercentage = 15m,
                TargetDate = weekStart.AddDays(60),
            });

        await Task.CompletedTask;
    }

    /// <summary>Carol — permanent resident (TR), full work rights, standard office hours.</summary>
    private static async Task SeedCarolAsync(ApplicationDbContext db, ApplicationUser user, DateOnly weekStart)
    {
        var office = new Job
        {
            UserId = user.Id,
            Title = "Office Administrator",
            HourlyRate = 35m,
            DateOfJoin = weekStart.AddDays(-300),
        };
        db.Jobs.Add(office);

        AddWeekShifts(db, weekStart,
        [
            (office, new TimeOnly(9, 0), new TimeOnly(17, 0), 60),
            (office, new TimeOnly(9, 0), new TimeOnly(17, 0), 45),
            (office, new TimeOnly(9, 0), new TimeOnly(17, 0), 60),
            (office, new TimeOnly(9, 0), new TimeOnly(17, 0), 45),
            (office, new TimeOnly(9, 0), new TimeOnly(17, 0), 60),
            (office, new TimeOnly(10, 0), new TimeOnly(16, 0), 30),
            (office, new TimeOnly(9, 0), new TimeOnly(13, 0), 15),
        ]);

        var categories = AddCategories(db, user.Id, ["Rent", "Groceries", "Phone", "Health", "Dining"]);
        AddExpenses(db, categories, weekStart,
        [
            ("Rent", 880m, ExpenseKind.Fixed, "Bank Transfer", 0),
            ("Groceries", 112m, ExpenseKind.Variable, "Card", 1),
            ("Phone", 55m, ExpenseKind.Fixed, "Card", 0),
            ("Health", 45m, ExpenseKind.Variable, "Card", 3),
            ("Dining", 68m, ExpenseKind.Variable, "Card", 2),
            ("Groceries", 94.50m, ExpenseKind.Variable, "Card", 4),
            ("Dining", 42m, ExpenseKind.Variable, "Cash", 5),
            ("Health", 28m, ExpenseKind.OneOff, "Card", 6),
            ("Phone", 12m, ExpenseKind.Variable, "Card", 1),
            ("Groceries", 76.40m, ExpenseKind.Variable, "Card", 3),
        ]);

        db.SavingsGoals.Add(new SavingsGoal
        {
            UserId = user.Id,
            Title = "Home deposit",
            TargetAmount = 20000m,
            CurrentAmount = 5400m,
            AllocationPercentage = 40m,
            TargetDate = weekStart.AddDays(365),
        });

        await Task.CompletedTask;
    }

    private static void AddWeekShifts(
        ApplicationDbContext db,
        DateOnly weekStart,
        (Job Job, TimeOnly Start, TimeOnly End, int BreakMinutes)[] weekPattern)
    {
        for (var day = 0; day < weekPattern.Length; day++)
        {
            var (job, start, end, breakMinutes) = weekPattern[day];
            AddShift(db, weekStart.AddDays(day), job, start, end, breakMinutes);
        }
    }

    private static void AddShift(
        ApplicationDbContext db,
        DateOnly workDate,
        Job job,
        TimeOnly start,
        TimeOnly end,
        int breakMinutes)
    {
        var (hours, earned) = Shift.Calculate(workDate, start, end, breakMinutes, job.HourlyRate);
        db.Shifts.Add(new Shift
        {
            JobId = job.Id,
            WorkDate = workDate,
            StartTime = start,
            EndTime = end,
            BreakMinutes = breakMinutes,
            TotalHours = hours,
            TotalEarned = earned,
        });
    }

    private static Dictionary<string, ExpenseCategory> AddCategories(
        ApplicationDbContext db,
        string userId,
        string[] titles)
    {
        var map = new Dictionary<string, ExpenseCategory>();
        foreach (var title in titles)
        {
            var category = new ExpenseCategory { UserId = userId, Title = title };
            db.ExpenseCategories.Add(category);
            map[title] = category;
        }

        return map;
    }

    private static void AddExpenses(
        ApplicationDbContext db,
        Dictionary<string, ExpenseCategory> categories,
        DateOnly weekStart,
        (string Category, decimal Amount, ExpenseKind Kind, string Payment, int DayOffset)[] items)
    {
        foreach (var (categoryTitle, amount, kind, payment, dayOffset) in items)
        {
            db.Expenses.Add(new Expense
            {
                CategoryId = categories[categoryTitle].Id,
                PaymentMethodId = ResolveSeedPaymentMethodId(db, payment),
                Amount = amount,
                Date = weekStart.AddDays(dayOffset),
                ExpenseKind = kind,
            });
        }
    }

    private static Guid ResolveSeedPaymentMethodId(ApplicationDbContext db, string name)
    {
        var match = db.PaymentMethods.Local.FirstOrDefault(p => p.Name == name)
            ?? db.PaymentMethods.FirstOrDefault(p => p.Name == name);
        if (match is not null)
        {
            return match.Id;
        }

        var created = new PaymentMethod { Name = name };
        db.PaymentMethods.Add(created);
        return created.Id;
    }

    private static DateOnly GetWeekStartMonday(DateOnly reference)
    {
        var dayOfWeek = (int)reference.DayOfWeek;
        var daysFromMonday = dayOfWeek == 0 ? 6 : dayOfWeek - 1;
        return reference.AddDays(-daysFromMonday);
    }
}
