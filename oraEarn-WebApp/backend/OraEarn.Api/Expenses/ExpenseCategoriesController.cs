using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OraEarn.Api.Expenses;

[ApiController]
[Authorize]
[Route("api/expense-categories")]
public class ExpenseCategoriesController(ExpenseCategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ExpenseCategoryResponse>>> GetAll()
    {
        var userId = GetUserId();
        return Ok(await categoryService.GetAllAsync(userId));
    }

    [HttpPost]
    public async Task<ActionResult<ExpenseCategoryResponse>> Create(ExpenseCategoryRequest request)
    {
        var userId = GetUserId();
        var category = await categoryService.CreateAsync(userId, request);
        return Ok(category);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ExpenseCategoryResponse>> Update(Guid id, ExpenseCategoryRequest request)
    {
        var userId = GetUserId();
        var category = await categoryService.UpdateAsync(userId, id, request);
        return category is null ? NotFound() : Ok(category);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var deleted = await categoryService.DeleteAsync(userId, id);
        return deleted ? NoContent() : NotFound();
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();
}