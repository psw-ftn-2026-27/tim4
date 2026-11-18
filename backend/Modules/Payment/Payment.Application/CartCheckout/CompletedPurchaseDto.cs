namespace Payment.Application.CartCheckout;

public sealed record CompletedPurchaseDto(Guid Id, DateTime PurchasedAt, List<PurchasedItemDto> Items, long Total,
    PurchaseCouponDto? AppliedCoupon);

public sealed record PurchasedItemDto(Guid Id, Guid ProductId, Guid EntityId, string Name, long Price);

public sealed record PurchaseCouponDto(string Code, Guid ProductId, int DiscountPercent, long Discount);
