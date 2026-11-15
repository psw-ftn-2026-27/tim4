using Payment.Domain.Purchases;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.Wallets;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class PurchaseSeed
{
    public static readonly Guid Buyer = Guid.Parse("20000000-0000-0000-0000-000000000004");
    public static readonly ShoppingCart CompletedCart;
    public static readonly Wallet CompletedWallet;
    public static readonly Purchase Completed;
    public static readonly Purchase CompletedFree;
    public static readonly Purchase[] All;

    static PurchaseSeed()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds()).UtcDateTime;
        CompletedCart = new ShoppingCart(Buyer);
        CompletedCart.Add(ProductSeed.ExistingOffer);
        var checkout = CompletedCart.PrepareCheckout([ProductSeed.ExistingOffer]);
        CompletedWallet = new Wallet(Buyer);
        CompletedWallet.Credit(100, WellKnownUsers.Administrator, now.AddHours(-1));
        Completed = new Purchasing().Complete(CompletedCart, CompletedWallet, checkout, [], now);
        CompletedCart.Add(ProductSeed.FreeOffer);
        var freeCheckout = CompletedCart.PrepareCheckout([ProductSeed.FreeOffer]);
        CompletedFree = new Purchasing().Complete(CompletedCart, CompletedWallet, freeCheckout, [Completed], now);
        All = [Completed, CompletedFree];
    }
}
