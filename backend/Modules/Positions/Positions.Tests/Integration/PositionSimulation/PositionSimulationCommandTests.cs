using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Positions.Application.PositionSimulation;
using Positions.Infrastructure.Persistence;
using Positions.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Positions.Tests.Integration.PositionSimulation;

public class PositionSimulationCommandTests : BaseIntegrationTest
{
    public PositionSimulationCommandTests(PositionsApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Tourist_without_a_position_sets_one()
    {
        var touristId = Guid.NewGuid();
        var client = Factory.CreateClientFor(touristId, "explorer");
        var request = new CoordinatesDto(45.2551, 19.8451);
        using var arrangeContext = Factory.CreateContext<PositionsDbContext>();
        var positionCountBefore = arrangeContext.TouristPositions.Count();

        var response = await client.PutAsJsonAsync("/api/positions/mine", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PositionsDbContext>();
        assertContext.TouristPositions.Count().Should().Be(positionCountBefore + 1);
        var stored = assertContext.TouristPositions.Single(position => position.TouristId == touristId);
        stored.Coordinates.Latitude.Should().Be(45.2551);
        stored.Coordinates.Longitude.Should().Be(19.8451);
    }

    [Fact]
    public async Task Tourist_with_a_position_moves_it_instead_of_adding_another()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new CoordinatesDto(43.3209, 21.8958);
        using var arrangeContext = Factory.CreateContext<PositionsDbContext>();
        var positionCountBefore = arrangeContext.TouristPositions.Count();

        var response = await client.PutAsJsonAsync("/api/positions/mine", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PositionsDbContext>();
        assertContext.TouristPositions.Count().Should().Be(positionCountBefore);
        var stored = assertContext.TouristPositions.Single(position => position.Id == TouristPositionSeed.ExplorersPosition.Id);
        stored.Coordinates.Latitude.Should().Be(43.3209);
        stored.Coordinates.Longitude.Should().Be(21.8958);
    }

    [Fact]
    public async Task Position_out_of_range_is_rejected_and_nothing_is_stored()
    {
        var touristId = Guid.NewGuid();
        var client = Factory.CreateClientFor(touristId, "explorer");
        var request = new CoordinatesDto(45.2551, 200);
        using var arrangeContext = Factory.CreateContext<PositionsDbContext>();
        var positionCountBefore = arrangeContext.TouristPositions.Count();

        var response = await client.PutAsJsonAsync("/api/positions/mine", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<PositionsDbContext>();
        assertContext.TouristPositions.Count().Should().Be(positionCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_set_a_position()
    {
        var client = Factory.CreateClient();
        var request = new CoordinatesDto(45.2551, 19.8451);

        var response = await client.PutAsJsonAsync("/api/positions/mine", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_set_a_position()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new CoordinatesDto(45.2551, 19.8451);

        var response = await client.PutAsJsonAsync("/api/positions/mine", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
