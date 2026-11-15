using Payment.Contracts;
using Payment.Application.ShoppingCart;

namespace Payment.Application.ProductOffering;

public sealed class PaymentApi : IPaymentApi
{
    private readonly ProductOfferingService _productOffering;
    private readonly ShoppingCartService _shoppingCart;

    public PaymentApi(ProductOfferingService productOffering, ShoppingCartService shoppingCart)
    {
        _productOffering = productOffering;
        _shoppingCart = shoppingCart;
    }

    public Task CreateProductAsync(CreateProductRequest request) => _productOffering.CreateProductAsync(
        new CreateProductDto(request.EntityId, request.Name, request.OwnerId, request.Price));

    public Task AddToCartAsync(Guid userId, Guid entityId) =>
        _shoppingCart.AddToCartAsync(userId, new AddToCartDto(entityId));
}
