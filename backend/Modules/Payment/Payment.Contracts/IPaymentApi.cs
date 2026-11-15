namespace Payment.Contracts;

public interface IPaymentApi
{
    Task CreateProductAsync(CreateProductRequest request);
    Task AddToCartAsync(Guid userId, Guid entityId);
    Task<bool> HasPurchasedAsync(Guid userId, Guid entityId);
    Task<List<Guid>> GetPurchasedEntityIdsAsync(Guid userId);
    // Each requested content ID has a result: null means no price is recorded; zero means free.
    Task<Dictionary<Guid, decimal?>> GetPricesByEntityIdsAsync(IReadOnlyCollection<Guid> entityIds);
}
