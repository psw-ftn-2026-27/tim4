using Microsoft.EntityFrameworkCore;
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

    public Task<Coupon?> GetByIdAsync(Guid id) => _context.Coupons.SingleOrDefaultAsync(coupon => coupon.Id == id);

    public Task<Coupon?> GetByCodeAsync(string code) =>
        _context.Coupons.SingleOrDefaultAsync(coupon => coupon.Code.Value == code);

    public void Add(Coupon coupon) => _context.Coupons.Add(coupon);
}
