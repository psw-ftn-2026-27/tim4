namespace Payment.Domain.Coupons;

public sealed record AppliedCoupon
{
    public Guid CouponId { get; private init; }
    public string Code { get; private init; }
    public Guid ProductId { get; private init; }
    public int DiscountPercent { get; private init; }
    public long Discount { get; private init; }

    private AppliedCoupon()
    {
        Code = null!;
    }

    internal AppliedCoupon(Coupon coupon, Guid productId, long discount)
    {
        CouponId = coupon.Id;
        Code = coupon.Code.Value;
        ProductId = productId;
        DiscountPercent = coupon.DiscountPercent;
        Discount = discount;
    }
}
