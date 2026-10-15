using Microsoft.EntityFrameworkCore;
using OraEarn.Api.Data;
using OraEarn.Api.Users;
using OraEarn.Api.Jobs;
using OraEarn.Api.Expenses;
using OraEarn.Api.SavingsGoals;
using OraEarn.Api.Notifications;

namespace OraEarn.Api.Shifts;

// Live clock-in/out. A session is a temporary row that becomes a normal Shift on clock-out; it is
// never reported on directly. Start time and date use server local time because shifts store
// wall-clock times, while break timing uses UTC because only the elapsed duration matters.
public class ActiveShiftSessionService(ApplicationDbContext db, JobService jobService, ShiftService shiftService)
{
    /// <summary>Returns the user's running clock-in session, or null if they are not clocked in.</summary>
    public async Task<ActiveShiftSessionResponse?> GetCurrentAsync(string userId)
    {
        var session = await db.ActiveShiftSessions.AsNoTracking()
            .Include(s => s.Job)
            .SingleOrDefaultAsync(s => s.UserId == userId);

        return session is null ? null : Map(session);
    }

    /// <summary>Starts a clock-in session on one of the user's jobs. Only one session per user may run.</summary>
    public async Task<ActiveShiftSessionResponse> ClockInAsync(string userId, Guid jobId)
    {
        // Friendly error for the common case; the unique index on UserId is the real guard against
        // two simultaneous clock-in requests both passing this check.
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

    /// <summary>
    /// Ends the session and saves it as a shift. A break still running is closed at clock-out time.
    /// </summary>
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

        // Going through ShiftService.CreateAsync means a clocked shift gets the same overnight
        // handling, pay calculation and visa check as a manual entry. If the visa check throws,
        // the session is kept so the user can still adjust it rather than lose the record.
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

    /// <summary>Starts an unpaid break in the running session.</summary>
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

    /// <summary>Ends the current break and adds its whole minutes to the session's break total.</summary>
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