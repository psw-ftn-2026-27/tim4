using Payment.Application.Purchases;

namespace Payment.Application.PurchaseAccess;

public sealed class PurchaseAccessQueries
{
    private readonly IPurchaseReadRepository _purchases;

    public PurchaseAccessQueries(IPurchaseReadRepository purchases)
    {
        _purchases = purchases;
    }

    public Task<bool> HasPurchasedAsync(Guid userId, Guid entityId) =>
        _purchases.HasPurchasedAsync(userId, entityId);

    public Task<List<Guid>> GetPurchasedEntityIdsAsync(Guid userId) =>
        _purchases.GetPurchasedEntityIdsAsync(userId);
}
