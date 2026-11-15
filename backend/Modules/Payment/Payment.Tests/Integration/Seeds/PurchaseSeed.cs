using Payment.Domain.Purchases;
using Cart = Payment.Domain.ShoppingCarts.ShoppingCart;
using Payment.Domain.Wallets;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class PurchaseSeed
{
    public static readonly Guid Buyer = Guid.Parse("20000000-0000-0000-0000-000000000004");
    public static readonly Cart CompletedCart;
    public static readonly Wallet CompletedWallet;
    public static readonly Purchase Completed;
    public static readonly Purchase[] All;

    static PurchaseSeed()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds()).UtcDateTime;
        CompletedCart = new Cart(Buyer);
        CompletedCart.Add(ProductSeed.ExistingOffer);
        var checkout = CompletedCart.PrepareCheckout([ProductSeed.ExistingOffer]);
        CompletedWallet = new Wallet(Buyer);
        CompletedWallet.Credit(100, WellKnownUsers.Administrator, now.AddHours(-1));
        Completed = new Purchasing().Complete(CompletedCart, CompletedWallet, checkout, [], now);
        All = [Completed];
    }
}
