namespace Payment.Contracts;

public interface IPaymentApi
{
    Task CreateProductAsync(CreateProductRequest request);
}
