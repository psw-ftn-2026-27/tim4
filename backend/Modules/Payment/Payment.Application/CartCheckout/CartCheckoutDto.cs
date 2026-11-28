namespace Payment.Application.CartCheckout;

public sealed record CartCheckoutDto(Guid Id, List<CartItemDto> Items, long Total, long AvailableBalance,
    CheckoutCouponDto? AppliedCoupon, long TotalToPay)
{
    public CheckoutGiftCardDto? AppliedGiftCard { get; init; }
    public AvailableGiftCardDto? AvailableGiftCard { get; init; }
}

public sealed record CheckoutCouponDto(Guid CouponId, string Code, Guid ProductId, int DiscountPercent, long Discount,
    bool IsValid, string? InvalidReason);

public sealed record CheckoutGiftCardDto(Guid GiftCardId, int Amount, long Discount, bool IsValid,
    string? InvalidReason);

public sealed record AvailableGiftCardDto(Guid GiftCardId, int Amount, DateOnly ValidUntil);
