using Exploration.Application.KeyPointAuthoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tours")]
[Authorize(Roles = "explorer")]
public sealed class KeyPointAuthoringController : ControllerBase
{
    private readonly KeyPointAuthoringService _keyPointService;
    private readonly KeyPointAuthoringQueries _keyPointQueries;

    public KeyPointAuthoringController(
        KeyPointAuthoringService keyPointService,
        KeyPointAuthoringQueries keyPointQueries)
    {
        _keyPointService = keyPointService;
        _keyPointQueries = keyPointQueries;
    }

    [HttpPost("{tourId:guid}/key-points")]
    public async Task<ActionResult> Add(Guid tourId, [FromForm] KeyPointDetailsDto dto, IFormFile? image)
    {
        await _keyPointService.AddAsync(tourId, User.GetUserId(), dto, await ReadBytesAsync(image));
        return NoContent();
    }

    [HttpPut("{tourId:guid}/key-points/{keyPointId:guid}")]
    public async Task<ActionResult> Update(Guid tourId, Guid keyPointId, [FromForm] KeyPointDetailsDto dto, IFormFile? image)
    {
        await _keyPointService.UpdateAsync(tourId, keyPointId, User.GetUserId(), dto, await ReadBytesAsync(image));
        return NoContent();
    }

    [HttpDelete("{tourId:guid}/key-points/{keyPointId:guid}")]
    public async Task<ActionResult> Remove(Guid tourId, Guid keyPointId, [FromQuery] double tourLengthInKm)
    {
        await _keyPointService.RemoveAsync(tourId, keyPointId, User.GetUserId(), tourLengthInKm);
        return NoContent();
    }

    [HttpGet("mine/{tourId:guid}/key-points")]
    public async Task<ActionResult<List<KeyPointDto>>> GetByTour(Guid tourId)
    {
        return await _keyPointQueries.GetByTourAsync(tourId, User.GetUserId());
    }

    private static async Task<byte[]?> ReadBytesAsync(IFormFile? file)
    {
        if (file is null)
        {
            return null;
        }

        using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        return stream.ToArray();
    }
}
