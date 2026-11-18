using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Payment.Application.PersonalGiftCards;
using Shared.Api;

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

    [HttpGet]
    public async Task<ActionResult<MyGiftCardsDto>> GetMyGiftCards() =>
        await _queries.GetMyGiftCardsAsync(User.GetUserId());

    [HttpPost]
    public async Task<ActionResult<Guid>> BuyGiftCard(BuyGiftCardDto dto) =>
        await _service.BuyGiftCardAsync(User.GetUserId(), dto);
}
