using Exploration.Application.LocationSearches;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/location-searches")]
[Authorize(Roles = "explorer")]
public sealed class LocationSearchesController : ControllerBase
{
    private readonly LocationSearchesService _searchesService;

    public LocationSearchesController(LocationSearchesService searchesService)
    {
        _searchesService = searchesService;
    }

    [HttpPost]
    public async Task<ActionResult> Record(RecordLocationSearchDto dto)
    {
        await _searchesService.RecordAsync(User.GetUserId(), dto);
        return NoContent();
    }
}
