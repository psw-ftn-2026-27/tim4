using Exploration.Application.TourBrowsing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Api;
using Shared.Domain;

namespace Exploration.Api;

[ApiController]
[Route("api/exploration/tours")]
[Authorize]
public sealed class TourBrowsingController : ControllerBase
{
    private readonly TourBrowsingService _browsingService;
    private readonly TourBrowsingQueries _browsingQueries;

    public TourBrowsingController(
        TourBrowsingService browsingService,
        TourBrowsingQueries browsingQueries)
    {
        _browsingService = browsingService;
        _browsingQueries = browsingQueries;
    }

    [HttpPost("{id:guid}/cart")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult> AddToCart(Guid id)
    {
        await _browsingService.AddToCartAsync(User.GetUserId(), id);
        return NoContent();
    }

    [HttpGet("published")]
    public async Task<ActionResult<PageResult<PublishedTourDto>>> GetPublished(
        [FromQuery] PublishedTourSearchDto search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        return await _browsingQueries.GetPublishedAsync(search, User.GetUserId(), page, pageSize);
    }

    [HttpGet("purchased")]
    [Authorize(Roles = "explorer")]
    public async Task<ActionResult<List<PurchasedTourDto>>> GetPurchased()
    {
        return await _browsingQueries.GetPurchasedAsync(User.GetUserId());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TourDetailsDto>> GetDetails(Guid id)
    {
        return await _browsingQueries.GetDetailsAsync(id, User.GetUserId());
    }
}
