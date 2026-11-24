using Games.Domain.Experience;

namespace Games.Tests.Integration.Seeds;

internal static class EarnedXpSeed
{
    public static readonly EarnedXp SecondExplorerLocationAward;

    static EarnedXpSeed()
    {
        var attempt = LocationChallengeAttemptSeed.SecondExplorerCompletedAttempt;
        SecondExplorerLocationAward = new EarnedXp(attempt.UserId, ChallengeType.Location,
            attempt.ChallengeId, attempt.AwardedXp, attempt.CompletedAt!.Value);
    }

    public static EarnedXp[] All => [SecondExplorerLocationAward];
}
