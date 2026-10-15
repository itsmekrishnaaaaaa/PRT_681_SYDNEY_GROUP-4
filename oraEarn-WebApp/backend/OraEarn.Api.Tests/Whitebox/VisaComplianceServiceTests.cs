using Microsoft.EntityFrameworkCore;
using OraEarn.Api.Data;
using OraEarn.Api.Jobs;
using OraEarn.Api.Shifts;
using OraEarn.Api.Users;
using OraEarn.Api.VisaCompliance;

namespace OraEarn.Api.Tests.Whitebox;

public class VisaComplianceServiceTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public async Task GetStatusAsync_NonStudent_DoesNotApply()
    {
        var db = CreateDb();
        await SeedUserAsync(db, "user-1", visaStatus: "Citizen", workingRights: "Unlimited");

        var status = await new VisaComplianceService(db).GetStatusAsync("user-1");

        Assert.False(status.Applies);
    }

    [Fact]
    public async Task GetStatusAsync_StudentAt44Hours_IsNearLimit()
    {
        var db = CreateDb();
        var job = await SeedUserAsync(db, "user-1", visaStatus: "Student", workingRights: "Student");
        await AddShiftAsync(db, job, Today.AddDays(-1), 22m);
        await AddShiftAsync(db, job, Today.AddDays(-3), 22m);

        var status = await new VisaComplianceService(db).GetStatusAsync("user-1");

        Assert.True(status.Applies);
        Assert.Equal(44m, status.FortnightHoursWorked);
        Assert.Equal(4m, status.RemainingHours);
        Assert.True(status.IsNearLimit);
        Assert.False(status.IsOverLimit);
    }

    [Fact]
    public async Task GetStatusAsync_ShiftOlderThanFortnight_IsIgnored()
    {
        var db = CreateDb();
        var job = await SeedUserAsync(db, "user-1", visaStatus: "Student", workingRights: "Student");
        await AddShiftAsync(db, job, Today.AddDays(-14), 40m);
        await AddShiftAsync(db, job, Today, 5m);

        var status = await new VisaComplianceService(db).GetStatusAsync("user-1");

        Assert.Equal(5m, status.FortnightHoursWorked);
    }

    [Fact]
    public async Task ValidateShiftAsync_ExceedsFortyEightHours_Throws()
    {
        var db = CreateDb();
        var job = await SeedUserAsync(db, "user-1", visaStatus: "Student", workingRights: "Student");
        await AddShiftAsync(db, job, Today.AddDays(-2), 45m);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            new VisaComplianceService(db).ValidateShiftAsync("user-1", 4m, Today));

        Assert.Contains("visa work limit exceeded", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ValidateShiftAsync_ExactlyFortyEightHours_IsAllowed()
    {
        var db = CreateDb();
        var job = await SeedUserAsync(db, "user-1", visaStatus: "Student", workingRights: "Student");
        await AddShiftAsync(db, job, Today.AddDays(-2), 44m);

        await new VisaComplianceService(db).ValidateShiftAsync("user-1", 4m, Today);
    }

    [Fact]
    public void BuildNotifications_OverLimit_ReturnsErrorSeverity()
    {
        var status = new VisaComplianceResponse(true, "Student", "Student", 50m, 48m, 0m, true, false, Today.AddDays(-13), Today);

        var notifications = new VisaComplianceService(CreateDb()).BuildNotifications(status);

        var notification = Assert.Single(notifications);
        Assert.Equal("visa_limit_exceeded", notification.Type);
        Assert.Equal("error", notification.Severity);
    }

    private static async Task<Job> SeedUserAsync(ApplicationDbContext db, string userId, string visaStatus, string workingRights)
    {
        db.Users.Add(new ApplicationUser
        {
            Id = userId,
            UserName = $"{userId}@test.local",
            Email = $"{userId}@test.local",
            VisaStatus = visaStatus,
            WorkingRights = workingRights,
        });
        var job = new Job { UserId = userId, Title = "Barista", HourlyRate = 30m, DateOfJoin = Today.AddMonths(-1) };
        db.Jobs.Add(job);
        await db.SaveChangesAsync();
        return job;
    }

    private static async Task AddShiftAsync(ApplicationDbContext db, Job job, DateOnly date, decimal hours)
    {
        db.Shifts.Add(new Shift { JobId = job.Id, WorkDate = date, TotalHours = hours, TotalEarned = hours * job.HourlyRate });
        await db.SaveChangesAsync();
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"VisaComplianceTests-{Guid.NewGuid()}")
            .Options;
        return new ApplicationDbContext(options);
    }
}
