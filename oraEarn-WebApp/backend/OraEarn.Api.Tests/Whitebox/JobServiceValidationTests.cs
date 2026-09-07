using Microsoft.EntityFrameworkCore;
using OraEarn.Api.Data;
using OraEarn.Api.Jobs;

namespace OraEarn.Api.Tests.Whitebox;

public class JobServiceValidationTests
{
    [Fact]
    public async Task CreateAsync_EmptyTitle_Throws()
    {
        var db = CreateDb();
        var service = new JobService(db);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync("user-1", new JobRequest("", 25m, DateOnly.FromDateTime(DateTime.UtcNow))));

        Assert.Contains("Title", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_NonPositiveRate_Throws()
    {
        var db = CreateDb();
        var service = new JobService(db);

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync("user-1", new JobRequest("Barista", 0m, DateOnly.FromDateTime(DateTime.UtcNow))));

        Assert.Contains("rate", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"JobServiceTests-{Guid.NewGuid()}")
            .Options;
        return new ApplicationDbContext(options);
    }
}
