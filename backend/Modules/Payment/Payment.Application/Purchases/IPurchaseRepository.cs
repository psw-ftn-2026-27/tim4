using Payment.Domain.Purchases;

namespace Payment.Application.Purchases;

public interface IPurchaseRepository
{
    Task<Purchase?> GetByCheckoutAsync(Guid userId, Guid checkoutId);
    Task<List<Purchase>> GetByContentAsync(Guid userId, IReadOnlyCollection<Guid> entityIds);
    void Add(Purchase purchase);
}
