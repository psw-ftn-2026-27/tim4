using Payment.Domain.Purchases;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.Wallets;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

// Payment purchases of seeded tours, so that Exploration learns through Payment's contract
// which tourist bought which tour. The carts and wallets they were paid from are seeded with them.
internal static class TourPurchaseSeed
{
    public static readonly ShoppingCart ExplorersCompletedCart;
    public static readonly Wallet ExplorersWallet;
    public static readonly Purchase ExplorerBoughtVineyardsByBicycle;
    public static readonly ShoppingCart SecondExplorersCompletedCart;
    public static readonly Wallet SecondExplorersWallet;
    public static readonly Purchase SecondExplorerBoughtMonasteries;

    static TourPurchaseSeed()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds()).UtcDateTime;

        ExplorersCompletedCart = new ShoppingCart(WellKnownUsers.Explorer);
        ExplorersCompletedCart.Add(TourProductSeed.VineyardsByBicycleFor60);
        var checkout = ExplorersCompletedCart.PrepareCheckout([TourProductSeed.VineyardsByBicycleFor60]);
        ExplorersWallet = new Wallet(WellKnownUsers.Explorer);
        ExplorersWallet.Credit(100, WellKnownUsers.Administrator, now.AddHours(-1));
        ExplorerBoughtVineyardsByBicycle = new Purchasing().Complete(ExplorersCompletedCart, ExplorersWallet, checkout,
            null, null, ExplorersCompletedCart.TotalToPay, [], now);

        SecondExplorersCompletedCart = new ShoppingCart(WellKnownUsers.SecondExplorer);
        SecondExplorersCompletedCart.Add(TourProductSeed.MonasteriesForFree);
        var secondCheckout = SecondExplorersCompletedCart.PrepareCheckout([TourProductSeed.MonasteriesForFree]);
        SecondExplorersWallet = new Wallet(WellKnownUsers.SecondExplorer);
        SecondExplorerBoughtMonasteries = new Purchasing().Complete(SecondExplorersCompletedCart, SecondExplorersWallet,
            secondCheckout, null, null, SecondExplorersCompletedCart.TotalToPay, [], now);
    }

    public static object[] All =>
    [
        ExplorersCompletedCart, ExplorersWallet, ExplorerBoughtVineyardsByBicycle,
        SecondExplorersCompletedCart, SecondExplorersWallet, SecondExplorerBoughtMonasteries,
    ];
}
