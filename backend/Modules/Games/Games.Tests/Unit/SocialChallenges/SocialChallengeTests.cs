using FluentAssertions;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.SocialChallenges;

public sealed class SocialChallengeTests
{
    [Fact]
    public void New_challenge_is_a_draft()
    {
        var challenge = CreateChallenge();

        challenge.Status.Should().Be(SocialChallengeStatus.Draft);
    }

    [Fact]
    public void Activity_accepts_a_same_day_interval()
    {
        var start = new TimeOnly(18, 0);
        var end = new TimeOnly(20, 0);

        var activityTime = new ActivityTime(start, end);

        activityTime.Should().BeEquivalentTo(new { Start = start, End = end });
    }

    [Theory]
    [InlineData(18, 18)]
    [InlineData(22, 2)]
    public void Activity_rejects_equal_times_or_an_overnight_interval(int start, int end)
    {
        var create = () => new ActivityTime(new TimeOnly(start, 0), new TimeOnly(end, 0));

        create.Should().Throw<DomainException>();
    }

    [Fact]
    public void Draft_accepts_changed_details()
    {
        var challenge = CreateChallenge();
        var time = new ActivityTime(new TimeOnly(10, 0), new TimeOnly(11, 0));

        challenge.UpdateDetails("Jutarnji susret", time, 4, 25, 60, "Bravo!");

        challenge.Should().BeEquivalentTo(new
        {
            Name = "Jutarnji susret",
            ActivityTime = time,
            RequiredParticipants = 4,
            RangeInMeters = 25m,
            XpReward = 60,
            CompletionMessage = "Bravo!",
            Status = SocialChallengeStatus.Draft
        });
    }

    [Fact]
    public void Draft_can_be_deleted()
    {
        var challenge = CreateChallenge();

        var delete = challenge.EnsureCanBeDeleted;

        delete.Should().NotThrow();
    }

    private static SocialChallenge CreateChallenge()
    {
        return new SocialChallenge("Večernji susret",
            new ActivityTime(new TimeOnly(18, 0), new TimeOnly(20, 0)), 3, 20, 50, "Uspeli ste!");
    }
}
