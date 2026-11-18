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

    static TourProductSeed()
    {
        VineyardsFor30 = new Product(TourSeed.PublishedVineyards.Id, TourSeed.PublishedVineyards.Name, TourSeed.PublishedVineyards.AuthorId, 30);
        MonasteriesForFree = new Product(TourSeed.PublishedMonasteries.Id, TourSeed.PublishedMonasteries.Name, TourSeed.PublishedMonasteries.AuthorId, 0);
        VineyardsByBicycleFor60 = new Product(TourSeed.PublishedVineyardsByBicycleEasy.Id, TourSeed.PublishedVineyardsByBicycleEasy.Name, TourSeed.PublishedVineyardsByBicycleEasy.AuthorId, 60);
        ArchivedOldTownWalkFor20 = new Product(TourSeed.ArchivedOldTownWalk.Id, TourSeed.ArchivedOldTownWalk.Name, TourSeed.ArchivedOldTownWalk.AuthorId, 20);
    }

    public static Product[] All => [VineyardsFor30, MonasteriesForFree, VineyardsByBicycleFor60, ArchivedOldTownWalkFor20];
}
