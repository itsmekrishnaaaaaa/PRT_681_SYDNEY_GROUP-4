using OraEarn.Api.Users;

namespace OraEarn.Api.Expenses;

public class ExpenseCategory
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string UserId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser User { get; set; } = null!;
    public ICollection<Expense> Expenses { get; set; } = [];
}
