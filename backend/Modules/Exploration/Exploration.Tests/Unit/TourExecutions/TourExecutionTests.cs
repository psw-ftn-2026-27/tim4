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
}
