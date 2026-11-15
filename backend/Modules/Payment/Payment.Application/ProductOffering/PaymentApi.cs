using Payment.Contracts;
using Payment.Application.ShoppingCart;
using Payment.Application.PurchaseAccess;
using Payment.Application.ProductPricing;

namespace Payment.Application.ProductOffering;

public sealed class PaymentApi : IPaymentApi
{
    private readonly ProductOfferingService _productOffering;
    private readonly ShoppingCartService _shoppingCart;
    private readonly PurchaseAccessQueries _purchaseAccess;
    private readonly ProductPricingQueries _productPricing;

    public PaymentApi(ProductOfferingService productOffering, ShoppingCartService shoppingCart,
        PurchaseAccessQueries purchaseAccess, ProductPricingQueries productPricing)
    {
        _productOffering = productOffering;
        _shoppingCart = shoppingCart;
        _purchaseAccess = purchaseAccess;
        _productPricing = productPricing;
    }

    public Task CreateProductAsync(CreateProductRequest request) => _productOffering.CreateProductAsync(
        new CreateProductDto(request.EntityId, request.Name, request.OwnerId, request.Price));

    public Task AddToCartAsync(Guid userId, Guid entityId) =>
        _shoppingCart.AddToCartAsync(userId, new AddToCartDto(entityId));

    public Task<bool> HasPurchasedAsync(Guid userId, Guid entityId) =>
        _purchaseAccess.HasPurchasedAsync(userId, entityId);

    public Task<List<Guid>> GetPurchasedEntityIdsAsync(Guid userId) =>
        _purchaseAccess.GetPurchasedEntityIdsAsync(userId);

    public Task<Dictionary<Guid, decimal?>> GetPricesByEntityIdsAsync(IReadOnlyCollection<Guid> entityIds) =>
        _productPricing.GetPricesByEntityIdsAsync(entityIds);
}
