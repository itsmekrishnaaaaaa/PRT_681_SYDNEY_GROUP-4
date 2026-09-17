using Microsoft.EntityFrameworkCore;
using OraEarn.Api.Data;
using OraEarn.Api.AiAdvisor;
using OraEarn.Api.Users;
using OraEarn.Api.Jobs;
using OraEarn.Api.Shifts;
using OraEarn.Api.Expenses;
using OraEarn.Api.Notifications;

namespace OraEarn.Api.SavingsGoals;

public class SavingsGoalService(ApplicationDbContext db)
{
    public async Task<IReadOnlyList<SavingsGoalResponse>> GetAllAsync(string userId)
    {
        var goals = await db.SavingsGoals.AsNoTracking()
            .Where(g => g.UserId == userId)
            .OrderByDescending(g => g.CreatedAt)
            .ToListAsync();

        return goals.Select(Map).ToList();
    }

    public async Task<SavingsGoalResponse?> GetByIdAsync(string userId, Guid id)
    {
        var goal = await db.SavingsGoals.AsNoTracking()
            .SingleOrDefaultAsync(g => g.Id == id && g.UserId == userId);

        return goal is null ? null : Map(goal);
    }

    public async Task<SavingsGoalResponse> CreateAsync(string userId, SavingsGoalRequest request)
    {
        ValidateAllocation(request.AllocationPercentage);

        var goal = new SavingsGoal
        {
            UserId = userId,
            Title = request.Title,
            TargetAmount = request.TargetAmount,
            CurrentAmount = request.CurrentAmount,
            AllocationPercentage = request.AllocationPercentage,
            TargetDate = request.TargetDate
        };

        db.SavingsGoals.Add(goal);
        await db.SaveChangesAsync();
        return Map(goal);
    }

    public async Task<SavingsGoalResponse?> UpdateAsync(string userId, Guid id, SavingsGoalRequest request)
    {
        ValidateAllocation(request.AllocationPercentage);

        var goal = await db.SavingsGoals.SingleOrDefaultAsync(g => g.Id == id && g.UserId == userId);
        if (goal is null)
        {
            return null;
        }

        goal.Title = request.Title;
        goal.TargetAmount = request.TargetAmount;
        goal.CurrentAmount = request.CurrentAmount;
        goal.AllocationPercentage = request.AllocationPercentage;
        goal.TargetDate = request.TargetDate;

        await db.SaveChangesAsync();
        return Map(goal);
    }

    public async Task<SavingsGoalResponse?> ContributeAsync(string userId, Guid id, SavingsGoalContributionRequest request)
    {
        if (request.Amount <= 0)
        {
            throw new ArgumentException("Contribution amount must be greater than zero.");
        }

        var goal = await db.SavingsGoals.SingleOrDefaultAsync(g => g.Id == id && g.UserId == userId);
        if (goal is null)
        {
            return null;
        }

        goal.CurrentAmount += request.Amount;
        await db.SaveChangesAsync();
        return Map(goal);
    }

    public async Task<bool> DeleteAsync(string userId, Guid id)
    {
        var goal = await db.SavingsGoals.SingleOrDefaultAsync(g => g.Id == id && g.UserId == userId);
        if (goal is null)
        {
            return false;
        }

        db.SavingsGoals.Remove(goal);
        await db.SaveChangesAsync();
        return true;
    }

    private static void ValidateAllocation(decimal allocation)
    {
        if (allocation is < 0 or > 100)
        {
            throw new ArgumentException("Allocation percentage must be between 0 and 100.");
        }
    }

    private static SavingsGoalResponse Map(SavingsGoal goal) =>
        new(
            goal.Id,
            goal.Title,
            goal.TargetAmount,
            goal.CurrentAmount,
            goal.AllocationPercentage,
            goal.TargetDate,
            goal.Status,
            goal.ProgressPercent,
            goal.RemainingAmount);
}