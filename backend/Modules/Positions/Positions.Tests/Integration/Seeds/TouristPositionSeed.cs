using Positions.Domain.TouristPositions;
using Shared.Tests;

namespace Positions.Tests.Integration.Seeds;

internal static class TouristPositionSeed
{
    public static readonly TouristPosition ExplorersPosition = new(WellKnownUsers.Explorer, new Coordinates(45.2551, 19.8451));
    public static readonly TouristPosition SecondExplorersPosition = new(WellKnownUsers.SecondExplorer, new Coordinates(44.8176, 20.4569));

    public static TouristPosition[] All => [ExplorersPosition, SecondExplorersPosition];
}
