using FluentAssertions;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.LocationChallenges;

public sealed class LocationChallengeTests
{
    private static readonly ChallengeLocation Start = new(45, 19);

    [Fact]
    public void Challenge_requires_a_hidden_point()
    {
        var create = () => new LocationChallenge("Izazov", Start, 25, []);

        create.Should().Throw<DomainException>();
    }

    [Fact]
    public void Explorer_outside_the_activation_distance_cannot_start_the_challenge()
    {
        var challenge = CreateChallenge();

        var activate = () => challenge.Activate(Guid.NewGuid(), new ChallengeLocation(45.01, 19), 10);

        activate.Should().Throw<DomainException>();
    }

    [Fact]
    public void Search_radius_includes_the_distance_to_the_farthest_point_and_up_to_one_hundred_more_meters()
    {
        var challenge = CreateChallenge();
        var farthestDistance = Start.DistanceTo(challenge.HiddenLocations[1].Location());

        var attempt = challenge.Activate(Guid.NewGuid(), Start, 10);

        attempt.DiameterInMeters.Should().BeInRange(farthestDistance, farthestDistance + 100);
    }

    [Theory]
    [InlineData(4.99, true)]
    [InlineData(5, true)]
    [InlineData(5.01, false)]
    public void Explorer_must_be_within_five_meters_to_discover_a_point(double distance, bool succeeds)
    {
        const double earthRadius = 6_371_000;
        var point = new HiddenLocation(new ChallengeLocation(0, 0), "/images/point.png");
        var challenge = new LocationChallenge("Izazov", Start, 25, [point]);
        var attempt = challenge.Activate(Guid.NewGuid(), Start, 10);
        var position = new ChallengeLocation(distance / earthRadius * (180 / Math.PI), 0);
        var check = () => attempt.CheckLocation(challenge, point.PointId, position, DateTime.UnixEpoch);

        if (succeeds)
        {
            check.Should().NotThrow();
            attempt.DiscoveredPointIds.Should().ContainSingle().Which.Should().Be(point.PointId);
        }
        else
        {
            check.Should().Throw<DomainException>();
            attempt.DiscoveredPointIds.Should().BeEmpty();
            attempt.AwardedXp.Should().Be(0);
        }
    }

    [Fact]
    public void Discovering_the_first_point_of_two_does_not_award_xp()
    {
        var challenge = CreateChallenge();
        var attempt = challenge.Activate(Guid.NewGuid(), Start, 10);
        var point = challenge.HiddenLocations[0];

        attempt.CheckLocation(challenge, point.PointId, point.Location(), DateTime.UnixEpoch);

        attempt.CompletedAt.Should().BeNull();
        attempt.AwardedXp.Should().Be(0);
    }

    [Fact]
    public void Discovering_the_same_point_twice_does_not_finish_a_two_point_challenge()
    {
        var challenge = CreateChallenge();
        var attempt = challenge.Activate(Guid.NewGuid(), Start, 10);
        var first = challenge.HiddenLocations[0];
        attempt.CheckLocation(challenge, first.PointId, first.Location(), DateTime.UnixEpoch);

        attempt.CheckLocation(challenge, first.PointId, first.Location(), DateTime.UnixEpoch.AddMinutes(1));

        attempt.DiscoveredPointIds.Should().ContainSingle().Which.Should().Be(first.PointId);
        attempt.CompletedAt.Should().BeNull();
        attempt.AwardedXp.Should().Be(0);
    }

    [Fact]
    public void Discovering_the_last_point_records_completion_and_awards_xp()
    {
        var challenge = CreateChallenge();
        var attempt = challenge.Activate(Guid.NewGuid(), Start, 10);
        var first = challenge.HiddenLocations[0];
        var last = challenge.HiddenLocations[1];
        attempt.CheckLocation(challenge, first.PointId, first.Location(), DateTime.UnixEpoch);
        var finishedAt = DateTime.UnixEpoch.AddMinutes(1);

        attempt.CheckLocation(challenge, last.PointId, last.Location(), finishedAt);

        attempt.CompletedAt.Should().Be(finishedAt);
        attempt.AwardedXp.Should().Be(challenge.XpReward);
    }

    [Fact]
    public void Rechecking_a_completed_challenge_does_not_change_the_completion_or_award()
    {
        var challenge = CreateChallenge();
        var attempt = challenge.Activate(Guid.NewGuid(), Start, 10);
        foreach (var point in challenge.HiddenLocations)
        {
            attempt.CheckLocation(challenge, point.PointId, point.Location(), DateTime.UnixEpoch);
        }
        var last = challenge.HiddenLocations[1];

        attempt.CheckLocation(challenge, last.PointId, last.Location(), DateTime.UnixEpoch.AddMinutes(1));

        attempt.CompletedAt.Should().Be(DateTime.UnixEpoch);
        attempt.AwardedXp.Should().Be(challenge.XpReward);
        attempt.DiscoveredPointIds.Should().HaveCount(2);
    }

    private static LocationChallenge CreateChallenge() => new("Izazov", Start, 25,
    [
        new HiddenLocation(new ChallengeLocation(45.001, 19), "/images/first.png"),
        new HiddenLocation(new ChallengeLocation(45.002, 19), "/images/last.png")
    ]);
}
