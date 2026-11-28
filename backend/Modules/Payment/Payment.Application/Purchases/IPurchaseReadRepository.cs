using Payment.Application.CartCheckout;

namespace Payment.Application.Purchases;

public interface IPurchaseReadRepository
{
    Task<CompletedPurchaseDto?> GetCompletedAsync(Guid userId, Guid purchaseId);
    Task<bool> HasPurchasedAsync(Guid userId, Guid entityId);
    Task<List<Guid>> GetPurchasedEntityIdsAsync(Guid userId);
}
