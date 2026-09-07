using Microsoft.EntityFrameworkCore;
using OraEarn.Api.Data;
using OraEarn.Api.Users;
using OraEarn.Api.Jobs;
using OraEarn.Api.Expenses;
using OraEarn.Api.SavingsGoals;
using OraEarn.Api.Notifications;

namespace OraEarn.Api.Shifts;

public class ActiveShiftSessionService(ApplicationDbContext db, JobService jobService, ShiftService shiftService)
{
    public async Task<ActiveShiftSessionResponse?> GetCurrentAsync(string userId)
    {
        var session = await db.ActiveShiftSessions.AsNoTracking()
            .Include(s => s.Job)
            .SingleOrDefaultAsync(s => s.UserId == userId);

        return session is null ? null : Map(session);
    }

    public async Task<ActiveShiftSessionResponse> ClockInAsync(string userId, Guid jobId)
    {
        var existing = await db.ActiveShiftSessions.SingleOrDefaultAsync(s => s.UserId == userId);
        if (existing is not null)
        {
            throw new InvalidOperationException("An active shift session already exists.");
        }

        var job = await jobService.GetEntityAsync(userId, jobId)
            ?? throw new ArgumentException("Job not found.");

        var now = DateTime.Now;
        var session = new ActiveShiftSession
        {
            UserId = userId,
            JobId = jobId,
            ShiftDate = DateOnly.FromDateTime(now),
            StartTime = TimeOnly.FromDateTime(now),
            BreakMinutesAccumulated = 0,
            BreakStartedAt = null
        };

        db.ActiveShiftSessions.Add(session);
        await db.SaveChangesAsync();
        session.Job = job;
        return Map(session);
    }

    public async Task<ShiftResponse> ClockOutAsync(string userId)
    {
        var session = await db.ActiveShiftSessions
            .Include(s => s.Job)
            .SingleOrDefaultAsync(s => s.UserId == userId)
            ?? throw new InvalidOperationException("No active shift session.");

        var breakMinutes = session.BreakMinutesAccumulated;
        if (session.BreakStartedAt.HasValue)
        {
            breakMinutes += (int)(DateTime.UtcNow - session.BreakStartedAt.Value).TotalMinutes;
        }

        var endTime = TimeOnly.FromDateTime(DateTime.Now);
        var shift = await shiftService.CreateAsync(userId, new ShiftRequest(
            session.JobId,
            session.ShiftDate,
            session.StartTime,
            endTime,
            breakMinutes));

        db.ActiveShiftSessions.Remove(session);
        await db.SaveChangesAsync();
        return shift;
    }

    public async Task<ActiveShiftSessionResponse> StartBreakAsync(string userId)
    {
        var session = await db.ActiveShiftSessions
            .Include(s => s.Job)
            .SingleOrDefaultAsync(s => s.UserId == userId)
            ?? throw new InvalidOperationException("No active shift session.");

        if (session.BreakStartedAt.HasValue)
        {
            throw new InvalidOperationException("Break already in progress.");
        }

        session.BreakStartedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
        return Map(session);
    }

    public async Task<ActiveShiftSessionResponse> EndBreakAsync(string userId)
    {
        var session = await db.ActiveShiftSessions
            .Include(s => s.Job)
            .SingleOrDefaultAsync(s => s.UserId == userId)
            ?? throw new InvalidOperationException("No active shift session.");

        if (!session.BreakStartedAt.HasValue)
        {
            throw new InvalidOperationException("No break in progress.");
        }

        session.BreakMinutesAccumulated += (int)(DateTime.UtcNow - session.BreakStartedAt.Value).TotalMinutes;
        session.BreakStartedAt = null;
        await db.SaveChangesAsync();
        return Map(session);
    }

    private static ActiveShiftSessionResponse Map(ActiveShiftSession session) =>
        new(
            session.Id,
            session.JobId,
            session.Job.Title,
            session.ShiftDate,
            session.StartTime,
            session.BreakMinutesAccumulated,
            session.BreakStartedAt,
            session.Job.HourlyRate);
}