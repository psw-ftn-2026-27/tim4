using Payment.Domain.Purchases;
using Payment.Domain.ShoppingCarts;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class ShoppingCartSeed
{
    public static readonly Guid FreeBuyer = Guid.Parse("20000000-0000-0000-0000-000000000003");
    public static readonly Guid MultiBuyer = Guid.Parse("20000000-0000-0000-0000-000000000005");
    public static readonly Guid CouponBuyer = Guid.Parse("20000000-0000-0000-0000-000000000007");
    public static readonly Guid ExpiredCouponBuyer = Guid.Parse("20000000-0000-0000-0000-000000000008");
    public static readonly ShoppingCart Ready;
    public static readonly ShoppingCart TooExpensive;
    public static readonly ShoppingCart Free;
    public static readonly ShoppingCart MultipleItems;
    public static readonly ShoppingCart Unprepared;
    public static readonly ShoppingCart WithCoupon;
    public static readonly ShoppingCart WithCouponExpiredAfterApplying;
    public static readonly ShoppingCart WithUsableGiftCard;
    public static readonly ShoppingCart WithGiftCard;
    public static readonly ShoppingCart WithGiftCardExpiredAfterApplying;
    public static readonly ShoppingCart[] All;

    static ShoppingCartSeed()
    {
        Ready = new ShoppingCart(WellKnownUsers.Explorer);
        Ready.Add(ProductSeed.CartOffer);
        Ready.PrepareCheckout([ProductSeed.CartOffer]);
        TooExpensive = new ShoppingCart(WellKnownUsers.SecondExplorer);
        TooExpensive.Add(ProductSeed.ExpensiveOffer);
        TooExpensive.PrepareCheckout([ProductSeed.ExpensiveOffer]);
        Free = new ShoppingCart(FreeBuyer);
        Free.Add(ProductSeed.FreeOffer);
        Free.PrepareCheckout([ProductSeed.FreeOffer]);
        MultipleItems = new ShoppingCart(MultiBuyer);
        MultipleItems.Add(ProductSeed.CartOffer);
        MultipleItems.Add(ProductSeed.ExistingOffer);
        MultipleItems.PrepareCheckout([ProductSeed.CartOffer, ProductSeed.ExistingOffer]);
        Unprepared = new ShoppingCart(Guid.Parse("20000000-0000-0000-0000-000000000006"));
        Unprepared.Add(ProductSeed.ExpensiveOffer);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        WithCoupon = new ShoppingCart(CouponBuyer);
        WithCoupon.Add(ProductSeed.ExpensiveOffer);
        var couponCheckout = WithCoupon.PrepareCheckout([ProductSeed.ExpensiveOffer]);
        new Purchasing().ApplyCoupon(WithCoupon, couponCheckout, CouponSeed.ExplorerForExpensiveOffer, [], today);
        WithCouponExpiredAfterApplying = new ShoppingCart(ExpiredCouponBuyer);
        WithCouponExpiredAfterApplying.Add(ProductSeed.ExpensiveOffer);
        var expiredCheckout = WithCouponExpiredAfterApplying.PrepareCheckout([ProductSeed.ExpensiveOffer]);
        new Purchasing().ApplyCoupon(WithCouponExpiredAfterApplying, expiredCheckout,
            CouponSeed.ExplorerExpiredYesterday, [], today.AddDays(-1));
        WithUsableGiftCard = new ShoppingCart(GiftCardSeed.FreeGiftCardBuyer);
        WithUsableGiftCard.Add(ProductSeed.CartOffer);
        WithUsableGiftCard.PrepareCheckout([ProductSeed.CartOffer]);
        WithGiftCard = new ShoppingCart(GiftCardSeed.AppliedGiftCardBuyer);
        WithGiftCard.Add(ProductSeed.CartOffer);
        var giftCardCheckout = WithGiftCard.PrepareCheckout([ProductSeed.CartOffer]);
        new Purchasing().UseGiftCard(WithGiftCard, giftCardCheckout, [GiftCardSeed.SmallerThanCartPrice], today);
        WithGiftCardExpiredAfterApplying = new ShoppingCart(GiftCardSeed.ExpiredGiftCardBuyer);
        WithGiftCardExpiredAfterApplying.Add(ProductSeed.ExpensiveOffer);
        var expiredGiftCardCheckout = WithGiftCardExpiredAfterApplying.PrepareCheckout([ProductSeed.ExpensiveOffer]);
        new Purchasing().UseGiftCard(WithGiftCardExpiredAfterApplying, expiredGiftCardCheckout,
            [GiftCardSeed.ExpiredYesterday], today.AddDays(-1));
        All = [Ready, TooExpensive, Free, MultipleItems, Unprepared, WithCoupon, WithCouponExpiredAfterApplying,
            WithUsableGiftCard, WithGiftCard, WithGiftCardExpiredAfterApplying,
            PurchaseSeed.CompletedCart, PurchaseSeed.CouponCart, PurchaseSeed.GiftCardCart];
    }
}
