using Games.Domain.LocationChallenges;

namespace Games.Tests.Integration.Seeds;

internal static class LocationChallengeSeed
{
    public static readonly LocationChallenge SinglePointChallenge = new(
        "Lokacijski izazov", new ChallengeLocation(45.25, 19.85), 25,
        [new HiddenLocation(new ChallengeLocation(45.26, 19.86), "/images/hidden.png")]);

    public static LocationChallenge[] All => [SinglePointChallenge];
}
