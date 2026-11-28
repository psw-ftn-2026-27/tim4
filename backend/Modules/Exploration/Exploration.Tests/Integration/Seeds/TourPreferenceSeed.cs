using Exploration.Domain.TourPreferences;
using Exploration.Domain.Tours;
using Shared.Tests;

namespace Exploration.Tests.Integration.Seeds;

internal static class TourPreferenceSeed
{
    public static readonly TourPreference ExplorersPreference = new(WellKnownUsers.Explorer, new PreferenceCriteria([TourDifficulty.Easy, TourDifficulty.Moderate], TransportMode.Walking, ["istorija"]), true);
    public static readonly TourPreference SecondExplorersPreference = new(WellKnownUsers.SecondExplorer, new PreferenceCriteria([TourDifficulty.Hard], TransportMode.Car, ["vino"]), false);

    public static TourPreference[] All => [ExplorersPreference, SecondExplorersPreference];
}
