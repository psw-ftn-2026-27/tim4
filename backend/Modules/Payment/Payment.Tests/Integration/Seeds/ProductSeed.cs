using Payment.Domain.Products;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class ProductSeed
{
    public static readonly Product ExistingOffer;
    public static readonly Product CartOffer;
    public static readonly Product ExpensiveOffer;
    public static readonly Product FreeOffer;
    public static readonly Product[] All;

    static ProductSeed()
    {
        ExistingOffer = new Product(Guid.NewGuid(), "Existing digital resource", WellKnownUsers.Explorer, 25);
        CartOffer = new Product(Guid.NewGuid(), "Cart resource", WellKnownUsers.SecondExplorer, 30);
        ExpensiveOffer = new Product(Guid.NewGuid(), "Expensive resource", WellKnownUsers.Explorer, 1000);
        FreeOffer = new Product(Guid.NewGuid(), "Free resource", WellKnownUsers.Explorer, 0);
        All = [ExistingOffer, CartOffer, ExpensiveOffer, FreeOffer];
    }
}
