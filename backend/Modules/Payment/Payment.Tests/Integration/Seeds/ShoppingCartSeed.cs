using Cart = Payment.Domain.ShoppingCarts.ShoppingCart;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class ShoppingCartSeed
{
    public static readonly Guid FreeBuyer = Guid.Parse("20000000-0000-0000-0000-000000000003");
    public static readonly Guid MultiBuyer = Guid.Parse("20000000-0000-0000-0000-000000000005");
    public static readonly Cart Ready;
    public static readonly Cart TooExpensive;
    public static readonly Cart Free;
    public static readonly Cart MultipleItems;
    public static readonly Cart Unprepared;
    public static readonly Cart[] All;

    static ShoppingCartSeed()
    {
        Ready = new Cart(WellKnownUsers.Explorer);
        Ready.Add(ProductSeed.CartOffer);
        Ready.PrepareCheckout([ProductSeed.CartOffer]);
        TooExpensive = new Cart(WellKnownUsers.SecondExplorer);
        TooExpensive.Add(ProductSeed.ExpensiveOffer);
        TooExpensive.PrepareCheckout([ProductSeed.ExpensiveOffer]);
        Free = new Cart(FreeBuyer);
        Free.Add(ProductSeed.FreeOffer);
        Free.PrepareCheckout([ProductSeed.FreeOffer]);
        MultipleItems = new Cart(MultiBuyer);
        MultipleItems.Add(ProductSeed.CartOffer);
        MultipleItems.Add(ProductSeed.ExistingOffer);
        MultipleItems.PrepareCheckout([ProductSeed.CartOffer, ProductSeed.ExistingOffer]);
        Unprepared = new Cart(Guid.Parse("20000000-0000-0000-0000-000000000006"));
        Unprepared.Add(ProductSeed.ExpensiveOffer);
        All = [Ready, TooExpensive, Free, MultipleItems, Unprepared, PurchaseSeed.CompletedCart];
    }
}
