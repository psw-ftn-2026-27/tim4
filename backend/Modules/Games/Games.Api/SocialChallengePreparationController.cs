using Games.Application.SocialChallengePreparation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Domain;

namespace Games.Api;

[ApiController]
[Authorize(Roles = "administrator")]
[Route("api/games/social-challenges")]
public sealed class SocialChallengePreparationController : ControllerBase
{
    private readonly SocialChallengePreparationService _service;
    private readonly SocialChallengePreparationQueries _queries;

    public SocialChallengePreparationController(SocialChallengePreparationService service,
        SocialChallengePreparationQueries queries)
    {
        _service = service;
        _queries = queries;
    }

    [HttpGet]
    public async Task<ActionResult<PageResult<SocialChallengeDto>>> GetAll([FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return Ok(await _queries.GetAllAsync(page, pageSize));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SocialChallengeDto>> GetById(Guid id)
    {
        return Ok(await _queries.GetByIdAsync(id));
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(SocialChallengeDetailsDto details)
    {
        var id = await _service.CreateAsync(details);
        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<object>> Update(Guid id, SocialChallengeDetailsDto details)
    {
        await _service.UpdateAsync(id, details);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<object>> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
