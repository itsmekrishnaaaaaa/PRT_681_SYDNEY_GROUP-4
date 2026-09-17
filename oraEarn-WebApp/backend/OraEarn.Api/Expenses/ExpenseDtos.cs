namespace OraEarn.Api.Expenses;

public record ExpenseCategoryRequest(string Title);

public record ExpenseCategoryResponse(Guid Id, string Title);

public record PaymentMethodResponse(Guid Id, string Name);

public record ExpenseRequest(
    Guid CategoryId,
    decimal Amount,
    DateOnly Date,
    ExpenseKind ExpenseKind,
    string PaymentMethod);

public record ExpenseResponse(
    Guid Id,
    Guid CategoryId,
    string CategoryTitle,
    decimal Amount,
    DateOnly Date,
    ExpenseKind ExpenseKind,
    string PaymentMethod);

public record ExpenseSummaryResponse(
    decimal TotalExpenses,
    int ExpenseCount,
    IReadOnlyDictionary<string, decimal> ByCategory);

public record ExpenseCalculatorRequest(
    decimal MonthlyIncome,
    IReadOnlyList<ExpenseRequest> Expenses);

public record ExpenseCalculatorResponse(
    decimal TotalExpenses,
    decimal RemainingBalance,
    decimal SavingsRatePercent,
    IReadOnlyDictionary<string, decimal> ByCategory);