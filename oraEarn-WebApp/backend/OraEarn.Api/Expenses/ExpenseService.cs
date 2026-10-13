using Microsoft.EntityFrameworkCore;
using OraEarn.Api.Data;
using OraEarn.Api.SavingsGoals;

namespace OraEarn.Api.Expenses;

public class ExpenseService(ApplicationDbContext db, ExpenseCategoryService categoryService, SavingsGoalSyncService savingsGoalSync)
{
    public async Task<IReadOnlyList<ExpenseResponse>> GetAllAsync(string userId, DateOnly? from, DateOnly? to)
    {
        var query = TrackedExpenseQuery(userId);

        if (from.HasValue)
        {
            query = query.Where(e => e.Date >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(e => e.Date <= to.Value);
        }

        var expenses = await query.OrderByDescending(e => e.Date).ToListAsync();
        return expenses.Select(Map).ToList();
    }

    public async Task<ExpenseResponse?> GetByIdAsync(string userId, Guid id)
    {
        var expense = await TrackedExpenseQuery(userId)
            .SingleOrDefaultAsync(e => e.Id == id);

        return expense is null ? null : Map(expense);
    }

    public async Task<ExpenseSummaryResponse> GetSummaryAsync(string userId, DateOnly? from, DateOnly? to)
    {
        var query = db.Expenses.AsNoTracking()
            .Include(e => e.Category)
            .ForUser(userId);

        if (from.HasValue)
        {
            query = query.Where(e => e.Date >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(e => e.Date <= to.Value);
        }

        var expenses = await query.ToListAsync();
        var byCategory = expenses
            .GroupBy(e => e.Category.Title)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

        return new ExpenseSummaryResponse(
            expenses.Sum(e => e.Amount),
            expenses.Count,
            byCategory);
    }

    public async Task<IReadOnlyList<PaymentMethodResponse>> GetPaymentMethodsAsync()
    {
        return await db.PaymentMethods.AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new PaymentMethodResponse(p.Id, p.Name))
            .ToListAsync();
    }

    public ExpenseCalculatorResponse Calculate(ExpenseCalculatorRequest request)
    {
        var byCategory = request.Expenses
            .GroupBy(_ => "Planned")
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

        var total = request.Expenses.Sum(e => e.Amount);
        var remaining = request.MonthlyIncome - total;
        var savingsRate = request.MonthlyIncome <= 0
            ? 0
            : Math.Round(remaining / request.MonthlyIncome * 100, 2);

        return new ExpenseCalculatorResponse(total, remaining, savingsRate, byCategory);
    }

    public async Task<ExpenseResponse> CreateAsync(string userId, ExpenseRequest request)
    {
        var category = await categoryService.GetEntityAsync(userId, request.CategoryId)
            ?? throw new ArgumentException("Expense category not found.");
        var paymentMethod = await ResolvePaymentMethodAsync(request.PaymentMethod);

        var expense = new Expense
        {
            CategoryId = request.CategoryId,
            PaymentMethodId = paymentMethod.Id,
            Amount = request.Amount,
            Date = request.Date,
            ExpenseKind = request.ExpenseKind,
        };

        db.Expenses.Add(expense);
        await db.SaveChangesAsync();
        await savingsGoalSync.ApplyAsync(userId);
        expense.Category = category;
        expense.PaymentMethod = paymentMethod;
        return Map(expense);
    }

    public async Task<ExpenseResponse?> UpdateAsync(string userId, Guid id, ExpenseRequest request)
    {
        var expense = await db.Expenses
            .Include(e => e.Category)
            .Include(e => e.PaymentMethod)
            .SingleOrDefaultAsync(e => e.Id == id && e.Category.UserId == userId);
        if (expense is null)
        {
            return null;
        }

        var category = await categoryService.GetEntityAsync(userId, request.CategoryId)
            ?? throw new ArgumentException("Expense category not found.");
        var paymentMethod = await ResolvePaymentMethodAsync(request.PaymentMethod);

        expense.CategoryId = request.CategoryId;
        expense.PaymentMethodId = paymentMethod.Id;
        expense.Amount = request.Amount;
        expense.Date = request.Date;
        expense.ExpenseKind = request.ExpenseKind;
        expense.Category = category;
        expense.PaymentMethod = paymentMethod;

        await db.SaveChangesAsync();
        await savingsGoalSync.ApplyAsync(userId);
        return Map(expense);
    }

    public async Task<bool> DeleteAsync(string userId, Guid id)
    {
        var expense = await db.Expenses
            .Include(e => e.Category)
            .SingleOrDefaultAsync(e => e.Id == id && e.Category.UserId == userId);
        if (expense is null)
        {
            return false;
        }

        db.Expenses.Remove(expense);
        await db.SaveChangesAsync();
        await savingsGoalSync.ApplyAsync(userId);
        return true;
    }

    private IQueryable<Expense> TrackedExpenseQuery(string userId) =>
        db.Expenses.AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.PaymentMethod)
            .ForUser(userId);

    private async Task<PaymentMethod> ResolvePaymentMethodAsync(string name)
    {
        var normalized = string.IsNullOrWhiteSpace(name) ? PaymentMethodNames.Other : name.Trim();
        var match = await db.PaymentMethods.SingleOrDefaultAsync(p => p.Name == normalized);
        if (match is not null)
        {
            return match;
        }

        var created = new PaymentMethod { Name = normalized };
        db.PaymentMethods.Add(created);
        await db.SaveChangesAsync();
        return created;
    }

    private static ExpenseResponse Map(Expense expense) =>
        new(
            expense.Id,
            expense.CategoryId,
            expense.Category.Title,
            expense.Amount,
            expense.Date,
            expense.ExpenseKind,
            expense.PaymentMethod.Name);
}
