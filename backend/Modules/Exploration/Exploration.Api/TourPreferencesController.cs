using Exploration.Application.TourPreferences;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/preferences")]
[Authorize(Roles = "explorer")]
public sealed class TourPreferencesController : ControllerBase
{
    private readonly TourPreferencesService _preferencesService;
    private readonly TourPreferencesQueries _preferencesQueries;

    public TourPreferencesController(
        TourPreferencesService preferencesService,
        TourPreferencesQueries preferencesQueries)
    {
        _preferencesService = preferencesService;
        _preferencesQueries = preferencesQueries;
    }

    [HttpGet]
    public async Task<ActionResult<TourPreferenceDto>> GetMine()
    {
        var preference = await _preferencesQueries.GetMineAsync(User.GetUserId());
        if (preference is null)
        {
            return NoContent();
        }
        return preference;
    }

    [HttpPut]
    public async Task<ActionResult> Save(TourPreferenceDto dto)
    {
        await _preferencesService.SaveAsync(User.GetUserId(), dto);
        return NoContent();
    }

    [HttpDelete]
    public async Task<ActionResult> Delete()
    {
        await _preferencesService.DeleteAsync(User.GetUserId());
        return NoContent();
    }
}
