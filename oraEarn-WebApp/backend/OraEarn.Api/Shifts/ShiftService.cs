using Microsoft.EntityFrameworkCore;
using OraEarn.Api.Data;
using OraEarn.Api.Jobs;
using OraEarn.Api.SavingsGoals;
using OraEarn.Api.VisaCompliance;

namespace OraEarn.Api.Shifts;

public class ShiftService(
    ApplicationDbContext db,
    JobService jobService,
    VisaComplianceService visaComplianceService,
    SavingsGoalSyncService savingsGoalSync)
{
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

    public async Task<ShiftResponse?> GetByIdAsync(string userId, Guid id)
    {
        var shift = await db.Shifts.AsNoTracking()
            .Include(s => s.Job)
            .SingleOrDefaultAsync(s => s.Id == id && s.Job.UserId == userId);

        return shift is null ? null : Map(shift);
    }

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

    public async Task<ShiftResponse> CreateAsync(string userId, ShiftRequest request)
    {
        ValidateTimes(request.StartTime, request.EndTime);
        var job = await jobService.GetEntityAsync(userId, request.JobId)
            ?? throw new ArgumentException("Job not found.");

        var (hours, earned) = Shift.Calculate(
            request.WorkDate,
            request.StartTime,
            request.EndTime,
            request.BreakMinutes,
            job.HourlyRate);

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
        await savingsGoalSync.ApplyAsync(userId);

        shift.Job = job;
        return Map(shift);
    }

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
        await savingsGoalSync.ApplyAsync(userId);
        return Map(shift);
    }

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
        await savingsGoalSync.ApplyAsync(userId);
        return true;
    }

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
