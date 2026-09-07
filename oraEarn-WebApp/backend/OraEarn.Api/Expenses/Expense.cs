namespace OraEarn.Api.Expenses;

public class Expense
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid CategoryId { get; set; }
    public Guid PaymentMethodId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly Date { get; set; }
    public ExpenseKind ExpenseKind { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ExpenseCategory Category { get; set; } = null!;
    public PaymentMethod PaymentMethod { get; set; } = null!;
}
