using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.PersonalGiftCards;
using Shared.Api;
using Shared.Domain;

namespace Payment.Api;

[ApiController]
[Route("api/payment/gift-cards")]
[Authorize(Roles = "explorer")]
public sealed class PersonalGiftCardsController : ControllerBase
{
    private readonly PersonalGiftCardsService _service;
    private readonly PersonalGiftCardsQueries _queries;

    public PersonalGiftCardsController(PersonalGiftCardsService service, PersonalGiftCardsQueries queries)
    {
        _service = service;
        _queries = queries;
    }

    [HttpGet("bought")]
    public async Task<ActionResult<PageResult<GiftCardDto>>> GetBought(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        await _queries.GetBoughtAsync(User.GetUserId(), page, pageSize);

    [HttpGet("received")]
    public async Task<ActionResult<PageResult<GiftCardDto>>> GetReceived(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20) =>
        await _queries.GetReceivedAsync(User.GetUserId(), page, pageSize);

    [HttpPost]
    public async Task<ActionResult<Guid>> BuyGiftCard(BuyGiftCardDto dto) =>
        await _service.BuyGiftCardAsync(User.GetUserId(), dto);
}
