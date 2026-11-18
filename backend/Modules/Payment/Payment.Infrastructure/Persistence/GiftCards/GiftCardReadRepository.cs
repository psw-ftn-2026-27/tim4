using Microsoft.EntityFrameworkCore;
using Payment.Application.CartCheckout;
using Payment.Application.GiftCards;
using Payment.Application.PersonalGiftCards;
using Payment.Domain.GiftCards;
using Shared.Domain;

namespace Payment.Infrastructure.Persistence.GiftCards;

internal sealed class GiftCardReadRepository : IGiftCardReadRepository
{
    private readonly PaymentDbContext _context;

    public GiftCardReadRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public Task<PageResult<GiftCardDto>> GetByGiverAsync(Guid giverId, DateOnly today, int page, int pageSize) =>
        GetPageAsync(_context.GiftCards.Where(giftCard => giftCard.GiverId == giverId), today, page, pageSize);

    public Task<PageResult<GiftCardDto>> GetByRecipientAsync(Guid recipientId, DateOnly today, int page,
        int pageSize) =>
        GetPageAsync(_context.GiftCards.Where(giftCard => giftCard.RecipientId == recipientId), today, page,
            pageSize);

    // The same choice as GiftCard.FirstToExpire, made in the database: a usable card of the recipient that is
    // not redeemed and has not expired, the one that expires first.
    public Task<AvailableGiftCardDto?> GetFirstToExpireAsync(Guid recipientId, DateOnly today) => _context.GiftCards
        .Where(giftCard => giftCard.RecipientId == recipientId && giftCard.RedeemedInPurchaseId == null
            && giftCard.ValidUntil >= today)
        .OrderBy(giftCard => giftCard.ValidUntil).ThenBy(giftCard => giftCard.CreatedAt).ThenBy(giftCard => giftCard.Id)
        .Select(giftCard => new AvailableGiftCardDto(giftCard.Id, giftCard.Amount, giftCard.ValidUntil))
        .FirstOrDefaultAsync();

    private static async Task<PageResult<GiftCardDto>> GetPageAsync(IQueryable<GiftCard> giftCards, DateOnly today,
        int page, int pageSize)
    {
        var totalCount = await giftCards.CountAsync();
        var items = await giftCards
            .OrderByDescending(giftCard => giftCard.CreatedAt).ThenBy(giftCard => giftCard.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(giftCard => new GiftCardDto(giftCard.Id, giftCard.GiverId, null, giftCard.RecipientId, null,
                giftCard.Amount, giftCard.Message, giftCard.CreatedAt, giftCard.ValidUntil,
                GiftCard.GetStatus(giftCard.RedeemedInPurchaseId, giftCard.ValidUntil, today)))
            .ToListAsync();

        return new PageResult<GiftCardDto>(items, totalCount);
    }
}
