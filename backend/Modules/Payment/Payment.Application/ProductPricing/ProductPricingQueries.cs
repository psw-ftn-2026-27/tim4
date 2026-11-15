using Payment.Application.Products;

namespace Payment.Application.ProductPricing;

public sealed class ProductPricingQueries
{
    private readonly IProductReadRepository _products;

    public ProductPricingQueries(IProductReadRepository products)
    {
        _products = products;
    }

    public Task<Dictionary<Guid, long?>> GetPricesByEntityIdsAsync(IReadOnlyCollection<Guid> entityIds) =>
        _products.GetPricesByEntityIdsAsync(entityIds);
}
