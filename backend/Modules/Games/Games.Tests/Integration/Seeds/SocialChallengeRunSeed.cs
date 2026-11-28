using Games.Domain.LocationChallenges;
using Games.Domain.SocialChallengeRuns;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class SocialChallengeRunSeed
{
    public static readonly SocialChallengeRun SolvedAllDayRun;

    static SocialChallengeRunSeed()
    {
        var challenge = SocialChallengeSeed.PublishedAllDay;
        var atChallenge = new ChallengeLocation(challenge.Latitude, challenge.Longitude);
        var joinedAt = new DateTime(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc);

        SolvedAllDayRun = new SocialChallengeJoining().Join(challenge, null, [], WellKnownUsers.Explorer, atChallenge, joinedAt);
        new SocialChallengeJoining().Join(challenge, SolvedAllDayRun, [], WellKnownUsers.SecondExplorer, atChallenge, joinedAt);
        SolvedAllDayRun.ReportLocation(challenge, WellKnownUsers.Explorer, atChallenge, joinedAt.AddSeconds(5));
    }

    public static SocialChallengeRun[] All => [SolvedAllDayRun];
}
