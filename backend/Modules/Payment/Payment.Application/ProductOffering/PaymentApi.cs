using Payment.Contracts;
using Payment.Application.ShoppingCart;
using Payment.Application.PurchaseAccess;

namespace Payment.Application.ProductOffering;

public sealed class PaymentApi : IPaymentApi
{
    private readonly ProductOfferingService _productOffering;
    private readonly ShoppingCartService _shoppingCart;
    private readonly PurchaseAccessQueries _purchaseAccess;

    public PaymentApi(ProductOfferingService productOffering, ShoppingCartService shoppingCart,
        PurchaseAccessQueries purchaseAccess)
    {
        _productOffering = productOffering;
        _shoppingCart = shoppingCart;
        _purchaseAccess = purchaseAccess;
    }

    public Task CreateProductAsync(CreateProductRequest request) => _productOffering.CreateProductAsync(
        new CreateProductDto(request.EntityId, request.Name, request.OwnerId, request.Price));

    public Task AddToCartAsync(Guid userId, Guid entityId) =>
        _shoppingCart.AddToCartAsync(userId, new AddToCartDto(entityId));

    public Task<bool> HasPurchasedAsync(Guid userId, Guid entityId) =>
        _purchaseAccess.HasPurchasedAsync(userId, entityId);

    public Task<List<Guid>> GetPurchasedEntityIdsAsync(Guid userId) =>
        _purchaseAccess.GetPurchasedEntityIdsAsync(userId);
}
