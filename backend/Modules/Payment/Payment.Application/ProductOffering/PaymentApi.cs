using Payment.Contracts;

namespace Payment.Application.ProductOffering;

public sealed class PaymentApi : IPaymentApi
{
    private readonly ProductOfferingService _productOffering;

    public PaymentApi(ProductOfferingService productOffering)
    {
        _productOffering = productOffering;
    }

    public Task CreateProductAsync(CreateProductRequest request) => _productOffering.CreateProductAsync(
        new CreateProductDto(request.EntityId, request.Name, request.OwnerId, request.Price));
}
