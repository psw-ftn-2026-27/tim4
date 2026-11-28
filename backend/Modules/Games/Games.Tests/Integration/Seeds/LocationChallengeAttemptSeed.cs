using Games.Domain.LocationChallenges;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class LocationChallengeAttemptSeed
{
    public static readonly LocationChallengeAttempt ExplorerActiveAttempt;
    public static readonly LocationChallengeAttempt SecondExplorerCompletedAttempt;

    static LocationChallengeAttemptSeed()
    {
        var challenge = LocationChallengeSeed.SinglePointChallenge;
        var start = new ChallengeLocation(challenge.StartLatitude, challenge.StartLongitude);
        var point = challenge.HiddenLocations[0];
        ExplorerActiveAttempt = challenge.Activate(WellKnownUsers.Explorer, start, 0);
        SecondExplorerCompletedAttempt = challenge.Activate(WellKnownUsers.SecondExplorer, start, 0);
        SecondExplorerCompletedAttempt.CheckLocation(challenge, point.PointId, point.Location(), DateTime.UnixEpoch);
    }

    public static LocationChallengeAttempt[] All => [ExplorerActiveAttempt, SecondExplorerCompletedAttempt];
}
