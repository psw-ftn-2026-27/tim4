using Payment.Application.OwnerCoupons;

namespace Payment.Application.Coupons;

public interface ICouponReadRepository
{
    Task<List<OwnerCouponDto>> GetByOwnerAsync(Guid ownerId, DateOnly today);
}
