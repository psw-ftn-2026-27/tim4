using Payment.Domain.Products;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class ProductSeed
{
    public static readonly Product ExistingOffer;
    public static readonly Product[] All;

    static ProductSeed()
    {
        ExistingOffer = new Product(Guid.NewGuid(), "Existing digital resource", WellKnownUsers.Explorer, 25.5m);
        All = [ExistingOffer];
    }
}
