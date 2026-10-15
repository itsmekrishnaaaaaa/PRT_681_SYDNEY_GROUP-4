using Microsoft.EntityFrameworkCore;
using OraEarn.Api.Data;

namespace OraEarn.Api.Jobs;

public class JobService(ApplicationDbContext db)
{
    /// <summary>Lists the user's jobs alphabetically by title.</summary>
    public async Task<IReadOnlyList<JobResponse>> GetAllAsync(string userId)
    {
        var jobs = await db.Jobs.AsNoTracking()
            .Where(j => j.UserId == userId)
            .OrderBy(j => j.Title)
            .ToListAsync();

        return jobs.Select(Map).ToList();
    }

    /// <summary>Returns one of the user's jobs, or null if it does not exist or is not theirs.</summary>
    public async Task<JobResponse?> GetByIdAsync(string userId, Guid id)
    {
        var job = await db.Jobs.AsNoTracking()
            .SingleOrDefaultAsync(j => j.Id == id && j.UserId == userId);

        return job is null ? null : Map(job);
    }

    /// <summary>
    /// Loads a tracked job owned by the user. Other modules use this as their ownership check before
    /// attaching shifts or clock-in sessions to a job.
    /// </summary>
    public async Task<Job?> GetEntityAsync(string userId, Guid id) =>
        await db.Jobs.SingleOrDefaultAsync(j => j.Id == id && j.UserId == userId);

    /// <summary>Creates a job for the user. Throws <see cref="ArgumentException"/> for a blank title or a rate of zero or less.</summary>
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

    /// <summary>
    /// Edits a job. A new hourly rate applies to shifts saved from now on; existing shifts keep the pay
    /// stored when they were logged. Returns null if the job is not the user's.
    /// </summary>
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

    /// <summary>
    /// Deletes one of the user's jobs. Returns false if not found. A job that still has shifts or a
    /// running clock-in session is protected by a Restrict rule and the save fails.
    /// </summary>
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