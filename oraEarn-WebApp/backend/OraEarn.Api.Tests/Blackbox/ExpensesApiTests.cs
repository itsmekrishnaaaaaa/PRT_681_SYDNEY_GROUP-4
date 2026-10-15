using System.Net;
using System.Net.Http.Json;
using OraEarn.Api.Expenses;

namespace OraEarn.Api.Tests.Blackbox;

public class ExpensesApiTests(OraEarnWebApplicationFactory factory) : IClassFixture<OraEarnWebApplicationFactory>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetExpenses_WithoutAuth_ReturnsUnauthorized()
    {
        var response = await _client.GetAsync("/api/expenses");
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategoryAndExpense_WithAuth_AppearsInSummary()
    {
        await _client.AuthenticateAsTestUserAsync();
        var category = await CreateCategoryAsync("Transport");

        var create = await _client.PostAsJsonAsync("/api/expenses", new
        {
            categoryId = category.Id,
            amount = 35.40m,
            date = "2026-10-01",
            expenseKind = "Variable",
            paymentMethod = "Card",
        });

        Assert.Equal(HttpStatusCode.Created, create.StatusCode);

        var summary = await _client.GetFromJsonAsync<ExpenseSummaryResponse>("/api/expenses/summary");
        Assert.NotNull(summary);
        Assert.Equal(35.40m, summary.ByCategory["Transport"]);
    }

    [Fact]
    public async Task CreateExpense_UnknownCategory_ReturnsBadRequest()
    {
        await _client.AuthenticateAsTestUserAsync();

        var response = await _client.PostAsJsonAsync("/api/expenses", new
        {
            categoryId = Guid.NewGuid(),
            amount = 10m,
            date = "2026-10-01",
            expenseKind = "OneOff",
            paymentMethod = "Cash",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateExpense_NegativeAmount_ReturnsBadRequest()
    {
        await _client.AuthenticateAsTestUserAsync();
        var category = await CreateCategoryAsync("Refund test");

        var response = await _client.PostAsJsonAsync("/api/expenses", new
        {
            categoryId = category.Id,
            amount = -50m,
            date = "2026-10-01",
            expenseKind = "OneOff",
            paymentMethod = "Cash",
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task CreateCategory_EmptyTitle_ReturnsBadRequest()
    {
        await _client.AuthenticateAsTestUserAsync();

        var response = await _client.PostAsJsonAsync("/api/expense-categories", new { title = "" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    private async Task<ExpenseCategoryResponse> CreateCategoryAsync(string title)
    {
        var response = await _client.PostAsJsonAsync("/api/expense-categories", new { title });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<ExpenseCategoryResponse>())!;
    }
}
