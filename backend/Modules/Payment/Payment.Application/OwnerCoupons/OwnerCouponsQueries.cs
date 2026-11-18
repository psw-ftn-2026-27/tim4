using Payment.Application.Coupons;
using Payment.Application.Products;
using Shared.Domain;

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

    public Task<PageResult<OwnerCouponDto>> GetMyCouponsAsync(Guid ownerId, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _coupons.GetByOwnerAsync(ownerId, DateOnly.FromDateTime(DateTime.UtcNow), page, pageSize);
    }

    public Task<List<OwnerProductDto>> GetMyProductsAsync(Guid ownerId) => _products.GetByOwnerAsync(ownerId);
}
