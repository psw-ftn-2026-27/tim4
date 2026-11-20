using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourExecutionSeed
{
    public static readonly TourExecution ExplorersVineyardsByBicycleWithFirstKeyPointReached;
    public static readonly TourExecution SecondExplorersMonasteriesCompleted;

    static TourExecutionSeed()
    {
        ExplorersVineyardsByBicycleWithFirstKeyPointReached = new(TourSeed.PublishedVineyardsByBicycleEasy.Id, WellKnownUsers.Explorer, new Location(45.2, 19.93), DateTime.UtcNow.AddMinutes(-30));
        ExplorersVineyardsByBicycleWithFirstKeyPointReached.ReachKeyPoint(TourSeed.PublishedVineyardsByBicycleEasy.KeyPoints[0].Id, false, DateTime.UtcNow.AddMinutes(-20));

        SecondExplorersMonasteriesCompleted = new(TourSeed.PublishedMonasteries.Id, WellKnownUsers.SecondExplorer, new Location(45.121, 19.944), DateTime.UtcNow.AddDays(-2));
        SecondExplorersMonasteriesCompleted.ReachKeyPoint(TourSeed.PublishedMonasteries.KeyPoints[0].Id, false, DateTime.UtcNow.AddDays(-2).AddHours(1));
        SecondExplorersMonasteriesCompleted.ReachKeyPoint(TourSeed.PublishedMonasteries.KeyPoints[1].Id, true, DateTime.UtcNow.AddDays(-2).AddHours(3));
    }

    public static TourExecution[] All => [ExplorersVineyardsByBicycleWithFirstKeyPointReached, SecondExplorersMonasteriesCompleted];
}
