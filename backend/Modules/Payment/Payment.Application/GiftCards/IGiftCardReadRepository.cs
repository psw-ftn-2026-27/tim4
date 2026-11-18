using Payment.Application.CartCheckout;
using Payment.Application.PersonalGiftCards;
using Shared.Domain;

namespace Payment.Application.GiftCards;

public interface IGiftCardReadRepository
{
    Task<PageResult<GiftCardDto>> GetByGiverAsync(Guid giverId, DateOnly today, int page, int pageSize);
    Task<PageResult<GiftCardDto>> GetByRecipientAsync(Guid recipientId, DateOnly today, int page, int pageSize);
    Task<AvailableGiftCardDto?> GetFirstToExpireAsync(Guid recipientId, DateOnly today);
}
