using Microsoft.EntityFrameworkCore;
using OraEarn.Api.Data;

namespace OraEarn.Api.Jobs;

public class JobService(ApplicationDbContext db)
{
    public async Task<IReadOnlyList<JobResponse>> GetAllAsync(string userId)
    {
        var jobs = await db.Jobs.AsNoTracking()
            .Where(j => j.UserId == userId)
            .OrderBy(j => j.Title)
            .ToListAsync();

        return jobs.Select(Map).ToList();
    }

    public async Task<JobResponse?> GetByIdAsync(string userId, Guid id)
    {
        var job = await db.Jobs.AsNoTracking()
            .SingleOrDefaultAsync(j => j.Id == id && j.UserId == userId);

        return job is null ? null : Map(job);
    }

    public async Task<Job?> GetEntityAsync(string userId, Guid id) =>
        await db.Jobs.SingleOrDefaultAsync(j => j.Id == id && j.UserId == userId);

    public async Task<JobResponse> CreateAsync(string userId, JobRequest request)
    {
        Validate(request);

        var job = new Job
        {
            UserId = userId,
            Title = request.Title,
            HourlyRate = request.HourlyRate,
            DateOfJoin = request.DateOfJoin
        };

        db.Jobs.Add(job);
        await db.SaveChangesAsync();
        return Map(job);
    }

    public async Task<JobResponse?> UpdateAsync(string userId, Guid id, JobRequest request)
    {
        Validate(request);

        var job = await db.Jobs.SingleOrDefaultAsync(j => j.Id == id && j.UserId == userId);
        if (job is null)
        {
            return null;
        }

        job.Title = request.Title;
        job.HourlyRate = request.HourlyRate;
        job.DateOfJoin = request.DateOfJoin;

        await db.SaveChangesAsync();
        return Map(job);
    }

    public async Task<bool> DeleteAsync(string userId, Guid id)
    {
        var job = await db.Jobs.SingleOrDefaultAsync(j => j.Id == id && j.UserId == userId);
        if (job is null)
        {
            return false;
        }

        db.Jobs.Remove(job);
        await db.SaveChangesAsync();
        return true;
    }

    private static JobResponse Map(Job job) =>
        new(job.Id, job.Title, job.HourlyRate, job.DateOfJoin);

    private static void Validate(JobRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Title is required.");
        }

        if (request.HourlyRate <= 0)
        {
            throw new ArgumentException("Hourly rate must be greater than zero.");
        }
    }
}