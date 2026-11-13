namespace Games.Tests.Integration.Seeds;

internal static class GamesSeed
{
    public static object[] All => [.. QuizSeed.All, SocialChallengeSeed.EveningDraft, SocialChallengeSeed.MorningDraft];
}
