using Payment.Application.PersonalGiftCards;

namespace Payment.Application.GiftCards;

public interface IGiftCardReadRepository
{
    Task<List<GiftCardDto>> GetByGiverAsync(Guid giverId, DateOnly today);
    Task<List<GiftCardDto>> GetByRecipientAsync(Guid recipientId, DateOnly today);
}
