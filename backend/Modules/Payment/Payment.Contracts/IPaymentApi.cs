namespace Payment.Contracts;

public interface IPaymentApi
{
    Task CreateProductAsync(CreateProductRequest request);
    Task AddToCartAsync(Guid userId, Guid entityId);
}
