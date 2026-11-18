using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Positions.Application.PositionSimulation;
using Positions.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Positions.Tests.Integration.PositionSimulation;

public class PositionSimulationQueryTests : BaseIntegrationTest
{
    public PositionSimulationQueryTests(PositionsApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Tourist_gets_their_own_position_and_not_another_tourists_one()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var coordinates = await client.GetFromJsonAsync<CoordinatesDto>("/api/positions/mine", JsonOptions);

        var expected = TouristPositionSeed.SecondExplorersPosition.Coordinates;
        coordinates!.Latitude.Should().Be(expected.Latitude);
        coordinates.Longitude.Should().Be(expected.Longitude);
    }

    [Fact]
    public async Task Tourist_without_a_position_gets_an_empty_response()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.GetAsync("/api/positions/mine");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await response.Content.ReadAsStringAsync()).Should().BeEmpty();
    }
}
