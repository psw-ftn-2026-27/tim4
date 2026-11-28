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
    public static readonly Purchase ExplorerBoughtFortressWithHiddenFirstKeyPoint;
    public static readonly Purchase ExplorerBoughtGame;
    public static readonly ShoppingCart SecondExplorersCompletedCart;
    public static readonly Wallet SecondExplorersWallet;
    public static readonly Purchase SecondExplorerBoughtMonasteries;
    public static readonly Purchase SecondExplorerBoughtOldTownWalkBeforeItWasArchived;

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

        // A second purchase from the same cart and wallet: a tour with a key point hidden until purchase.
        ExplorersCompletedCart.Add(TourProductSeed.FortressWithHiddenFirstKeyPointFor40);
        var fortressCheckout = ExplorersCompletedCart.PrepareCheckout([TourProductSeed.FortressWithHiddenFirstKeyPointFor40]);
        ExplorerBoughtFortressWithHiddenFirstKeyPoint = new Purchasing().Complete(ExplorersCompletedCart, ExplorersWallet,
            fortressCheckout, null, null, ExplorersCompletedCart.TotalToPay, [ExplorerBoughtVineyardsByBicycle], now);

        // A third purchase from the same cart and wallet: a product that is not a tour.
        ExplorersCompletedCart.Add(TourProductSeed.GameForFree);
        var gameCheckout = ExplorersCompletedCart.PrepareCheckout([TourProductSeed.GameForFree]);
        ExplorerBoughtGame = new Purchasing().Complete(ExplorersCompletedCart, ExplorersWallet, gameCheckout, null, null,
            ExplorersCompletedCart.TotalToPay, [ExplorerBoughtVineyardsByBicycle, ExplorerBoughtFortressWithHiddenFirstKeyPoint], now);

        SecondExplorersCompletedCart = new ShoppingCart(WellKnownUsers.SecondExplorer);
        SecondExplorersCompletedCart.Add(TourProductSeed.MonasteriesForFree);
        var secondCheckout = SecondExplorersCompletedCart.PrepareCheckout([TourProductSeed.MonasteriesForFree]);
        SecondExplorersWallet = new Wallet(WellKnownUsers.SecondExplorer);
        SecondExplorerBoughtMonasteries = new Purchasing().Complete(SecondExplorersCompletedCart, SecondExplorersWallet,
            secondCheckout, null, null, SecondExplorersCompletedCart.TotalToPay, [], now);

        // A second purchase from the same cart and wallet: a paid tour its author has since archived.
        SecondExplorersWallet.Credit(20, WellKnownUsers.Administrator, now.AddHours(-1));
        SecondExplorersCompletedCart.Add(TourProductSeed.ArchivedOldTownWalkFor20);
        var oldTownCheckout = SecondExplorersCompletedCart.PrepareCheckout([TourProductSeed.ArchivedOldTownWalkFor20]);
        SecondExplorerBoughtOldTownWalkBeforeItWasArchived = new Purchasing().Complete(SecondExplorersCompletedCart,
            SecondExplorersWallet, oldTownCheckout, null, null, SecondExplorersCompletedCart.TotalToPay,
            [SecondExplorerBoughtMonasteries], now);
    }

    public static object[] All =>
    [
        ExplorersCompletedCart, ExplorersWallet, ExplorerBoughtVineyardsByBicycle, ExplorerBoughtFortressWithHiddenFirstKeyPoint,
        ExplorerBoughtGame, SecondExplorersCompletedCart, SecondExplorersWallet, SecondExplorerBoughtMonasteries,
        SecondExplorerBoughtOldTownWalkBeforeItWasArchived,
    ];
}
