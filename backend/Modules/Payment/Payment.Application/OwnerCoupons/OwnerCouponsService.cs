using Payment.Application.Coupons;
using Payment.Application.Products;
using Payment.Domain.Coupons;

namespace Payment.Application.OwnerCoupons;

public sealed class OwnerCouponsService
{
    private readonly ICouponRepository _coupons;
    private readonly IProductRepository _products;
    private readonly CouponIssuing _couponIssuing;
    private readonly IUnitOfWork _unitOfWork;

    public OwnerCouponsService(ICouponRepository coupons, IProductRepository products,
        CouponIssuing couponIssuing, IUnitOfWork unitOfWork)
    {
        _coupons = coupons;
        _products = products;
        _couponIssuing = couponIssuing;
        _unitOfWork = unitOfWork;
    }

    public async Task CreateCouponAsync(Guid ownerId, CreateCouponDto dto)
    {
        var ownerProducts = await _products.GetByOwnerAsync(ownerId);
        var coupon = _couponIssuing.Issue(ownerId, ownerProducts, dto.ProductId, dto.DiscountPercent,
            dto.ValidUntil, DateTime.UtcNow);
        _coupons.Add(coupon);
        await _unitOfWork.SaveChangesAsync();
    }
}
