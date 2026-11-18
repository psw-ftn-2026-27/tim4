using Payment.Application.Coupons;
using Payment.Domain.Coupons;

namespace Payment.Infrastructure.Persistence.Coupons;

internal sealed class CouponRepository : ICouponRepository
{
    private readonly PaymentDbContext _context;

    public CouponRepository(PaymentDbContext context)
    {
        _context = context;
    }

    public void Add(Coupon coupon) => _context.Coupons.Add(coupon);
}
