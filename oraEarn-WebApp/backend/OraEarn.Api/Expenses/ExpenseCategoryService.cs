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
    /// <summary>Lists the user's own expense categories alphabetically.</summary>
    public async Task<IReadOnlyList<ExpenseCategoryResponse>> GetAllAsync(string userId)
    {
        var categories = await db.ExpenseCategories.AsNoTracking()
            .Where(c => c.UserId == userId)
            .OrderBy(c => c.Title)
            .ToListAsync();

        return categories.Select(Map).ToList();
    }

    /// <summary>Returns one of the user's categories, or null if it does not exist or is not theirs.</summary>
    public async Task<ExpenseCategoryResponse?> GetByIdAsync(string userId, Guid id)
    {
        var category = await db.ExpenseCategories.AsNoTracking()
            .SingleOrDefaultAsync(c => c.Id == id && c.UserId == userId);

        return category is null ? null : Map(category);
    }

    /// <summary>Loads a tracked category owned by the user; ExpenseService uses it as its ownership check.</summary>
    public async Task<ExpenseCategory?> GetEntityAsync(string userId, Guid id) =>
        await db.ExpenseCategories.SingleOrDefaultAsync(c => c.Id == id && c.UserId == userId);

    /// <summary>Creates a category with a trimmed, non-blank title. Throws <see cref="ArgumentException"/> if blank.</summary>
    public async Task<ExpenseCategoryResponse> CreateAsync(string userId, ExpenseCategoryRequest request)
    {
        var category = new ExpenseCategory
        {
            UserId = userId,
            Title = ValidateTitle(request.Title)
        };

        db.ExpenseCategories.Add(category);
        await db.SaveChangesAsync();
        return Map(category);
    }

    /// <summary>Renames one of the user's categories; null if not theirs. Existing expenses follow the new name.</summary>
    public async Task<ExpenseCategoryResponse?> UpdateAsync(string userId, Guid id, ExpenseCategoryRequest request)
    {
        var title = ValidateTitle(request.Title);
        var category = await db.ExpenseCategories.SingleOrDefaultAsync(c => c.Id == id && c.UserId == userId);
        if (category is null)
        {
            return null;
        }

        category.Title = title;
        await db.SaveChangesAsync();
        return Map(category);
    }

    /// <summary>
    /// Deletes one of the user's categories. Returns false if not found. A category that still has
    /// expenses is protected by a Restrict rule and the save fails.
    /// </summary>
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

    // Validated before the lookup in UpdateAsync so a bad request is a 400 even for a missing category.
    private static string ValidateTitle(string? title) =>
        string.IsNullOrWhiteSpace(title)
            ? throw new ArgumentException("Category title is required.")
            : title.Trim();

    private static ExpenseCategoryResponse Map(ExpenseCategory category) =>
        new(category.Id, category.Title);
}