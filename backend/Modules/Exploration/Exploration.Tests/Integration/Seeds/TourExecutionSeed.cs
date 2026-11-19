using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourExecutionSeed
{
    public static readonly TourExecution ExplorersVineyardsByBicycleInProgress = new(TourSeed.PublishedVineyardsByBicycleEasy.Id, WellKnownUsers.Explorer, new Location(45.2, 19.93), DateTime.UtcNow);

    public static TourExecution[] All => [ExplorersVineyardsByBicycleInProgress];
}
