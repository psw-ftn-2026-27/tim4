using Games.Application.LocationChallengePlaying;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Games.Api;

[ApiController]
[Authorize(Roles = "explorer")]
[Route("api/games/location-challenges/{challengeId:guid}/attempt")]
public sealed class LocationChallengePlayingController : ControllerBase
{
    private readonly LocationChallengePlayingService _service;
    private readonly LocationChallengePlayingQueries _queries;

    public LocationChallengePlayingController(
        LocationChallengePlayingService service, LocationChallengePlayingQueries queries)
    {
        _service = service;
        _queries = queries;
    }

    [HttpGet]
    public async Task<ActionResult<LocationChallengePlayDto>> Get(Guid challengeId)
    {
        return await _queries.GetAsync(challengeId, User.GetUserId());
    }

    [HttpPost]
    public async Task<ActionResult> Activate(Guid challengeId, CheckLocationDto position)
    {
        await _service.ActivateAsync(challengeId, User.GetUserId(), position);
        return NoContent();
    }

    [HttpPost("points/{pointId:guid}/check")]
    public async Task<ActionResult> CheckLocation(Guid challengeId, Guid pointId, CheckLocationDto position)
    {
        await _service.CheckLocationAsync(challengeId, User.GetUserId(), pointId, position);
        return NoContent();
    }
}
