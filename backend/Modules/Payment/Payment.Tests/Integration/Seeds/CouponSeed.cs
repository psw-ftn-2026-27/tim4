using Payment.Domain.Coupons;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class CouponSeed
{
    public static readonly Coupon ExplorerForExpensiveOffer;
    public static readonly Coupon ExplorerExpiringToday;
    public static readonly Coupon ExplorerExpiredYesterday;
    public static readonly Coupon SecondExplorerForAllProducts;
    public static readonly Coupon[] All;

    static CouponSeed()
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var issuing = new CouponIssuing();
        ExplorerForExpensiveOffer = issuing.Issue(WellKnownUsers.Explorer,
            [ProductSeed.ExpensiveOffer, ProductSeed.FreeOffer], ProductSeed.ExpensiveOffer.Id, 20, today.AddDays(30), today);
        ExplorerExpiringToday = issuing.Issue(WellKnownUsers.Explorer,
            [ProductSeed.ExpensiveOffer, ProductSeed.FreeOffer], null, 10, today, today);
        ExplorerExpiredYesterday = issuing.Issue(WellKnownUsers.Explorer,
            [ProductSeed.ExpensiveOffer, ProductSeed.FreeOffer], null, 15, today.AddDays(-1), today.AddDays(-1));
        SecondExplorerForAllProducts = issuing.Issue(WellKnownUsers.SecondExplorer,
            [ProductSeed.ExistingOffer, ProductSeed.CartOffer], null, 50, today.AddDays(10), today);
        All = [ExplorerForExpensiveOffer, ExplorerExpiringToday, ExplorerExpiredYesterday, SecondExplorerForAllProducts];
    }
}
