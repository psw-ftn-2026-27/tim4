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

    public Task<List<GiftCard>> GetByRecipientAsync(Guid recipientId) =>
        _context.GiftCards.Where(giftCard => giftCard.RecipientId == recipientId).ToListAsync();

    public void Add(GiftCard giftCard) => _context.GiftCards.Add(giftCard);
}
