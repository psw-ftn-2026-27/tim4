using Payment.Domain.GiftCards;

namespace Payment.Application.GiftCards;

public interface IGiftCardRepository
{
    Task<GiftCard?> GetByIdAsync(Guid id);
    Task<List<GiftCard>> GetUnredeemedByRecipientAsync(Guid recipientId, DateOnly today);
    void Add(GiftCard giftCard);
}
