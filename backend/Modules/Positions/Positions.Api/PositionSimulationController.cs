using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Positions.Application.PositionSimulation;
using Shared.Api;

namespace Positions.Api;

[ApiController]
[Route("api/positions/mine")]
[Authorize(Roles = "explorer")]
public sealed class PositionSimulationController : ControllerBase
{
    private readonly PositionSimulationService _simulationService;
    private readonly PositionSimulationQueries _simulationQueries;

    public PositionSimulationController(
        PositionSimulationService simulationService,
        PositionSimulationQueries simulationQueries)
    {
        _simulationService = simulationService;
        _simulationQueries = simulationQueries;
    }

    [HttpGet]
    public async Task<ActionResult<CoordinatesDto>> GetMine()
    {
        var coordinates = await _simulationQueries.GetMineAsync(User.GetUserId());
        if (coordinates is null)
        {
            return NoContent();
        }
        return coordinates;
    }

    [HttpPut]
    public async Task<ActionResult> Set(CoordinatesDto dto)
    {
        await _simulationService.SetAsync(User.GetUserId(), dto);
        return NoContent();
    }
}
