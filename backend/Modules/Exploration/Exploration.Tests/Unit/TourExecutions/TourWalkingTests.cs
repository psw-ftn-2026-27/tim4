using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.TourExecutions;

public class TourWalkingTests
{
    // How many meters of the Earth's surface one degree of latitude spans, by the radius Location uses.
    private const double MetersPerDegreeOfLatitude = 6371000 * Math.PI / 180;

    [Theory]
    [InlineData(0, PositionCheckOutcome.ReachedKeyPoint)]
    [InlineData(49, PositionCheckOutcome.ReachedKeyPoint)]
    [InlineData(51, PositionCheckOutcome.NotNear)]
    [InlineData(500, PositionCheckOutcome.NotNear)]
    public void Tourist_reaches_the_next_key_point_only_within_50_m_of_it(double metersAway, PositionCheckOutcome expected)
    {
        var tour = CreateTour();
        var execution = StartExecution(tour);
        var next = tour.KeyPoints[0].Location;
        var position = new Location(next.Latitude + metersAway / MetersPerDegreeOfLatitude, next.Longitude);

        var outcome = new TourWalking().CheckPosition(tour, execution, position, DateTime.UtcNow);

        outcome.Should().Be(expected);
        execution.HasReached(tour.KeyPoints[0].Id).Should().Be(expected == PositionCheckOutcome.ReachedKeyPoint);
    }

    [Fact]
    public void Being_near_a_key_point_that_is_not_the_next_one_does_not_change_the_session()
    {
        var tour = CreateTour();
        var execution = StartExecution(tour);

        var outcome = new TourWalking().CheckPosition(tour, execution, tour.KeyPoints[1].Location, DateTime.UtcNow);

        outcome.Should().Be(PositionCheckOutcome.NotNear);
        execution.ReachedKeyPoints.Should().BeEmpty();
        execution.Status.Should().Be(TourExecutionStatus.InProgress);
    }

    [Fact]
    public void Every_check_records_the_last_activity_even_when_the_tourist_is_not_near()
    {
        var tour = CreateTour();
        var execution = StartExecution(tour);
        var now = DateTime.UtcNow;

        new TourWalking().CheckPosition(tour, execution, new Location(44.8, 20.4), now);

        execution.LastActivityAt.Should().Be(now);
    }

    [Fact]
    public void Reaching_the_last_key_point_completes_the_session_at_that_time()
    {
        var tour = CreateTour();
        var execution = StartExecution(tour);
        execution.ReachKeyPoint(tour.KeyPoints[0].Id, false, DateTime.UtcNow);
        execution.ReachKeyPoint(tour.KeyPoints[1].Id, false, DateTime.UtcNow);
        var now = DateTime.UtcNow;

        var outcome = new TourWalking().CheckPosition(tour, execution, tour.KeyPoints[2].Location, now);

        outcome.Should().Be(PositionCheckOutcome.CompletedTour);
        execution.Status.Should().Be(TourExecutionStatus.Completed);
        execution.CompletedAt.Should().Be(now);
    }

    [Fact]
    public void Check_does_not_change_a_completed_session()
    {
        var tour = CreateTour();
        var execution = StartExecution(tour);
        execution.ReachKeyPoint(tour.KeyPoints[0].Id, false, DateTime.UtcNow);
        execution.ReachKeyPoint(tour.KeyPoints[1].Id, false, DateTime.UtcNow);
        execution.ReachKeyPoint(tour.KeyPoints[2].Id, true, DateTime.UtcNow);
        var lastActivityBefore = execution.LastActivityAt;

        var check = () => new TourWalking().CheckPosition(tour, execution, tour.KeyPoints[2].Location, DateTime.UtcNow.AddMinutes(5));

        check.Should().Throw<DomainException>();
        execution.LastActivityAt.Should().Be(lastActivityBefore);
        execution.ReachedKeyPointCount.Should().Be(3);
    }

    private static TourExecution StartExecution(Tour tour)
    {
        return new TourExecution(tour.Id, Guid.NewGuid(), new Location(45.24, 19.84), DateTime.UtcNow.AddHours(-1));
    }

    private static Tour CreateTour()
    {
        var tour = new Tour(Guid.NewGuid(), "Šetnja", new string('a', 100), TourDifficulty.Easy, ["grad"], new RecommendedGroupSize(1, 10));
        tour.AddTransportTime(TransportMode.Walking, 60);
        tour.AddKeyPoint(new KeyPoint("Prva", "Prva tačka.", "/images/prva.jpg", "Prva tajna.", new Location(45.25, 19.85), true), 0);
        tour.AddKeyPoint(new KeyPoint("Druga", "Druga tačka.", "/images/druga.jpg", "Druga tajna.", new Location(45.26, 19.86), true), 1);
        tour.AddKeyPoint(new KeyPoint("Treća", "Treća tačka.", "/images/treca.jpg", null, new Location(45.27, 19.87), true), 2);
        tour.Publish(new TourPricing(TourPricingType.Free, null));
        return tour;
    }
}
