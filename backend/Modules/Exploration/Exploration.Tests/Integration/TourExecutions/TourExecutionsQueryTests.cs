using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourExecutions;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourExecutions;

public class TourExecutionsQueryTests : BaseIntegrationTest
{
    public TourExecutionsQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Active_tour_holds_the_tour_name_its_key_points_the_progress_and_the_first_unreached_key_point_as_the_next_one()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var execution = TourExecutionSeed.ExplorersVineyardsByBicycleWithFirstKeyPointReached;
        var tour = TourSeed.PublishedVineyardsByBicycleEasy;

        var active = await client.GetFromJsonAsync<ActiveTourDto>("/api/exploration/tour-executions/active", JsonOptions);

        active!.ExecutionId.Should().Be(execution.Id);
        active.TourName.Should().Be(tour.Name);
        active.KeyPoints.Select(keyPoint => keyPoint.Id).Should().Equal(tour.KeyPoints.Select(keyPoint => keyPoint.Id));
        active.NextKeyPointId.Should().Be(tour.KeyPoints[1].Id);
        active.ReachedKeyPointCount.Should().Be(execution.ReachedKeyPointCount);
    }

    [Fact]
    public async Task Active_tour_reveals_the_secrets_of_the_reached_key_points_only()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.PublishedVineyardsByBicycleEasy;
        var reachedKeyPoint = tour.KeyPoints[0];
        var unreachedKeyPoints = tour.KeyPoints.Skip(1).ToList();

        var active = await client.GetFromJsonAsync<ActiveTourDto>("/api/exploration/tour-executions/active", JsonOptions);

        active!.ReachedKeyPoints.Should().Contain(new ReachedKeyPointDto(reachedKeyPoint.Id, reachedKeyPoint.Name, reachedKeyPoint.Secret));
        active.ReachedKeyPoints.Select(keyPoint => keyPoint.Id).Should().NotIntersectWith(unreachedKeyPoints.Select(keyPoint => keyPoint.Id));
        active.ReachedKeyPoints.Select(keyPoint => keyPoint.Secret).Should().NotIntersectWith(unreachedKeyPoints.Select(keyPoint => keyPoint.Secret));
    }

    [Fact]
    public async Task Tourist_cannot_read_another_tourists_session()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var othersExecution = TourExecutionSeed.ExplorersVineyardsByBicycleWithFirstKeyPointReached;

        var response = await client.GetAsync($"/api/exploration/tour-executions/{othersExecution.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Tourist_without_a_tour_in_progress_gets_no_active_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync("/api/exploration/tour-executions/active");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
