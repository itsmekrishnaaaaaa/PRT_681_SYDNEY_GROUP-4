using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace OraEarn.Api.Jobs;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class JobsController(JobService jobService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<JobResponse>>> GetAll()
    {
        var userId = GetUserId();
        return Ok(await jobService.GetAllAsync(userId));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobResponse>> GetById(Guid id)
    {
        var userId = GetUserId();
        var job = await jobService.GetByIdAsync(userId, id);
        return job is null ? NotFound() : Ok(job);
    }

    [HttpPost]
    public async Task<ActionResult<JobResponse>> Create(JobRequest request)
    {
        try
        {
            var userId = GetUserId();
            var job = await jobService.CreateAsync(userId, request);
            return CreatedAtAction(nameof(GetById), new { id = job.Id }, job);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<JobResponse>> Update(Guid id, JobRequest request)
    {
        try
        {
            var userId = GetUserId();
            var job = await jobService.UpdateAsync(userId, id, request);
            return job is null ? NotFound() : Ok(job);
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
        var deleted = await jobService.DeleteAsync(userId, id);
        return deleted ? NoContent() : NotFound();
    }

    private string GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? throw new UnauthorizedAccessException();
}