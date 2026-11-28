using Games.Application.SocialChallengePlaying;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Games.Api;

[ApiController]
[Authorize(Roles = "explorer")]
[Route("api/games/social-challenges")]
public sealed class SocialChallengePlayingController : ControllerBase
{
    private readonly SocialChallengePlayingService _service;
    private readonly SocialChallengePlayingQueries _queries;

    public SocialChallengePlayingController(SocialChallengePlayingService service,
        SocialChallengePlayingQueries queries)
    {
        _service = service;
        _queries = queries;
    }

    [HttpGet("map")]
    public async Task<ActionResult<List<SocialChallengeMapItemDto>>> GetMap()
    {
        return await _queries.GetMapAsync(User.GetUserId());
    }

    [HttpGet("{challengeId:guid}/run")]
    public async Task<ActionResult<SocialChallengeRunDto>> GetRun(Guid challengeId)
    {
        return await _queries.GetRunAsync(challengeId, User.GetUserId());
    }

    [HttpPost("{challengeId:guid}/run")]
    public async Task<ActionResult> Activate(Guid challengeId, SocialChallengePositionDto position)
    {
        await _service.ActivateAsync(challengeId, User.GetUserId(), position);
        return NoContent();
    }

    [HttpPost("{challengeId:guid}/run/location")]
    public async Task<ActionResult> ReportLocation(Guid challengeId, SocialChallengePositionDto position)
    {
        await _service.ReportLocationAsync(challengeId, User.GetUserId(), position);
        return NoContent();
    }
}
