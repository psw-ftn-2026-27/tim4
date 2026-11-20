using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.TourExecutions;

public class TourExecutionTests
{
    [Fact]
    public void New_session_is_in_progress_from_its_start_time_at_the_tourists_location()
    {
        var tourId = Guid.NewGuid();
        var touristId = Guid.NewGuid();
        var location = new Location(45.25, 19.85);
        var now = DateTime.UtcNow;

        var execution = new TourExecution(tourId, touristId, location, now);

        execution.TourId.Should().Be(tourId);
        execution.TouristId.Should().Be(touristId);
        execution.Status.Should().Be(TourExecutionStatus.InProgress);
        execution.StartedAt.Should().Be(now);
        execution.LastActivityAt.Should().Be(now);
        execution.StartLocation.Should().Be(location);
    }

    [Fact]
    public void Session_of_a_tour_without_key_points_has_no_next_key_point()
    {
        var execution = new TourExecution(Guid.NewGuid(), Guid.NewGuid(), new Location(45.25, 19.85), DateTime.UtcNow);

        var next = () => execution.NextKeyPointId([]);

        next.Should().Throw<DomainException>();
    }

    [Fact]
    public void Next_key_point_is_the_first_one_in_tour_order_that_is_not_reached()
    {
        var keyPointIds = new List<Guid> { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };
        var execution = new TourExecution(Guid.NewGuid(), Guid.NewGuid(), new Location(45.25, 19.85), DateTime.UtcNow);
        execution.ReachKeyPoint(keyPointIds[0], false, DateTime.UtcNow);

        var next = execution.NextKeyPointId(keyPointIds);

        next.Should().Be(keyPointIds[1]);
    }

    [Fact]
    public void Key_point_that_is_already_reached_cannot_be_reached_again()
    {
        var keyPointId = Guid.NewGuid();
        var execution = new TourExecution(Guid.NewGuid(), Guid.NewGuid(), new Location(45.25, 19.85), DateTime.UtcNow);
        execution.ReachKeyPoint(keyPointId, false, DateTime.UtcNow);

        var reachAgain = () => execution.ReachKeyPoint(keyPointId, false, DateTime.UtcNow);

        reachAgain.Should().Throw<DomainException>();
        execution.ReachedKeyPointCount.Should().Be(1);
    }

    [Fact]
    public void Abandoning_a_session_in_progress_makes_it_abandoned_at_that_time()
    {
        var execution = NewSession();
        var now = DateTime.UtcNow;

        execution.Abandon(now);

        execution.Status.Should().Be(TourExecutionStatus.Abandoned);
        execution.AbandonedAt.Should().Be(now);
    }

    [Fact]
    public void Completed_session_cannot_be_abandoned()
    {
        var execution = NewSession();
        execution.ReachKeyPoint(Guid.NewGuid(), true, DateTime.UtcNow);

        var abandon = () => execution.Abandon(DateTime.UtcNow);

        abandon.Should().Throw<DomainException>();
        execution.Status.Should().Be(TourExecutionStatus.Completed);
        execution.AbandonedAt.Should().BeNull();
    }

    [Fact]
    public void Abandoned_session_cannot_be_abandoned_again()
    {
        var execution = NewSession();
        var abandonedAt = DateTime.UtcNow.AddMinutes(-5);
        execution.Abandon(abandonedAt);

        var abandon = () => execution.Abandon(DateTime.UtcNow);

        abandon.Should().Throw<DomainException>();
        execution.AbandonedAt.Should().Be(abandonedAt);
    }

    [Fact]
    public void Abandoning_a_session_keeps_its_reached_key_points()
    {
        var keyPointId = Guid.NewGuid();
        var execution = NewSession();
        execution.ReachKeyPoint(keyPointId, false, DateTime.UtcNow);

        execution.Abandon(DateTime.UtcNow);

        execution.ReachedKeyPoints.Should().ContainSingle(reached => reached.KeyPointId == keyPointId);
    }

    [Fact]
    public void Abandoned_session_cannot_reach_key_points_any_more()
    {
        var execution = NewSession();
        execution.Abandon(DateTime.UtcNow);

        var reach = () => execution.ReachKeyPoint(Guid.NewGuid(), false, DateTime.UtcNow);

        reach.Should().Throw<DomainException>();
        execution.ReachedKeyPoints.Should().BeEmpty();
    }

    private static TourExecution NewSession()
    {
        return new TourExecution(Guid.NewGuid(), Guid.NewGuid(), new Location(45.25, 19.85), DateTime.UtcNow);
    }
}
