using FluentAssertions;
using Games.Domain.LocationChallenges;
using Games.Domain.SocialChallengeRuns;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.SocialChallengeRuns;

public sealed class SocialChallengeJoiningTests
{
    private static readonly DateTime InsideActivity = new DateTime(2026, 11, 1, 19, 0, 0, DateTimeKind.Local).ToUniversalTime();
    private static readonly ChallengeLocation AtChallenge = new(45.25, 19.85);

    [Theory]
    [InlineData(false, 45.25, 19.85, 19)]
    [InlineData(true, 45.26, 19.86, 19)]
    [InlineData(true, 45.25, 19.85, 17)]
    [InlineData(true, 45.25, 19.85, 21)]
    public void Unpublished_challenge_player_out_of_range_and_time_outside_activity_reject_activation(
        bool published, double latitude, double longitude, int hour)
    {
        var challenge = CreateChallenge();
        if (published)
        {
            challenge.Publish();
        }
        var now = new DateTime(2026, 11, 1, hour, 0, 0, DateTimeKind.Local).ToUniversalTime();

        var join = () => new SocialChallengeJoining().Join(challenge, null, [], Guid.NewGuid(),
            new ChallengeLocation(latitude, longitude), now);

        join.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_player_who_already_solved_the_challenge_cannot_activate_it_again()
    {
        var challenge = CreatePublishedChallenge();
        var player = Guid.NewGuid();
        var solvedRun = new SocialChallengeJoining().Join(challenge, null, [], player, AtChallenge, InsideActivity);
        new SocialChallengeJoining().Join(challenge, solvedRun, [], Guid.NewGuid(), AtChallenge, InsideActivity);
        solvedRun.ReportLocation(challenge, player, AtChallenge, InsideActivity.AddSeconds(1));

        var join = () => new SocialChallengeJoining().Join(challenge, null, [solvedRun], player, AtChallenge,
            InsideActivity.AddMinutes(1));

        join.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_second_player_joins_the_active_run_instead_of_opening_a_new_one()
    {
        var challenge = CreatePublishedChallenge();
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var activeRun = new SocialChallengeJoining().Join(challenge, null, [], first, AtChallenge, InsideActivity);

        var run = new SocialChallengeJoining().Join(challenge, activeRun, [], second, AtChallenge, InsideActivity);

        run.Should().BeSameAs(activeRun);
        run.Participants.Select(participant => participant.UserId).Should().BeEquivalentTo([first, second]);
    }

    private static SocialChallenge CreatePublishedChallenge()
    {
        var challenge = CreateChallenge();
        challenge.Publish();
        return challenge;
    }

    private static SocialChallenge CreateChallenge()
    {
        return new SocialChallenge("Večernji susret",
            new ActivityTime(new TimeOnly(18, 0), new TimeOnly(20, 0)), 2, 20, 50, "Uspeli ste!",
            AtChallenge.Latitude, AtChallenge.Longitude);
    }
}
