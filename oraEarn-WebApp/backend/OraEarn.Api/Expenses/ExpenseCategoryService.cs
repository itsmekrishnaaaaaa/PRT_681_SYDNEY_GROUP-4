using Microsoft.EntityFrameworkCore;
using OraEarn.Api.Data;
using OraEarn.Api.Users;
using OraEarn.Api.Jobs;
using OraEarn.Api.Shifts;
using OraEarn.Api.SavingsGoals;
using OraEarn.Api.Notifications;

namespace OraEarn.Api.Expenses;

public class ExpenseCategoryService(ApplicationDbContext db)
{
    public async Task<IReadOnlyList<ExpenseCategoryResponse>> GetAllAsync(string userId)
    {
        var categories = await db.ExpenseCategories.AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Title)
            .ToListAsync();

        return categories.Select(Map).ToList();
    }

    public async Task<ExpenseCategoryResponse?> GetByIdAsync(string userId, Guid id)
    {
        var category = await db.ExpenseCategories.AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        return category is null ? null : Map(category);
    }

    public async Task<ExpenseCategory?> GetEntityAsync(string userId, Guid id) =>
        await db.ExpenseCategories.SingleOrDefaultAsync(c => c.Id == id && c.UserId == userId);

    public async Task<ExpenseCategoryResponse> CreateAsync(string userId, ExpenseCategoryRequest request)
    {
        var category = new ExpenseCategory
        {
            UserId = userId,
            Title = request.Title
        };

        db.ExpenseCategories.Add(category);
        await db.SaveChangesAsync();
        return Map(category);
    }

    public async Task<ExpenseCategoryResponse?> UpdateAsync(string userId, Guid id, ExpenseCategoryRequest request)
    {
        var category = await db.ExpenseCategories.SingleOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (category is null)
        {
            return null;
        }

        category.Title = request.Title;
        await db.SaveChangesAsync();
        return Map(category);
    }

    public async Task<bool> DeleteAsync(string userId, Guid id)
    {
        var category = await db.ExpenseCategories.SingleOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (category is null)
        {
            return false;
        }

        db.ExpenseCategories.Remove(category);
        await db.SaveChangesAsync();
        return true;
    }

    private static ExpenseCategoryResponse Map(ExpenseCategory category) =>
        new(category.Id, category.Title);
}