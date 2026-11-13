using Games.Domain.SocialChallenges;

namespace Games.Tests.Integration.Seeds;

internal static class SocialChallengeSeed
{
    public static readonly SocialChallenge EveningDraft = new("A. Večernji susret",
        new ActivityTime(new TimeOnly(18, 0), new TimeOnly(20, 0)), 3, 20, 50, "Uspeli ste!");
    public static readonly SocialChallenge MorningDraft = new("B. Jutarnji susret",
        new ActivityTime(new TimeOnly(8, 0), new TimeOnly(10, 0)), 2, 15, 25, "Dobro jutro!");

    public static SocialChallenge[] All => [EveningDraft, MorningDraft];
}
