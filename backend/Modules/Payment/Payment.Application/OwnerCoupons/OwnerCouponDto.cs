namespace Payment.Application.OwnerCoupons;

public sealed record OwnerCouponDto(string Code, int DiscountPercent, DateOnly ValidUntil, string? ProductName, bool IsValid);
