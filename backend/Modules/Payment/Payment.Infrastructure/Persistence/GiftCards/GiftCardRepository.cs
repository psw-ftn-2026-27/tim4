using Microsoft.EntityFrameworkCore;
using Payment.Application.GiftCards;
using Payment.Domain.GiftCards;

namespace Payment.Infrastructure.Persistence.GiftCards;

internal sealed class GiftCardRepository : IGiftCardRepository
{
    private readonly PaymentDbContext _context;

    public GiftCardRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public Task<GiftCard?> GetByIdAsync(Guid id) =>
        _context.GiftCards.SingleOrDefaultAsync(giftCard => giftCard.Id == id);

    public Task<List<GiftCard>> GetUnredeemedByRecipientAsync(Guid recipientId, DateOnly today) => _context.GiftCards
        .Where(giftCard => giftCard.RecipientId == recipientId && giftCard.RedeemedInPurchaseId == null
            && giftCard.ValidUntil >= today)
        .ToListAsync();

    public void Add(GiftCard giftCard) => _context.GiftCards.Add(giftCard);
}
