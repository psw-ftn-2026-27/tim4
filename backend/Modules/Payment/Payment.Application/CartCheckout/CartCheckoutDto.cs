namespace Payment.Application.CartCheckout;

public sealed record CartCheckoutDto(Guid Id, List<CartItemDto> Items, long Total, long AvailableBalance,
    CheckoutCouponDto? AppliedCoupon, long TotalToPay);

public sealed record CheckoutCouponDto(Guid CouponId, string Code, Guid ProductId, int DiscountPercent, long Discount,
    bool IsValid, string? InvalidReason);
