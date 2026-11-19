using Games.Application.LocationChallengeAuthoring;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Games.Api;

[ApiController]
[Authorize(Roles = "administrator")]
[Route("api/games/location-challenges")]
public sealed class LocationChallengeAuthoringController : ControllerBase
{
    private readonly LocationChallengeAuthoringService _service;
    private readonly LocationChallengeAuthoringQueries _queries;

    public LocationChallengeAuthoringController(
        LocationChallengeAuthoringService service, LocationChallengeAuthoringQueries queries)
    {
        _service = service;
        _queries = queries;
    }
    [HttpGet("{id:guid}/admin")]
    public async Task<ActionResult<LocationChallengeDetailsDto>> GetDetails(Guid id)
    {
        return await _queries.GetDetailsAsync(id);
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create([FromForm] LocationChallengeForm form)
    {
        var points = new List<HiddenLocationContentDto>();
        foreach (var point in form.Points ?? [])
        {
            using var buffer = new MemoryStream();
            if (point.Image is not null)
            {
                await point.Image.CopyToAsync(buffer);
            }
            points.Add(new HiddenLocationContentDto(point.Latitude, point.Longitude, buffer.ToArray()));
        }

        var id = await _service.CreateAsync(new LocationChallengeContentDto(
            form.Name, form.StartLatitude, form.StartLongitude, form.XpReward, points));
        return CreatedAtAction(nameof(GetDetails), new { id }, id);
    }

    public sealed record LocationChallengeForm(
        string Name, [BindRequired] double StartLatitude, [BindRequired] double StartLongitude,
        int XpReward, List<HiddenLocationForm>? Points);

    public sealed record HiddenLocationForm(double? Latitude, double? Longitude, IFormFile? Image);
}
