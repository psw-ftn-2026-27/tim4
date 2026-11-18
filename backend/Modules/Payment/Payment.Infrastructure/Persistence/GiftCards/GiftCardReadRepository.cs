using Microsoft.EntityFrameworkCore;
using Payment.Application.GiftCards;
using Payment.Application.PersonalGiftCards;
using Payment.Domain.GiftCards;

namespace Payment.Infrastructure.Persistence.GiftCards;

internal sealed class GiftCardReadRepository : IGiftCardReadRepository
{
    private readonly PaymentDbContext _context;

    public GiftCardReadRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public Task<List<GiftCardDto>> GetByGiverAsync(Guid giverId, DateOnly today) =>
        Project(_context.GiftCards.Where(giftCard => giftCard.GiverId == giverId), today);

    public Task<List<GiftCardDto>> GetByRecipientAsync(Guid recipientId, DateOnly today) =>
        Project(_context.GiftCards.Where(giftCard => giftCard.RecipientId == recipientId), today);

    private static Task<List<GiftCardDto>> Project(IQueryable<GiftCard> giftCards, DateOnly today) => giftCards
        .OrderByDescending(giftCard => giftCard.CreatedAt).ThenBy(giftCard => giftCard.Id)
        .Select(giftCard => new GiftCardDto(giftCard.Id, giftCard.GiverId, null, giftCard.RecipientId, null,
            giftCard.Amount, giftCard.Message, giftCard.CreatedAt, giftCard.ValidUntil,
            GiftCard.GetStatus(giftCard.RedeemedInPurchaseId, giftCard.ValidUntil, today)))
        .ToListAsync();
}
