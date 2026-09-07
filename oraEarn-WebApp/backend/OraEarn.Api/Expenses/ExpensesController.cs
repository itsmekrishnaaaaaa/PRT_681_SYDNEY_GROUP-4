using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OraEarn.Api.Expenses;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ExpensesController(ExpenseService expenseService) : ControllerBase
{
    [HttpGet("payment-methods")]
    public async Task<ActionResult<IReadOnlyList<PaymentMethodResponse>>> GetPaymentMethods()
    {
        return Ok(await expenseService.GetPaymentMethodsAsync());
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExpenseResponse>>> GetAll(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        var userId = GetUserId();
        return Ok(await expenseService.GetAllAsync(userId, from, to));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ExpenseSummaryResponse>> GetSummary(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        var userId = GetUserId();
        return Ok(await expenseService.GetSummaryAsync(userId, from, to));
    }

    [HttpPost("calculate")]
    public ActionResult<ExpenseCalculatorResponse> Calculate(ExpenseCalculatorRequest request)
    {
        return Ok(expenseService.Calculate(request));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ExpenseResponse>> GetById(Guid id)
    {
        var userId = GetUserId();
        var expense = await expenseService.GetByIdAsync(userId, id);
        return expense is null ? NotFound() : Ok(expense);
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseResponse>> Create(ExpenseRequest request)
    {
        try
        {
            var userId = GetUserId();
            var expense = await expenseService.CreateAsync(userId, request);
            return CreatedAtAction(nameof(GetById), new { id = expense.Id }, expense);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExpenseResponse>> Update(Guid id, ExpenseRequest request)
    {
        try
        {
            var userId = GetUserId();
            var expense = await expenseService.UpdateAsync(userId, id, request);
            return expense is null ? NotFound() : Ok(expense);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var deleted = await expenseService.DeleteAsync(userId, id);
        return deleted ? NoContent() : NotFound();
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();
}