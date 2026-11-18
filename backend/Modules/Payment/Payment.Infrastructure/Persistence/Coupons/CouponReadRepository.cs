using Microsoft.EntityFrameworkCore;
using Payment.Application.Coupons;
using Payment.Application.OwnerCoupons;

namespace Payment.Infrastructure.Persistence.Coupons;

internal sealed class CouponReadRepository : ICouponReadRepository
{
    private readonly PaymentDbContext _context;

    public CouponReadRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public Task<List<OwnerCouponDto>> GetByOwnerAsync(Guid ownerId, DateOnly today) => _context.Coupons
        .Where(coupon => coupon.OwnerId == ownerId)
        .OrderByDescending(coupon => coupon.ValidUntil).ThenBy(coupon => coupon.Code.Value)
        .Select(coupon => new OwnerCouponDto(
            coupon.Code.Value,
            coupon.DiscountPercent,
            coupon.ValidUntil,
            _context.Products.Where(product => product.Id == coupon.ProductId)
                .Select(product => product.Name).FirstOrDefault(),
            coupon.ValidUntil >= today))
        .ToListAsync();
}
