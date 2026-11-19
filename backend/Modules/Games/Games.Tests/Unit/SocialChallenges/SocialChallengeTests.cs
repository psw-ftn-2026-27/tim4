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

        challenge.UpdateDetails("Jutarnji susret", time, 4, 25, 60, "Bravo!", 45.25, 19.85);

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

    [Theory]
    [InlineData(null, null)]
    [InlineData(null, 19d)]
    [InlineData(45d, null)]
    [InlineData(91d, 19d)]
    [InlineData(45d, 181d)]
    [InlineData(double.NaN, 19d)]
    public void A_challenge_requires_valid_coordinates(double? latitude, double? longitude)
    {
        var create = () => new SocialChallenge("Susret",
            new ActivityTime(new TimeOnly(18, 0), new TimeOnly(20, 0)), 3, 20, 50, "Bravo!",
            latitude, longitude);

        create.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_draft_can_be_published()
    {
        var challenge = CreateChallenge();

        challenge.Publish();

        challenge.Status.Should().Be(SocialChallengeStatus.Published);
    }

    [Fact]
    public void A_published_challenge_can_be_retired()
    {
        var challenge = CreateChallenge();
        challenge.Publish();

        challenge.Retire();

        challenge.Status.Should().Be(SocialChallengeStatus.Retired);
    }

    [Fact]
    public void A_retired_challenge_can_be_archived()
    {
        var challenge = CreateChallenge();
        challenge.Publish();
        challenge.Retire();

        challenge.Archive();

        challenge.Status.Should().Be(SocialChallengeStatus.Archived);
    }

    [Fact]
    public void A_draft_cannot_skip_publishing()
    {
        var challenge = CreateChallenge();

        var retire = challenge.Retire;

        retire.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_published_challenge_cannot_skip_retirement()
    {
        var challenge = CreateChallenge();
        challenge.Publish();

        var archive = challenge.Archive;

        archive.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_published_challenge_cannot_be_published_again()
    {
        var challenge = CreateChallenge();
        challenge.Publish();

        var publish = challenge.Publish;

        publish.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_published_challenge_cannot_be_changed_or_deleted()
    {
        var challenge = CreateChallenge();
        challenge.Publish();
        var time = new ActivityTime(new TimeOnly(10, 0), new TimeOnly(11, 0));

        var update = () => challenge.UpdateDetails("Promena", time, 4, 25, 60, "Bravo!", 45, 19);
        var delete = challenge.EnsureCanBeDeleted;

        update.Should().Throw<DomainException>();
        delete.Should().Throw<DomainException>();
        challenge.Name.Should().Be("Večernji susret");
    }

    [Fact]
    public void An_archived_challenge_can_be_deleted()
    {
        var challenge = CreateChallenge();
        challenge.Publish();
        challenge.Retire();
        challenge.Archive();

        var delete = challenge.EnsureCanBeDeleted;

        delete.Should().NotThrow();
    }

    private static SocialChallenge CreateChallenge()
    {
        return new SocialChallenge("Večernji susret",
            new ActivityTime(new TimeOnly(18, 0), new TimeOnly(20, 0)), 3, 20, 50, "Uspeli ste!", 45.25, 19.85);
    }
}
