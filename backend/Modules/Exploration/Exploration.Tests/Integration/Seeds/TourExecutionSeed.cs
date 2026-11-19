using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourExecutionSeed
{
    public static readonly TourExecution ExplorersVineyardsByBicycleWithFirstKeyPointReached;

    static TourExecutionSeed()
    {
        ExplorersVineyardsByBicycleWithFirstKeyPointReached = new(TourSeed.PublishedVineyardsByBicycleEasy.Id, WellKnownUsers.Explorer, new Location(45.2, 19.93), DateTime.UtcNow.AddMinutes(-30));
        ExplorersVineyardsByBicycleWithFirstKeyPointReached.ReachKeyPoint(TourSeed.PublishedVineyardsByBicycleEasy.KeyPoints[0].Id, false, DateTime.UtcNow.AddMinutes(-20));
    }

    public static TourExecution[] All => [ExplorersVineyardsByBicycleWithFirstKeyPointReached];
}
