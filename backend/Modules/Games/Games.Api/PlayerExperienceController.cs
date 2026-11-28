using Games.Application.PlayerExperience;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Games.Api;

[ApiController]
[Authorize(Roles = "explorer")]
[Route("api/games/experience")]
public sealed class PlayerExperienceController : ControllerBase
{
    private readonly PlayerExperienceQueries _queries;

    public PlayerExperienceController(PlayerExperienceQueries queries)
    {
        _queries = queries;
    }

    [HttpGet]
    public async Task<ActionResult<TotalXpDto>> GetTotalXp()
    {
        return await _queries.GetTotalXpAsync(User.GetUserId());
    }
}
