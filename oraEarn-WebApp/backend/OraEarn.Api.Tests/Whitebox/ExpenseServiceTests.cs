using Microsoft.EntityFrameworkCore;
using OraEarn.Api.Data;
using OraEarn.Api.Expenses;

namespace OraEarn.Api.Tests.Whitebox;

public class ExpenseServiceTests
{
    private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

    [Fact]
    public void Calculate_IncomeAndExpenses_ReturnsRemainingAndSavingsRate()
    {
        var service = CreateService(CreateDb());
        var request = new ExpenseCalculatorRequest(2000m,
        [
            new ExpenseRequest(Guid.NewGuid(), 800m, Today, ExpenseKind.Fixed, "Card"),
            new ExpenseRequest(Guid.NewGuid(), 200m, Today, ExpenseKind.Variable, "Cash"),
        ]);

        var result = service.Calculate(request);

        Assert.Equal(1000m, result.TotalExpenses);
        Assert.Equal(1000m, result.RemainingBalance);
        Assert.Equal(50m, result.SavingsRatePercent);
    }

    [Fact]
    public void Calculate_ZeroIncome_ReturnsZeroSavingsRate()
    {
        var service = CreateService(CreateDb());

        var result = service.Calculate(new ExpenseCalculatorRequest(0m,
            [new ExpenseRequest(Guid.NewGuid(), 50m, Today, ExpenseKind.OneOff, "Cash")]));

        Assert.Equal(0m, result.SavingsRatePercent);
        Assert.Equal(-50m, result.RemainingBalance);
    }

    [Fact]
    public async Task CreateAsync_CategoryOwnedByAnotherUser_Throws()
    {
        var db = CreateDb();
        var foreignCategory = await AddCategoryAsync(db, "user-2", "Rent");

        var ex = await Assert.ThrowsAsync<ArgumentException>(() =>
            CreateService(db).CreateAsync("user-1",
                new ExpenseRequest(foreignCategory.Id, 100m, Today, ExpenseKind.Fixed, "Card")));

        Assert.Contains("category not found", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateAsync_BlankPaymentMethod_DefaultsToOther()
    {
        var db = CreateDb();
        var category = await AddCategoryAsync(db, "user-1", "Groceries");

        var expense = await CreateService(db).CreateAsync("user-1",
            new ExpenseRequest(category.Id, 42.5m, Today, ExpenseKind.Variable, "  "));

        Assert.Equal(PaymentMethodNames.Other, expense.PaymentMethod);
        Assert.Equal("Groceries", expense.CategoryTitle);
    }

    [Fact]
    public async Task GetSummaryAsync_GroupsByCategoryAndExcludesOtherUsers()
    {
        var db = CreateDb();
        var service = CreateService(db);
        var rent = await AddCategoryAsync(db, "user-1", "Rent");
        var food = await AddCategoryAsync(db, "user-1", "Food");
        var otherUsers = await AddCategoryAsync(db, "user-2", "Rent");

        await service.CreateAsync("user-1", new ExpenseRequest(rent.Id, 400m, Today, ExpenseKind.Fixed, "Card"));
        await service.CreateAsync("user-1", new ExpenseRequest(food.Id, 30m, Today, ExpenseKind.Variable, "Cash"));
        await service.CreateAsync("user-1", new ExpenseRequest(food.Id, 20m, Today, ExpenseKind.Variable, "Cash"));
        await service.CreateAsync("user-2", new ExpenseRequest(otherUsers.Id, 999m, Today, ExpenseKind.Fixed, "Card"));

        var summary = await service.GetSummaryAsync("user-1", null, null);

        Assert.Equal(450m, summary.TotalExpenses);
        Assert.Equal(3, summary.ExpenseCount);
        Assert.Equal(400m, summary.ByCategory["Rent"]);
        Assert.Equal(50m, summary.ByCategory["Food"]);
    }

    [Fact]
    public async Task DeleteAsync_ExpenseOfAnotherUser_ReturnsFalse()
    {
        var db = CreateDb();
        var service = CreateService(db);
        var category = await AddCategoryAsync(db, "user-2", "Rent");
        var expense = await service.CreateAsync("user-2", new ExpenseRequest(category.Id, 400m, Today, ExpenseKind.Fixed, "Card"));

        var deleted = await service.DeleteAsync("user-1", expense.Id);

        Assert.False(deleted);
        Assert.Equal(1, await db.Expenses.CountAsync());
    }

    private static async Task<ExpenseCategory> AddCategoryAsync(ApplicationDbContext db, string userId, string title)
    {
        var category = new ExpenseCategory { UserId = userId, Title = title };
        db.ExpenseCategories.Add(category);
        await db.SaveChangesAsync();
        return category;
    }

    private static ExpenseService CreateService(ApplicationDbContext db) =>
        new(db, new ExpenseCategoryService(db));

    private static ApplicationDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase($"ExpenseServiceTests-{Guid.NewGuid()}")
            .Options;
        return new ApplicationDbContext(options);
    }
}
