using Games.Domain.Experience;
using Shared.Tests;

namespace Games.Tests.Integration.Seeds;

internal static class EarnedXpSeed
{
    public static readonly EarnedXp SecondExplorerLocationAward;
    public static readonly EarnedXp SecondExplorerQuizAward;
    public static readonly EarnedXp ExplorerSocialAward;
    public static readonly EarnedXp SecondExplorerSocialAward;

    static EarnedXpSeed()
    {
        var attempt = LocationChallengeAttemptSeed.SecondExplorerCompletedAttempt;
        SecondExplorerLocationAward = new EarnedXp(attempt.UserId, ChallengeType.Location,
            attempt.ChallengeId, attempt.AwardedXp, attempt.CompletedAt!.Value);

        var quizAttempt = QuizAttemptSeed.SecondExplorerSolvedMonasteriesAttempt;
        SecondExplorerQuizAward = new EarnedXp(quizAttempt.PlayerId, ChallengeType.Quiz, quizAttempt.QuizId,
            (int)Math.Round(quizAttempt.EarnedXp(), MidpointRounding.AwayFromZero), quizAttempt.CompletedAt!.Value);

        var run = SocialChallengeRunSeed.SolvedAllDayRun;
        ExplorerSocialAward = new EarnedXp(WellKnownUsers.Explorer, ChallengeType.Social,
            run.ChallengeId, run.AwardedXp, run.SolvedAt!.Value);
        SecondExplorerSocialAward = new EarnedXp(WellKnownUsers.SecondExplorer, ChallengeType.Social,
            run.ChallengeId, run.AwardedXp, run.SolvedAt!.Value);
    }

    public static EarnedXp[] All =>
        [SecondExplorerLocationAward, SecondExplorerQuizAward, ExplorerSocialAward, SecondExplorerSocialAward];
}
