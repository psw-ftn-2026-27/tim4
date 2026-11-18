using Payment.Application.Coupons;
using Payment.Application.Products;

namespace Payment.Application.OwnerCoupons;

public sealed class OwnerCouponsQueries
{
    private readonly ICouponReadRepository _coupons;
    private readonly IProductReadRepository _products;

    public OwnerCouponsQueries(ICouponReadRepository coupons, IProductReadRepository products)
    {
        _coupons = coupons;
        _products = products;
    }

    public Task<List<OwnerCouponDto>> GetMyCouponsAsync(Guid ownerId) =>
        _coupons.GetByOwnerAsync(ownerId, DateOnly.FromDateTime(DateTime.UtcNow));

    public Task<List<OwnerProductDto>> GetMyProductsAsync(Guid ownerId) => _products.GetByOwnerAsync(ownerId);
}
