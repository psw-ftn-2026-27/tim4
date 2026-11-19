using Games.Application.LocationChallengeBrowsing;
using Games.Application.LocationChallenges;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain;

namespace Games.Api;

[ApiController]
[Authorize(Roles = "administrator,explorer")]
[Route("api/games/location-challenges")]
public sealed class LocationChallengeBrowsingController : ControllerBase
{
    private readonly LocationChallengeBrowsingQueries _queries;

    public LocationChallengeBrowsingController(LocationChallengeBrowsingQueries queries)
    {
        _queries = queries;
    }
    [HttpGet]
    public async Task<ActionResult<PageResult<LocationChallengeDto>>> GetPage(int page = 1, int pageSize = 10)
    {
        return await _queries.GetPageAsync(page, pageSize);
    }
}
