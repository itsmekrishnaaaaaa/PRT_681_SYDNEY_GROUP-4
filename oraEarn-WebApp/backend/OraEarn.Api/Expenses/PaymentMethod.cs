namespace OraEarn.Api.Expenses;

public class PaymentMethod
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;

    public ICollection<Expense> Expenses { get; set; } = [];
}

public static class PaymentMethodNames
{
    public const string Card = "Card";
    public const string Cash = "Cash";
    public const string BankTransfer = "Bank Transfer";
    public const string Other = "Other";

    public static readonly string[] All = [Card, Cash, BankTransfer, Other];
}
