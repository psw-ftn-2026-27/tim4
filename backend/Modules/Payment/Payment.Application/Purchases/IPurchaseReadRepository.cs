using Payment.Application.ShoppingCart;

namespace Payment.Application.Purchases;

public interface IPurchaseReadRepository
{
    Task<CompletedPurchaseDto?> GetCompletedAsync(Guid userId, Guid purchaseId);
}
