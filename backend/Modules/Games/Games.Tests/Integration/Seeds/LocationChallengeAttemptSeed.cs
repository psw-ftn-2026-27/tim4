using Games.Domain.Experience;
using Games.Domain.LocationChallenges;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class LocationChallengeAttemptSeed
{
    public static readonly LocationChallenge Challenge;
    public static readonly HiddenLocation Point;
    public static readonly LocationChallengeAttempt ActiveAttempt;
    public static readonly LocationChallengeAttempt CompletedAttempt;
    public static readonly EarnedXp Award;

    static LocationChallengeAttemptSeed()
    {
        var start = new ChallengeLocation(45.25, 19.85);
        Point = new HiddenLocation(new ChallengeLocation(45.26, 19.86), "/images/hidden.png");
        Challenge = new LocationChallenge("Lokacijski izazov", start, 25, [Point]);
        ActiveAttempt = Challenge.Activate(WellKnownUsers.Explorer, start, 0);
        CompletedAttempt = Challenge.Activate(WellKnownUsers.Explorer, start, 0);
        Award = CompletedAttempt.CheckLocation(Challenge, Point.PointId, Point.Location(), DateTime.UnixEpoch)!;
    }
}
