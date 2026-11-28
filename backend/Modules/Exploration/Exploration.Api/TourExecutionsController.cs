using Exploration.Application.TourExecutions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tour-executions")]
[Authorize(Roles = "explorer")]
public sealed class TourExecutionsController : ControllerBase
{
    private readonly TourExecutionsService _executionsService;
    private readonly TourExecutionsQueries _executionsQueries;

    public TourExecutionsController(
        TourExecutionsService executionsService,
        TourExecutionsQueries executionsQueries)
    {
        _executionsService = executionsService;
        _executionsQueries = executionsQueries;
    }

    [HttpPost]
    public async Task<ActionResult> Start(StartTourDto dto)
    {
        await _executionsService.StartAsync(User.GetUserId(), dto);
        return NoContent();
    }

    [HttpGet("active")]
    public async Task<ActionResult<ActiveTourDto>> GetActive()
    {
        var active = await _executionsQueries.GetActiveAsync(User.GetUserId());
        if (active is null)
        {
            return NoContent();
        }
        return active;
    }

    [HttpPost("active/position-checks")]
    public async Task<ActionResult<PositionCheckResultDto>> CheckPosition(CheckPositionDto dto)
    {
        return await _executionsService.CheckPositionAsync(User.GetUserId(), dto);
    }

    [HttpPost("active/abandonment")]
    public async Task<ActionResult> Abandon()
    {
        await _executionsService.AbandonAsync(User.GetUserId());
        return NoContent();
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TourExecutionDto>> GetExecution(Guid id)
    {
        return await _executionsQueries.GetExecutionAsync(User.GetUserId(), id);
    }
}
