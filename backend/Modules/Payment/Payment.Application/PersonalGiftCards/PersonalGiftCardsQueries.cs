using Identity.Contracts;
using Payment.Application.GiftCards;
using Shared.Domain;

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

    public async Task<PageResult<GiftCardDto>> GetBoughtAsync(Guid giverId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var bought = await _giftCards.GetByGiverAsync(giverId, Today(), page, pageSize);
        var emails = await GetEmailsAsync(bought.Items.Select(card => card.RecipientId));

        return new PageResult<GiftCardDto>(
            bought.Items.Select(card => card with { RecipientEmail = emails.GetValueOrDefault(card.RecipientId) })
                .ToList(),
            bought.TotalCount);
    }

    public async Task<PageResult<GiftCardDto>> GetReceivedAsync(Guid recipientId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var received = await _giftCards.GetByRecipientAsync(recipientId, Today(), page, pageSize);
        var emails = await GetEmailsAsync(received.Items.Select(card => card.GiverId));

        return new PageResult<GiftCardDto>(
            received.Items.Select(card => card with { GiverEmail = emails.GetValueOrDefault(card.GiverId) }).ToList(),
            received.TotalCount);
    }

    private async Task<Dictionary<Guid, string>> GetEmailsAsync(IEnumerable<Guid> userIds) =>
        (await _identityApi.GetExplorersByIdsAsync(userIds.Distinct().ToList()))
        .ToDictionary(explorer => explorer.Id, explorer => explorer.Email);

    private static DateOnly Today() => DateOnly.FromDateTime(DateTime.UtcNow);
}
