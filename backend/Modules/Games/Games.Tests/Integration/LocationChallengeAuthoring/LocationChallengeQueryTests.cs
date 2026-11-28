using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Games.Application.LocationChallengeAuthoring;
using Games.Domain.LocationChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengeAuthoring;

public sealed class LocationChallengeQueryTests : BaseIntegrationTest
{
    private const string Address = "/api/games/location-challenges";
    private readonly LocationChallenge _challenge = new("Izazov", new ChallengeLocation(45.25, 19.85), 25,
        [new HiddenLocation(new ChallengeLocation(45.26789, 19.86789), "/images/secret.png")]);

    public LocationChallengeQueryTests(GamesApiFactory factory) : base(factory)
    {
        Factory.Reseed<GamesDbContext>([.. GamesSeed.All, _challenge]);
    }

    [Fact]
    public async Task Explorer_sees_start_locations_without_hidden_point_data()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        using var response = await client.GetAsync(Address);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var challenge = body.GetProperty("items").EnumerateArray()
            .Should().ContainSingle(item => item.GetProperty("id").GetGuid() == _challenge.Id).Which;
        challenge.GetProperty("startLatitude").GetDouble().Should().Be(45.25);
        challenge.GetProperty("pointCount").GetInt32().Should().Be(1);
        challenge.GetRawText().Should().NotContain("45.26789").And.NotContain("19.86789")
            .And.NotContain("secret.png").And.NotContain(_challenge.HiddenLocations[0].PointId.ToString());
    }

    [Fact]
    public async Task Anonymous_visitor_cannot_browse_location_challenges()
    {
        using var client = Factory.CreateClient();

        using var response = await client.GetAsync(Address);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData(false, HttpStatusCode.Unauthorized)]
    [InlineData(true, HttpStatusCode.Forbidden)]
    public async Task Only_an_administrator_can_access_hidden_coordinates_and_photo_addresses(
        bool signedIn, HttpStatusCode expected)
    {
        using var client = signedIn
            ? Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer")
            : Factory.CreateClient();

        using var response = await client.GetAsync($"{Address}/{_challenge.Id}/admin");

        response.StatusCode.Should().Be(expected);
    }

    [Fact]
    public async Task Administrator_sees_the_coordinates_and_photo_of_each_hidden_point()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        using var response = await client.GetAsync($"{Address}/{_challenge.Id}/admin");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var details = await response.Content.ReadFromJsonAsync<LocationChallengeDetailsDto>(JsonOptions);
        details!.Points.Should().ContainSingle().Which.Should().Be(
            new HiddenLocationDto(_challenge.HiddenLocations[0].PointId, 45.26789, 19.86789, "/images/secret.png"));
    }
}
