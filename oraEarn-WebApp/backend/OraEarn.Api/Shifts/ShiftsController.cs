using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OraEarn.Api.Shifts;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ShiftsController(
    ShiftService shiftService,
    ActiveShiftSessionService sessionService,
    ShiftNotificationService notifications) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ShiftResponse>>> GetAll(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        var userId = GetUserId();
        return Ok(await shiftService.GetAllAsync(userId, from, to));
    }

    [HttpGet("summary")]
    public async Task<ActionResult<ShiftSummaryResponse>> GetSummary(
        [FromQuery] DateOnly? from,
        [FromQuery] DateOnly? to)
    {
        var userId = GetUserId();
        return Ok(await shiftService.GetSummaryAsync(userId, from, to));
    }

    [HttpGet("active")]
    public async Task<ActionResult<ActiveShiftSessionResponse>> GetActiveSession()
    {
        var userId = GetUserId();
        var session = await sessionService.GetCurrentAsync(userId);
        return session is null ? NotFound() : Ok(session);
    }

    [HttpPost("clock-in")]
    public async Task<ActionResult<ActiveShiftSessionResponse>> ClockIn(ClockInRequest request)
    {
        try
        {
            var userId = GetUserId();
            var session = await sessionService.ClockInAsync(userId, request.JobId);
            return Ok(session);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("clock-out")]
    public async Task<ActionResult<ShiftResponse>> ClockOut()
    {
        try
        {
            var userId = GetUserId();
            var shift = await sessionService.ClockOutAsync(userId);
            await notifications.NotifyCreatedAsync(userId, shift);
            return Ok(shift);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("break/start")]
    public async Task<ActionResult<ActiveShiftSessionResponse>> StartBreak()
    {
        try
        {
            var userId = GetUserId();
            return Ok(await sessionService.StartBreakAsync(userId));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("break/end")]
    public async Task<ActionResult<ActiveShiftSessionResponse>> EndBreak()
    {
        try
        {
            var userId = GetUserId();
            return Ok(await sessionService.EndBreakAsync(userId));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShiftResponse>> GetById(Guid id)
    {
        var userId = GetUserId();
        var shift = await shiftService.GetByIdAsync(userId, id);
        return shift is null ? NotFound() : Ok(shift);
    }

    [HttpPost]
    public async Task<ActionResult<ShiftResponse>> Create(ShiftRequest request)
    {
        try
        {
            var userId = GetUserId();
            var shift = await shiftService.CreateAsync(userId, request);
            await notifications.NotifyCreatedAsync(userId, shift);
            return CreatedAtAction(nameof(GetById), new { id = shift.Id }, shift);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ShiftResponse>> Update(Guid id, ShiftRequest request)
    {
        try
        {
            var userId = GetUserId();
            var shift = await shiftService.UpdateAsync(userId, id, request);
            if (shift is not null)
            {
                await notifications.NotifyUpdatedAsync(userId, shift);
            }
            return shift is null ? NotFound() : Ok(shift);
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var userId = GetUserId();
        var deleted = await shiftService.DeleteAsync(userId, id);
        if (deleted)
        {
            await notifications.NotifyDeletedAsync(userId, id);
        }
        return deleted ? NoContent() : NotFound();
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();
}