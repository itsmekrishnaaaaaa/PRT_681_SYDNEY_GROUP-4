using Microsoft.EntityFrameworkCore;
using OraEarn.Api.Data;

namespace OraEarn.Api.Expenses;

// Expenses carry no UserId column (3NF): ownership is resolved through Expense.Category.UserId, so
// every query joins the category or uses QueryExtensions.ForUser.
public class ExpenseService(ApplicationDbContext db, ExpenseCategoryService categoryService)
{
    /// <summary>Lists the user's expenses, newest first, optionally limited to an inclusive date range.</summary>
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

    /// <summary>Returns one expense, or null if it does not exist or belongs to another user.</summary>
    public async Task<ExpenseResponse?> GetByIdAsync(string userId, Guid id)
    {
        var expense = await TrackedExpenseQuery(userId)
            .SingleOrDefaultAsync(e => e.Id == id);

        return expense is null ? null : Map(expense);
    }

    /// <summary>Totals the user's spending, overall and per category title, over an optional date range.</summary>
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

    /// <summary>Lists the shared payment-method lookup values for the expense form.</summary>
    public async Task<IReadOnlyList<PaymentMethodResponse>> GetPaymentMethodsAsync()
    {
        return await db.PaymentMethods.AsNoTracking()
            .OrderBy(p => p.Name)
            .Select(p => new PaymentMethodResponse(p.Id, p.Name))
            .ToListAsync();
    }

    /// <summary>
    /// Budget calculator (what-if): totals planned expenses against a monthly income and returns the
    /// savings rate. Nothing is saved; zero or negative income gives a 0% rate instead of dividing by zero.
    /// </summary>
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

    /// <summary>
    /// Records an expense in one of the user's categories. Throws <see cref="ArgumentException"/> for an
    /// amount of zero or less or an unknown category.
    /// </summary>
    public async Task<ExpenseResponse> CreateAsync(string userId, ExpenseRequest request)
    {
        ValidateAmount(request.Amount);
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
        expense.Category = category;
        expense.PaymentMethod = paymentMethod;
        return Map(expense);
    }

    /// <summary>Edits one of the user's expenses with the same validation as create; null if not theirs.</summary>
    public async Task<ExpenseResponse?> UpdateAsync(string userId, Guid id, ExpenseRequest request)
    {
        ValidateAmount(request.Amount);
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
        return Map(expense);
    }

    /// <summary>Deletes one of the user's expenses. Returns false if it is not found or not theirs.</summary>
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
        return true;
    }

    // Enforced here, not only in the React form, so Swagger, scripts and other clients cannot store
    // zero or negative amounts that would distort every total (defect DEF-02).
    private static void ValidateAmount(decimal amount)
    {
        if (amount <= 0)
        {
            throw new ArgumentException("Amount must be greater than zero.");
        }
    }

    // Despite the name this query is read-only (AsNoTracking); it is the shared "user's expenses with
    // category and payment method" projection for the list and detail reads.
    private IQueryable<Expense> TrackedExpenseQuery(string userId) =>
        db.Expenses.AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.PaymentMethod)
            .ForUser(userId);

    // Blank means "Other". An unknown name is added to the shared PaymentMethods table rather than
    // rejected, so any user can grow the global list; the unique index stops exact duplicates.
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
