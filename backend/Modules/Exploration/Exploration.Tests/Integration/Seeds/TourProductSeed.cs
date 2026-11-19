using Payment.Domain.Products;

namespace Exploration.Tests.Integration.Seeds;

// Payment products of the seeded tours, so that tour search finds their prices through
// Payment's contract. A tour without a product here has no price.
internal static class TourProductSeed
{
    public static readonly Product VineyardsFor30;
    public static readonly Product MonasteriesForFree;
    public static readonly Product VineyardsByBicycleFor60;
    public static readonly Product ArchivedOldTownWalkFor20;
    public static readonly Product FortressWithHiddenFirstKeyPointFor40;
    // A product left behind for a draft tour, so that publishing it at another price conflicts.
    public static readonly Product DraftQuayFor25;

    static TourProductSeed()
    {
        VineyardsFor30 = new Product(TourSeed.PublishedVineyards.Id, TourSeed.PublishedVineyards.Name, TourSeed.PublishedVineyards.AuthorId, 30);
        MonasteriesForFree = new Product(TourSeed.PublishedMonasteries.Id, TourSeed.PublishedMonasteries.Name, TourSeed.PublishedMonasteries.AuthorId, 0);
        VineyardsByBicycleFor60 = new Product(TourSeed.PublishedVineyardsByBicycleEasy.Id, TourSeed.PublishedVineyardsByBicycleEasy.Name, TourSeed.PublishedVineyardsByBicycleEasy.AuthorId, 60);
        ArchivedOldTownWalkFor20 = new Product(TourSeed.ArchivedOldTownWalk.Id, TourSeed.ArchivedOldTownWalk.Name, TourSeed.ArchivedOldTownWalk.AuthorId, 20);
        FortressWithHiddenFirstKeyPointFor40 = new Product(TourSeed.PublishedFortressWithHiddenFirstKeyPoint.Id, TourSeed.PublishedFortressWithHiddenFirstKeyPoint.Name, TourSeed.PublishedFortressWithHiddenFirstKeyPoint.AuthorId, 40);
        DraftQuayFor25 = new Product(TourSeed.PublishableQuayWithExistingProduct.Id, TourSeed.PublishableQuayWithExistingProduct.Name, TourSeed.PublishableQuayWithExistingProduct.AuthorId, 25);
    }

    public static Product[] All => [VineyardsFor30, MonasteriesForFree, VineyardsByBicycleFor60, ArchivedOldTownWalkFor20, FortressWithHiddenFirstKeyPointFor40, DraftQuayFor25];
}
