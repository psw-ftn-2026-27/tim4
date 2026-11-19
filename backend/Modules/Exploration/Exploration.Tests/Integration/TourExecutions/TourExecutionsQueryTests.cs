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
    public async Task Active_tour_holds_the_tour_name_its_key_points_and_the_first_key_point_as_the_next_one()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.PublishedVineyardsByBicycleEasy;

        var active = await client.GetFromJsonAsync<ActiveTourDto>("/api/exploration/tour-executions/active", JsonOptions);

        active!.ExecutionId.Should().Be(TourExecutionSeed.ExplorersVineyardsByBicycleInProgress.Id);
        active.TourName.Should().Be(tour.Name);
        active.KeyPoints.Select(keyPoint => keyPoint.Id).Should().Equal(tour.KeyPoints.Select(keyPoint => keyPoint.Id));
        active.NextKeyPointId.Should().Be(tour.KeyPoints[0].Id);
        active.ReachedKeyPointCount.Should().Be(0);
    }

    [Fact]
    public async Task Tourist_without_a_tour_in_progress_gets_no_active_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync("/api/exploration/tour-executions/active");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
