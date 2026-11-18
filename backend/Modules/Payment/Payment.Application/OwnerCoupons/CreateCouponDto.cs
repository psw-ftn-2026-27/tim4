namespace Payment.Application.OwnerCoupons;

public sealed record CreateCouponDto(int DiscountPercent, DateOnly ValidUntil, Guid? ProductId);
