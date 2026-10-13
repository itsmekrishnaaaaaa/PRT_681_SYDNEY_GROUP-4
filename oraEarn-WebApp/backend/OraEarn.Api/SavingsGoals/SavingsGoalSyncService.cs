using Microsoft.EntityFrameworkCore;
using OraEarn.Api.Data;
using OraEarn.Api.Expenses;
using OraEarn.Api.Shifts;

namespace OraEarn.Api.SavingsGoals;

public class SavingsGoalSyncService(ApplicationDbContext db)
{
    public async Task ApplyAsync(string userId)
    {
        var goals = await db.SavingsGoals.Where(g => g.UserId == userId).ToListAsync();
        if (goals.Count == 0)
        {
            return;
        }

        var earnings = await db.Shifts.ForUser(userId).SumAsync(s => (decimal?)s.TotalEarned) ?? 0;
        var expenses = await db.Expenses.ForUser(userId).SumAsync(e => (decimal?)e.Amount) ?? 0;
        SavingsGoalAllocator.Apply(goals, earnings, expenses);
        await db.SaveChangesAsync();
    }
}
