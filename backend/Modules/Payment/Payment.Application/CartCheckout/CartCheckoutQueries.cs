using Payment.Application.Coupons;
using Payment.Application.GiftCards;
using Payment.Application.Purchases;
using Payment.Application.ShoppingCarts;
using Payment.Domain.GiftCards;
using Shared.Domain.Exceptions;

namespace Payment.Application.CartCheckout;

public sealed class CartCheckoutQueries
{
    private readonly IShoppingCartReadRepository _carts;
    private readonly IPurchaseReadRepository _purchases;
    private readonly ICouponRepository _coupons;
    private readonly IGiftCardRepository _giftCards;
    private readonly IGiftCardReadRepository _giftCardReads;

    public CartCheckoutQueries(IShoppingCartReadRepository carts, IPurchaseReadRepository purchases,
        ICouponRepository coupons, IGiftCardRepository giftCards, IGiftCardReadRepository giftCardReads)
    {
        _carts = carts;
        _purchases = purchases;
        _coupons = coupons;
        _giftCards = giftCards;
        _giftCardReads = giftCardReads;
    }

    public async Task<CartDto> GetCartAsync(Guid userId) => await _carts.GetCartAsync(userId)
        ?? new CartDto(null, [], 0);

    public async Task<CartCheckoutDto> GetCartCheckoutAsync(Guid userId, Guid checkoutId)
    {
        var checkout = await _carts.GetCheckoutAsync(userId, checkoutId)
            ?? throw new NotFoundException("Pripremljena kupovina ne postoji. Osveži korpu.");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        checkout = await CheckAppliedCouponAsync(checkout, today);
        var totalAfterCoupon = checkout.Total - (checkout.AppliedCoupon is { IsValid: true } coupon ? coupon.Discount : 0);
        return checkout.AppliedGiftCard is null
            ? checkout with
            {
                TotalToPay = totalAfterCoupon,
                AvailableGiftCard = await _giftCardReads.GetFirstToExpireAsync(userId, today)
            }
            : await CheckAppliedGiftCardAsync(userId, checkout, totalAfterCoupon, today);
    }

    public async Task<CompletedPurchaseDto> GetCompletedPurchaseAsync(Guid userId, Guid purchaseId) =>
        await _purchases.GetCompletedAsync(userId, purchaseId)
        ?? throw new NotFoundException("Kupovina ne postoji.");

    private async Task<CartCheckoutDto> CheckAppliedCouponAsync(CartCheckoutDto checkout, DateOnly today)
    {
        if (checkout.AppliedCoupon is null) return checkout;

        var coupon = await _coupons.GetByIdAsync(checkout.AppliedCoupon.CouponId);
        if (coupon is not null && coupon.IsValidOn(today)) return checkout;

        var reason = coupon is null ? "Kupon više ne postoji." : "Kupon je istekao.";
        return checkout with { AppliedCoupon = checkout.AppliedCoupon with { IsValid = false, InvalidReason = reason } };
    }

    private async Task<CartCheckoutDto> CheckAppliedGiftCardAsync(Guid userId, CartCheckoutDto checkout,
        long totalAfterCoupon, DateOnly today)
    {
        var applied = checkout.AppliedGiftCard!;
        var giftCard = await _giftCards.GetByIdAsync(applied.GiftCardId);
        var status = giftCard?.GetStatus(today);
        if (giftCard is not null && giftCard.IsUsableBy(userId, today))
        {
            var discount = AppliedGiftCard.DiscountOn(applied.Amount, totalAfterCoupon);
            return checkout with
            {
                AppliedGiftCard = applied with { Discount = discount },
                TotalToPay = totalAfterCoupon - discount
            };
        }

        var reason = status switch
        {
            GiftCardStatus.Expired => "Poklon kartica je istekla.",
            GiftCardStatus.Redeemed => "Poklon kartica je već iskorišćena.",
            _ => "Poklon kartica više ne važi."
        };
        return checkout with
        {
            AppliedGiftCard = applied with { Discount = 0, IsValid = false, InvalidReason = reason },
            TotalToPay = totalAfterCoupon
        };
    }
}
