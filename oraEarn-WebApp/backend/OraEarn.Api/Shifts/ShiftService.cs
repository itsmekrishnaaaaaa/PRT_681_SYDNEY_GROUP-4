using Microsoft.EntityFrameworkCore;
using OraEarn.Api.Data;
using OraEarn.Api.Jobs;
using OraEarn.Api.VisaCompliance;

namespace OraEarn.Api.Shifts;

// Shifts carry no UserId column (3NF): ownership is always resolved through Shift.Job.UserId,
// which is why every query here joins the job or uses QueryExtensions.ForUser.
public class ShiftService(ApplicationDbContext db, JobService jobService, VisaComplianceService visaComplianceService)
{
    /// <summary>Lists the user's shifts, newest first, optionally limited to an inclusive date range.</summary>
    public async Task<IReadOnlyList<ShiftResponse>> GetAllAsync(string userId, DateOnly? from, DateOnly? to)
    {
        var query = db.Shifts.AsNoTracking()
            .Include(s => s.Job)
            .ForUser(userId);

        if (from.HasValue)
        {
            query = query.Where(s => s.WorkDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(s => s.WorkDate <= to.Value);
        }

        var shifts = await query.OrderByDescending(s => s.WorkDate).ThenByDescending(s => s.StartTime).ToListAsync();
        return shifts.Select(Map).ToList();
    }

    /// <summary>Returns one shift, or null if it does not exist or belongs to another user.</summary>
    public async Task<ShiftResponse?> GetByIdAsync(string userId, Guid id)
    {
        var shift = await db.Shifts.AsNoTracking()
            .Include(s => s.Job)
            .SingleOrDefaultAsync(s => s.Id == id && s.Job.UserId == userId);

        return shift is null ? null : Map(shift);
    }

    /// <summary>Totals hours, earnings and shift count for the user over an optional date range.</summary>
    public async Task<ShiftSummaryResponse> GetSummaryAsync(string userId, DateOnly? from, DateOnly? to)
    {
        var query = db.Shifts.AsNoTracking().ForUser(userId);

        if (from.HasValue)
        {
            query = query.Where(s => s.WorkDate >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(s => s.WorkDate <= to.Value);
        }

        var shifts = await query.ToListAsync();
        return new ShiftSummaryResponse(
            shifts.Sum(s => s.TotalHours),
            shifts.Sum(s => s.TotalEarned),
            shifts.Count);
    }

    /// <summary>
    /// Logs a shift against one of the user's jobs. Throws <see cref="ArgumentException"/> for bad
    /// input or an unknown job, and <see cref="InvalidOperationException"/> if it breaks the visa limit.
    /// </summary>
    public async Task<ShiftResponse> CreateAsync(string userId, ShiftRequest request)
    {
        ValidateTimes(request.StartTime, request.EndTime);

        // Looking the job up through the user doubles as the ownership check: another user's job id
        // is reported as "not found" rather than revealing that it exists.
        var job = await jobService.GetEntityAsync(userId, request.JobId)
            ?? throw new ArgumentException("Job not found.");

        var (hours, earned) = Shift.Calculate(
            request.WorkDate,
            request.StartTime,
            request.EndTime,
            request.BreakMinutes,
            job.HourlyRate);

        // The visa rule needs the calculated hours, so it runs after Calculate and before anything is saved.
        await visaComplianceService.ValidateShiftAsync(userId, hours, request.WorkDate);

        var shift = new Shift
        {
            JobId = request.JobId,
            WorkDate = request.WorkDate,
            StartTime = request.StartTime,
            EndTime = request.EndTime,
            BreakMinutes = request.BreakMinutes,
            TotalHours = hours,
            TotalEarned = earned
        };

        db.Shifts.Add(shift);
        await db.SaveChangesAsync();

        shift.Job = job;
        return Map(shift);
    }

    /// <summary>
    /// Edits a shift and recalculates its stored hours and pay at the current job rate.
    /// Returns null if the shift is not the user's.
    /// </summary>
    public async Task<ShiftResponse?> UpdateAsync(string userId, Guid id, ShiftRequest request)
    {
        ValidateTimes(request.StartTime, request.EndTime);

        var shift = await db.Shifts.Include(s => s.Job)
            .SingleOrDefaultAsync(s => s.Id == id && s.Job.UserId == userId);
        if (shift is null)
        {
            return null;
        }

        var job = await jobService.GetEntityAsync(userId, request.JobId)
            ?? throw new ArgumentException("Job not found.");

        var (hours, earned) = Shift.Calculate(
            request.WorkDate,
            request.StartTime,
            request.EndTime,
            request.BreakMinutes,
            job.HourlyRate);

        // Passing the shift id excludes its old hours from the fortnight total; otherwise editing a
        // shift would count it twice and could falsely block the change.
        await visaComplianceService.ValidateShiftAsync(userId, hours, request.WorkDate, shift.Id);

        shift.JobId = request.JobId;
        shift.WorkDate = request.WorkDate;
        shift.StartTime = request.StartTime;
        shift.EndTime = request.EndTime;
        shift.BreakMinutes = request.BreakMinutes;
        shift.TotalHours = hours;
        shift.TotalEarned = earned;
        shift.Job = job;

        await db.SaveChangesAsync();
        return Map(shift);
    }

    /// <summary>Deletes one of the user's shifts. Returns false if it is not found or not theirs.</summary>
    public async Task<bool> DeleteAsync(string userId, Guid id)
    {
        var shift = await db.Shifts.Include(s => s.Job)
            .SingleOrDefaultAsync(s => s.Id == id && s.Job.UserId == userId);
        if (shift is null)
        {
            return false;
        }

        db.Shifts.Remove(shift);
        await db.SaveChangesAsync();
        return true;
    }

    // Only identical times are rejected; an earlier end time is a valid overnight shift (see Shift.Calculate).
    private static void ValidateTimes(TimeOnly start, TimeOnly end)
    {
        if (start == end)
        {
            throw new ArgumentException("Start and end time cannot be the same.");
        }
    }

    private static ShiftResponse Map(Shift shift) =>
        new(
            shift.Id,
            shift.JobId,
            shift.Job.Title,
            shift.WorkDate,
            shift.StartTime,
            shift.EndTime,
            shift.BreakMinutes,
            shift.TotalHours,
            shift.TotalEarned);
}
