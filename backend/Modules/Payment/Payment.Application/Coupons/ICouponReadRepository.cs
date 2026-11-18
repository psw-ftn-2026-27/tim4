using Payment.Application.OwnerCoupons;
using Shared.Domain;

namespace Payment.Application.Coupons;

public interface ICouponReadRepository
{
    Task<PageResult<OwnerCouponDto>> GetByOwnerAsync(Guid ownerId, DateOnly today, int page, int pageSize);
}
