using Microsoft.EntityFrameworkCore;
using Payment.Application.Coupons;
using Payment.Application.OwnerCoupons;
using Shared.Domain;

namespace Payment.Infrastructure.Persistence.Coupons;

internal sealed class CouponReadRepository : ICouponReadRepository
{
    private readonly PaymentDbContext _context;

    public CouponReadRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public async Task<PageResult<OwnerCouponDto>> GetByOwnerAsync(Guid ownerId, DateOnly today, int page, int pageSize)
    {
        var coupons = _context.Coupons.Where(coupon => coupon.OwnerId == ownerId);
        var totalCount = await coupons.CountAsync();
        var items = await coupons
            .OrderByDescending(coupon => coupon.CreatedAt).ThenBy(coupon => coupon.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(coupon => new OwnerCouponDto(
                coupon.Code.Value,
                coupon.DiscountPercent,
                coupon.ValidUntil,
                _context.Products.Where(product => product.Id == coupon.ProductId)
                    .Select(product => product.Name).FirstOrDefault(),
                coupon.ValidUntil >= today))
            .ToListAsync();

        return new PageResult<OwnerCouponDto>(items, totalCount);
    }
}
