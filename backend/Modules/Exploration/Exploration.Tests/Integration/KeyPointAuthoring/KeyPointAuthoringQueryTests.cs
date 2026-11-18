using System.Net;
using System.Net.Http.Json;
using Exploration.Application.KeyPointAuthoring;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.KeyPointAuthoring;

public class KeyPointAuthoringQueryTests : BaseIntegrationTest
{
    public KeyPointAuthoringQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Author_sees_the_key_points_of_their_tour_in_order_with_their_secrets()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.DraftWithKeyPoints;

        var response = await client.GetAsync($"/api/exploration/tours/mine/{tour.Id}/key-points");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var keyPoints = (await response.Content.ReadFromJsonAsync<List<KeyPointDto>>(JsonOptions))!;
        keyPoints.Select(point => point.Id).Should().Equal(tour.KeyPoints.Select(point => point.Id));
        keyPoints.Select(point => point.Secret).Should().Equal(tour.KeyPoints.Select(point => point.Secret));
    }

    [Fact]
    public async Task Explorer_cannot_see_the_key_points_of_another_authors_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync($"/api/exploration/tours/mine/{TourSeed.DraftWithKeyPoints.Id}/key-points");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
