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
        var stored = assertContext.TourExecutions.Single(execution => execution.TouristId == WellKnownUsers.SecondExplorer);
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
}
