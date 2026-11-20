using FluentAssertions;
using Games.Domain.LocationChallenges;
using Games.Domain.SocialChallengeRuns;
using Games.Domain.SocialChallenges;
using Xunit;

namespace Games.Tests.Unit.SocialChallengeRuns;

public sealed class SocialChallengeRunTests
{
    private static readonly DateTime Start = new(2026, 11, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly ChallengeLocation AtChallenge = new(45.25, 19.85);
    private static readonly ChallengeLocation FarFromChallenge = new(45.26, 19.86);

    [Fact]
    public void Run_is_solved_once_enough_active_players_are_in_range_and_each_active_player_gets_the_xp()
    {
        var challenge = CreatePublishedChallenge(requiredParticipants: 2);
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var run = new SocialChallengeJoining().Join(challenge, null, [], first, AtChallenge, Start);
        new SocialChallengeJoining().Join(challenge, run, [], second, AtChallenge, Start.AddSeconds(1));

        run.ReportLocation(challenge, first, AtChallenge, Start.AddSeconds(2));

        run.SolvedAt.Should().Be(Start.AddSeconds(2));
        run.AwardedXp.Should().Be(challenge.XpReward);
        run.Participants.Select(participant => participant.UserId).Should().BeEquivalentTo([first, second]);
    }

    [Fact]
    public void A_player_silent_for_more_than_ten_seconds_is_not_counted_and_gets_no_xp()
    {
        var challenge = CreatePublishedChallenge(requiredParticipants: 2);
        var silent = Guid.NewGuid();
        var onTheLimit = Guid.NewGuid();
        var reporter = Guid.NewGuid();
        var run = new SocialChallengeJoining().Join(challenge, null, [], silent, AtChallenge, Start);
        new SocialChallengeJoining().Join(challenge, run, [], onTheLimit, AtChallenge, Start.AddSeconds(2));
        new SocialChallengeJoining().Join(challenge, run, [], reporter, AtChallenge, Start.AddSeconds(5));

        run.ReportLocation(challenge, reporter, AtChallenge, Start.AddSeconds(12));

        run.SolvedAt.Should().Be(Start.AddSeconds(12));
        run.Participants.Select(participant => participant.UserId).Should().BeEquivalentTo([onTheLimit, reporter]);
    }

    [Fact]
    public void A_player_reporting_from_outside_the_range_drops_out_of_the_run()
    {
        var challenge = CreatePublishedChallenge(requiredParticipants: 3);
        var leaving = Guid.NewGuid();
        var staying = Guid.NewGuid();
        var run = new SocialChallengeJoining().Join(challenge, null, [], leaving, AtChallenge, Start);
        new SocialChallengeJoining().Join(challenge, run, [], staying, AtChallenge, Start);

        run.ReportLocation(challenge, leaving, FarFromChallenge, Start.AddSeconds(1));

        run.Participants.Should().ContainSingle().Which.UserId.Should().Be(staying);
        run.SolvedAt.Should().BeNull();
    }

    private static SocialChallenge CreatePublishedChallenge(int requiredParticipants)
    {
        var challenge = new SocialChallenge("Celodnevni susret",
            new ActivityTime(TimeOnly.MinValue, TimeOnly.MaxValue), requiredParticipants, 20, 50, "Uspeli ste!",
            AtChallenge.Latitude, AtChallenge.Longitude);
        challenge.Publish();
        return challenge;
    }
}
