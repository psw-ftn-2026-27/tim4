using Payment.Domain.Purchases;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.Wallets;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class PurchaseSeed
{
    public static readonly Guid Buyer = Guid.Parse("20000000-0000-0000-0000-000000000004");
    public static readonly Guid CouponBuyer = Guid.Parse("20000000-0000-0000-0000-000000000009");
    public static readonly ShoppingCart CompletedCart;
    public static readonly Wallet CompletedWallet;
    public static readonly Purchase Completed;
    public static readonly Purchase CompletedFree;
    public static readonly ShoppingCart CouponCart;
    public static readonly Wallet CouponWallet;
    public static readonly Purchase CompletedWithCoupon;
    public static readonly Purchase[] All;

    static PurchaseSeed()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds()).UtcDateTime;
        CompletedCart = new ShoppingCart(Buyer);
        CompletedCart.Add(ProductSeed.ExistingOffer);
        var checkout = CompletedCart.PrepareCheckout([ProductSeed.ExistingOffer]);
        CompletedWallet = new Wallet(Buyer);
        CompletedWallet.Credit(100, WellKnownUsers.Administrator, now.AddHours(-1));
        Completed = new Purchasing().Complete(CompletedCart, CompletedWallet, checkout, null, CompletedCart.TotalToPay, [], now);
        CompletedCart.Add(ProductSeed.FreeOffer);
        var freeCheckout = CompletedCart.PrepareCheckout([ProductSeed.FreeOffer]);
        CompletedFree = new Purchasing().Complete(CompletedCart, CompletedWallet, freeCheckout, null, CompletedCart.TotalToPay,
            [Completed], now);
        var today = DateOnly.FromDateTime(now);
        CouponCart = new ShoppingCart(CouponBuyer);
        CouponCart.Add(ProductSeed.CartOffer);
        var couponCheckout = CouponCart.PrepareCheckout([ProductSeed.CartOffer]);
        new Purchasing().ApplyCoupon(CouponCart, couponCheckout, CouponSeed.SecondExplorerForAllProducts, [], today);
        CouponWallet = new Wallet(CouponBuyer);
        CouponWallet.Credit(100, WellKnownUsers.Administrator, now.AddHours(-1));
        CompletedWithCoupon = new Purchasing().Complete(CouponCart, CouponWallet, couponCheckout,
            CouponSeed.SecondExplorerForAllProducts, CouponCart.TotalToPay, [], now);
        CouponCart.Add(ProductSeed.ExistingOffer);
        CouponCart.Add(ProductSeed.ExpensiveOffer);
        var nextCheckout = CouponCart.PrepareCheckout([ProductSeed.ExistingOffer, ProductSeed.ExpensiveOffer]);
        new Purchasing().ApplyCoupon(CouponCart, nextCheckout, CouponSeed.ExplorerForExpensiveOffer,
            [CompletedWithCoupon], today);
        All = [Completed, CompletedFree, CompletedWithCoupon];
    }
}
