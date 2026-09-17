using OraEarn.Api.Expenses;
using OraEarn.Api.Shifts;

namespace OraEarn.Api.Data;

public static class QueryExtensions
{
    public static IQueryable<Shift> ForUser(this IQueryable<Shift> query, string userId) =>
        query.Where(s => s.Job.UserId == userId);

    public static IQueryable<Expense> ForUser(this IQueryable<Expense> query, string userId) =>
        query.Where(e => e.Category.UserId == userId);
}
