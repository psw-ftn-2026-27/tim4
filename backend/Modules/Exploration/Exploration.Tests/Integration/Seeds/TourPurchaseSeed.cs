using Payment.Domain.Purchases;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.Wallets;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

// A Payment purchase of a seeded tour, so that tour details find it through Payment's contract.
// The cart and the wallet it was paid from are seeded with it.
internal static class TourPurchaseSeed
{
    public static readonly ShoppingCart ExplorersCompletedCart;
    public static readonly Wallet ExplorersWallet;
    public static readonly Purchase ExplorerBoughtVineyardsByBicycle;

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
    }

    public static object[] All => [ExplorersCompletedCart, ExplorersWallet, ExplorerBoughtVineyardsByBicycle];
}
