using System.Net;
using System.Net.Http.Json;
using OraEarn.Api.Jobs;
using OraEarn.Api.Tests.Blackbox;

namespace OraEarn.Api.Tests.Blackbox;

public class JobsApiTests(OraEarnWebApplicationFactory factory) : IClassFixture<OraEarnWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetJobs_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/jobs");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateAndListJob_WithAuth_ReturnsCreatedJob()
    {
        await _client.AuthenticateAsTestUserAsync();

        var create = await _client.PostAsJsonAsync("/api/jobs", new
        {
            title = "Barista",
            hourlyRate = 28.5m,
            dateOfJoin = "2026-01-15",
        });

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);
        var job = await create.Content.ReadFromJsonAsync<JobResponse>();
        Assert.NotNull(job);
        Assert.Equal("Barista", job.Title);

        var list = await _client.GetAsync("/api/jobs");
        list.EnsureSuccessStatusCode();
        var jobs = await list.Content.ReadFromJsonAsync<List<JobResponse>>();
        Assert.Contains(jobs ?? [], j => j.Id == job.Id);
    }

    [Fact]
    public async Task CreateJob_WithInvalidRate_ReturnsBadRequest()
    {
        await _client.AuthenticateAsTestUserAsync();

        var response = await _client.PostAsJsonAsync("/api/jobs", new
        {
            title = "Invalid",
            hourlyRate = -1m,
            dateOfJoin = "2026-01-15",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
