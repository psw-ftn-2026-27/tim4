using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourExecutions;
using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourExecutions;

public class TourExecutionsCommandTests : BaseIntegrationTest
{
    public TourExecutionsCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Tourist_who_bought_a_tour_starts_it_and_a_session_in_progress_is_stored_with_its_start_time_and_location()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var tour = TourSeed.PublishedMonasteries;
        var startedAfter = DateTime.UtcNow;

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions", new StartTourDto(tour.Id, 45.12, 19.94));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.TourExecutions.Single(execution => execution.TouristId == WellKnownUsers.SecondExplorer
            && execution.Status == TourExecutionStatus.InProgress);
        stored.TourId.Should().Be(tour.Id);
        stored.Status.Should().Be(TourExecutionStatus.InProgress);
        stored.StartLocation.Should().Be(new Location(45.12, 19.94));
        stored.StartedAt.Should().BeOnOrAfter(startedAfter);
    }

    [Fact]
    public async Task Tourist_who_has_not_bought_a_tour_cannot_start_it_and_no_session_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var executionCountBefore = arrangeContext.TourExecutions.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions", new StartTourDto(TourSeed.PublishedVineyards.Id, 45.2, 19.93));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourExecutions.Count().Should().Be(executionCountBefore);
    }

    [Fact]
    public async Task Starting_a_tour_the_tourist_already_completed_stores_a_new_session_and_keeps_the_completed_one()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var completed = TourExecutionSeed.SecondExplorersMonasteriesCompleted;
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var executionCountBefore = arrangeContext.TourExecutions.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions", new StartTourDto(completed.TourId, 45.12, 19.94));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourExecutions.Count().Should().Be(executionCountBefore + 1);
        var started = assertContext.TourExecutions.Single(execution => execution.TouristId == WellKnownUsers.SecondExplorer
            && execution.Status == TourExecutionStatus.InProgress);
        started.TourId.Should().Be(completed.TourId);
        started.ReachedKeyPoints.Should().BeEmpty();
        var stored = assertContext.TourExecutions.Single(execution => execution.Id == completed.Id);
        stored.Status.Should().Be(TourExecutionStatus.Completed);
        stored.ReachedKeyPoints.Should().HaveCount(completed.ReachedKeyPointCount);
    }

    [Fact]
    public async Task Starting_a_tour_whose_session_is_in_progress_continues_it_and_stores_no_new_session()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var inProgress = TourExecutionSeed.ExplorersVineyardsByBicycleWithFirstKeyPointReached;
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var executionCountBefore = arrangeContext.TourExecutions.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions", new StartTourDto(inProgress.TourId, 45.21, 19.94));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourExecutions.Count().Should().Be(executionCountBefore);
        var stored = assertContext.TourExecutions.Single(execution => execution.Id == inProgress.Id);
        stored.Status.Should().Be(TourExecutionStatus.InProgress);
        stored.LastActivityAt.Should().BeAfter(inProgress.LastActivityAt);
    }

    [Fact]
    public async Task Tourist_cannot_start_a_tour_that_does_not_exist_and_no_session_is_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var executionCountBefore = arrangeContext.TourExecutions.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions", new StartTourDto(Guid.NewGuid(), 45.12, 19.94));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourExecutions.Count().Should().Be(executionCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_start_a_tour()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions", new StartTourDto(TourSeed.PublishedMonasteries.Id, 45.12, 19.94));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_start_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions", new StartTourDto(TourSeed.PublishedMonasteries.Id, 45.12, 19.94));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Tourist_who_arrives_at_the_next_key_point_reaches_it_and_it_is_stored_with_the_time_it_was_reached()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var execution = TourExecutionSeed.ExplorersVineyardsByBicycleWithFirstKeyPointReached;
        var nextKeyPoint = TourSeed.PublishedVineyardsByBicycleEasy.KeyPoints[1];
        var checkedAfter = DateTime.UtcNow;

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions/active/position-checks",
            new CheckPositionDto(nextKeyPoint.Location.Latitude, nextKeyPoint.Location.Longitude));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PositionCheckResultDto>(JsonOptions);
        result!.Outcome.Should().Be(PositionCheckOutcome.ReachedKeyPoint);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.TourExecutions.Single(stored => stored.Id == execution.Id);
        stored.ReachedKeyPoints.Should().ContainSingle(reached => reached.KeyPointId == nextKeyPoint.Id)
            .Which.ReachedAt.Should().BeOnOrAfter(checkedAfter);
    }

    [Fact]
    public async Task Tourist_without_a_tour_in_progress_cannot_check_their_position_and_no_session_changes()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var executionCountBefore = arrangeContext.TourExecutions.Count(execution => execution.TouristId == WellKnownUsers.SecondExplorer);

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions/active/position-checks", new CheckPositionDto(45.121, 19.944));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourExecutions.Count(execution => execution.TouristId == WellKnownUsers.SecondExplorer).Should().Be(executionCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_check_a_position()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions/active/position-checks", new CheckPositionDto(45.199, 19.93));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_check_a_position()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync("/api/exploration/tour-executions/active/position-checks", new CheckPositionDto(45.199, 19.93));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Tourist_abandons_the_tour_in_progress_and_the_stored_session_is_abandoned_with_its_time_and_the_same_reached_key_points()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var inProgress = TourExecutionSeed.ExplorersVineyardsByBicycleWithFirstKeyPointReached;
        var abandonedAfter = DateTime.UtcNow;

        var response = await client.PostAsync("/api/exploration/tour-executions/active/abandonment", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.TourExecutions.Single(execution => execution.Id == inProgress.Id);
        stored.Status.Should().Be(TourExecutionStatus.Abandoned);
        stored.AbandonedAt.Should().BeOnOrAfter(abandonedAfter);
        stored.ReachedKeyPoints.Select(reached => reached.KeyPointId).Should()
            .Equal(inProgress.ReachedKeyPoints.Select(reached => reached.KeyPointId));
    }

    [Fact]
    public async Task Tourist_without_a_tour_in_progress_cannot_abandon_a_tour_and_no_session_changes()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var completed = TourExecutionSeed.SecondExplorersMonasteriesCompleted;
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var executionCountBefore = arrangeContext.TourExecutions.Count(execution => execution.TouristId == WellKnownUsers.SecondExplorer);

        var response = await client.PostAsync("/api/exploration/tour-executions/active/abandonment", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourExecutions.Count(execution => execution.TouristId == WellKnownUsers.SecondExplorer).Should().Be(executionCountBefore);
        var stored = assertContext.TourExecutions.Single(execution => execution.Id == completed.Id);
        stored.Status.Should().Be(TourExecutionStatus.Completed);
        stored.AbandonedAt.Should().BeNull();
    }

    [Fact]
    public async Task Anonymous_user_cannot_abandon_a_tour()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync("/api/exploration/tour-executions/active/abandonment", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_abandon_a_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync("/api/exploration/tour-executions/active/abandonment", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
