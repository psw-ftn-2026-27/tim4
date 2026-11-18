using Payment.Domain.GiftCards;

namespace Payment.Application.GiftCards;

public interface IGiftCardRepository
{
    Task<GiftCard?> GetByIdAsync(Guid id);
    Task<List<GiftCard>> GetByRecipientAsync(Guid recipientId);
    void Add(GiftCard giftCard);
}
