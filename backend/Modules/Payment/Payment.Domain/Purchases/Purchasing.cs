using Payment.Domain.Coupons;
using Payment.Domain.Products;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Purchases;

public sealed class Purchasing
{
    public void AddToCart(ShoppingCart cart, Product product, IReadOnlyCollection<Purchase> existingPurchases)
    {
        EnsureNotOwned(cart.UserId, [product.EntityId], existingPurchases);
        cart.Add(product);
    }

    public void EnsureNotCompleted(Purchase? completed)
    {
        if (completed is not null) throw new DomainException("Ova kupovina je već završena.");
    }

    public void ApplyCoupon(ShoppingCart cart, Guid checkoutId, Coupon coupon,
        IReadOnlyCollection<Purchase> existingPurchases, DateOnly today)
    {
        EnsureCouponUsable(cart.UserId, coupon, existingPurchases, today);
        var item = coupon.AppliesTo(cart.Items);
        cart.ApplyCoupon(checkoutId, new AppliedCoupon(coupon, item.ProductId, coupon.DiscountFor(item.Price)));
    }

    public Purchase Complete(ShoppingCart cart, Wallet wallet, Guid checkoutId, Coupon? coupon, long seenTotal,
        IReadOnlyCollection<Purchase> existingPurchases, DateTime now)
    {
        cart.EnsureCheckout(checkoutId);
        EnsureNotOwned(cart.UserId, cart.Items.Select(item => item.EntityId), existingPurchases);
        if (cart.UserId != wallet.UserId) throw new DomainException("Novčanik mora da pripada vlasniku korpe.");
        EnsureAppliedCouponUsable(cart, coupon, existingPurchases, DateOnly.FromDateTime(now));
        cart.EnsureTotalToPay(seenTotal);
        var purchase = new Purchase(cart.UserId, checkoutId, cart.Items, cart.AppliedCoupon, now);
        wallet.Pay(purchase.Id, purchase.Items.Select(item => new PurchasePayment(item.EntityId, item.Price)).ToList(), now);
        cart.CompleteCheckout(checkoutId);
        return purchase;
    }

    private static void EnsureAppliedCouponUsable(ShoppingCart cart, Coupon? coupon,
        IReadOnlyCollection<Purchase> existingPurchases, DateOnly today)
    {
        if (cart.AppliedCoupon is null) return;
        if (coupon is null || coupon.Id != cart.AppliedCoupon.CouponId)
            throw new DomainException("Primenjeni kupon više ne važi.");
        EnsureCouponUsable(cart.UserId, coupon, existingPurchases, today);
    }

    private static void EnsureCouponUsable(Guid userId, Coupon coupon, IReadOnlyCollection<Purchase> purchases,
        DateOnly today)
    {
        if (!coupon.IsValidOn(today)) throw new DomainException("Kupon je istekao.");
        if (purchases.Any(purchase => purchase.UserId == userId && purchase.AppliedCoupon?.CouponId == coupon.Id))
            throw new DomainException("Već si iskoristio ovaj kupon.");
    }

    private static void EnsureNotOwned(Guid userId, IEnumerable<Guid> entityIds, IReadOnlyCollection<Purchase> purchases)
    {
        if (purchases.Where(purchase => purchase.UserId == userId).SelectMany(purchase => purchase.Items)
            .Any(item => entityIds.Contains(item.EntityId)))
            throw new DomainException("Već si kupio ovaj proizvod.");
    }
}
