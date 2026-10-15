using System.Net;
using System.Net.Http.Json;
using OraEarn.Api.Jobs;
using OraEarn.Api.Shifts;

namespace OraEarn.Api.Tests.Blackbox;

public class ShiftsApiTests(OraEarnWebApplicationFactory factory) : IClassFixture<OraEarnWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task CreateShift_Overnight_CalculatesHoursAndEarnings()
    {
        await _client.AuthenticateAsTestUserAsync();
        var job = await CreateJobAsync("Night fill", 30m);

        var response = await _client.PostAsJsonAsync("/api/shifts", new
        {
            jobId = job.Id,
            workDate = "2026-09-20",
            startTime = "22:00:00",
            endTime = "06:00:00",
            breakMinutes = 30,
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var shift = await response.Content.ReadFromJsonAsync<ShiftResponse>();
        Assert.NotNull(shift);
        Assert.Equal(7.5m, shift.TotalHours);
        Assert.Equal(225m, shift.TotalEarned);
    }

    [Fact]
    public async Task CreateShift_SameStartAndEnd_ReturnsBadRequest()
    {
        await _client.AuthenticateAsTestUserAsync();
        var job = await CreateJobAsync("Barista", 28m);

        var response = await _client.PostAsJsonAsync("/api/shifts", new
        {
            jobId = job.Id,
            workDate = "2026-09-20",
            startTime = "09:00:00",
            endTime = "09:00:00",
            breakMinutes = 0,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateShift_JobNotOwned_ReturnsBadRequest()
    {
        await _client.AuthenticateAsTestUserAsync();

        var response = await _client.PostAsJsonAsync("/api/shifts", new
        {
            jobId = Guid.NewGuid(),
            workDate = "2026-09-20",
            startTime = "09:00:00",
            endTime = "17:00:00",
            breakMinutes = 30,
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<JobResponse> CreateJobAsync(string title, decimal rate)
    {
        var response = await _client.PostAsJsonAsync("/api/jobs", new { title, hourlyRate = rate, dateOfJoin = "2026-01-15" });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<JobResponse>())!;
    }
}
