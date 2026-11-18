using Payment.Application.Coupons;
using Payment.Application.Purchases;
using Payment.Application.ShoppingCarts;
using Shared.Domain.Exceptions;

namespace Payment.Application.CartCheckout;

public sealed class CartCheckoutQueries
{
    private readonly IShoppingCartReadRepository _carts;
    private readonly IPurchaseReadRepository _purchases;
    private readonly ICouponRepository _coupons;

    public CartCheckoutQueries(IShoppingCartReadRepository carts, IPurchaseReadRepository purchases,
        ICouponRepository coupons)
    {
        _carts = carts;
        _purchases = purchases;
        _coupons = coupons;
    }

    public async Task<CartDto> GetCartAsync(Guid userId) => await _carts.GetCartAsync(userId)
        ?? new CartDto(null, [], 0);

    public async Task<CartCheckoutDto> GetCartCheckoutAsync(Guid userId, Guid checkoutId)
    {
        var checkout = await _carts.GetCheckoutAsync(userId, checkoutId)
            ?? throw new NotFoundException("Pripremljena kupovina ne postoji. Osveži korpu.");
        if (checkout.AppliedCoupon is null) return checkout;

        var coupon = await _coupons.GetByIdAsync(checkout.AppliedCoupon.CouponId);
        if (coupon is not null && coupon.IsValidOn(DateOnly.FromDateTime(DateTime.UtcNow))) return checkout;

        var reason = coupon is null ? "Kupon više ne postoji." : "Kupon je istekao.";
        return checkout with
        {
            AppliedCoupon = checkout.AppliedCoupon with { IsValid = false, InvalidReason = reason },
            TotalToPay = checkout.Total
        };
    }

    public async Task<CompletedPurchaseDto> GetCompletedPurchaseAsync(Guid userId, Guid purchaseId) =>
        await _purchases.GetCompletedAsync(userId, purchaseId)
        ?? throw new NotFoundException("Kupovina ne postoji.");
}
