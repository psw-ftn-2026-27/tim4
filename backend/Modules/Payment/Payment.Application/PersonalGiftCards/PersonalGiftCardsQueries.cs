using Identity.Contracts;
using Payment.Application.GiftCards;

namespace Payment.Application.PersonalGiftCards;

public sealed class PersonalGiftCardsQueries
{
    private readonly IGiftCardReadRepository _giftCards;
    private readonly IIdentityApi _identityApi;

    public PersonalGiftCardsQueries(IGiftCardReadRepository giftCards, IIdentityApi identityApi)
    {
        _giftCards = giftCards;
        _identityApi = identityApi;
    }

    public async Task<MyGiftCardsDto> GetMyGiftCardsAsync(Guid userId)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var bought = await _giftCards.GetByGiverAsync(userId, today);
        var received = await _giftCards.GetByRecipientAsync(userId, today);
        var userIds = bought.Select(card => card.RecipientId).Concat(received.Select(card => card.GiverId))
            .Distinct().ToList();
        var emails = (await _identityApi.GetExplorersByIdsAsync(userIds))
            .ToDictionary(explorer => explorer.Id, explorer => explorer.Email);

        return new MyGiftCardsDto(
            bought.Select(card => card with { RecipientEmail = emails.GetValueOrDefault(card.RecipientId) }).ToList(),
            received.Select(card => card with { GiverEmail = emails.GetValueOrDefault(card.GiverId) }).ToList());
    }
}
