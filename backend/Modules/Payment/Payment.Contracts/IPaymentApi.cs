namespace Payment.Contracts;

public interface IPaymentApi
{
    Task CreateProductAsync(CreateProductRequest request);
    Task AddToCartAsync(Guid userId, Guid entityId);
    Task<bool> HasPurchasedAsync(Guid userId, Guid entityId);
    Task<List<Guid>> GetPurchasedEntityIdsAsync(Guid userId);
}
