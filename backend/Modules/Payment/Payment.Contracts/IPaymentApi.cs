namespace Payment.Contracts;

public interface IPaymentApi
{
    Task CreateProductAsync(CreateProductRequest request);
    Task UpdateProductPriceAsync(Guid entityId, Guid ownerId, long price);
    Task AddToCartAsync(Guid userId, Guid entityId);
    Task<bool> HasPurchasedAsync(Guid userId, Guid entityId);
    Task<List<Guid>> GetPurchasedEntityIdsAsync(Guid userId);
    // Each requested content ID has a result: null means no price is recorded; zero means free.
    Task<Dictionary<Guid, long?>> GetPricesByEntityIdsAsync(IReadOnlyCollection<Guid> entityIds);
}
