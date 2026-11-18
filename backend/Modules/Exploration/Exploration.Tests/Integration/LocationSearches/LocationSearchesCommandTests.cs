using System.Net;
using System.Net.Http.Json;
using Exploration.Application.LocationSearches;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.LocationSearches;

public class LocationSearchesCommandTests : BaseIntegrationTest
{
    public LocationSearchesCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Location_search_is_recorded_with_its_circle_criteria_total_and_tourist()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new RecordLocationSearchDto(45.25, 19.85, 12.5, "Petrovaradin",
            [TourDifficulty.Easy, TourDifficulty.Moderate], ["istorija"], TransportMode.Walking,
            100, 900, 2, 15, 7);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var recordCountBefore = arrangeContext.LocationSearchRecords.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/location-searches", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.LocationSearchRecords.Count().Should().Be(recordCountBefore + 1);
        var stored = assertContext.LocationSearchRecords.Single();
        stored.TouristId.Should().Be(WellKnownUsers.Explorer);
        stored.Center.Should().Be(new Location(45.25, 19.85));
        stored.RadiusKm.Should().Be(12.5);
        stored.Criteria.Name.Should().Be("Petrovaradin");
        stored.Criteria.Difficulties.Should().Equal(TourDifficulty.Easy, TourDifficulty.Moderate);
        stored.Criteria.Tags.Should().Equal("istorija");
        stored.Criteria.Transport.Should().Be(TransportMode.Walking);
        stored.Criteria.PriceFrom.Should().Be(100);
        stored.Criteria.PriceTo.Should().Be(900);
        stored.Criteria.LengthFrom.Should().Be(2);
        stored.Criteria.LengthTo.Should().Be(15);
        stored.ResultCount.Should().Be(7);
    }

    [Fact]
    public async Task Location_search_with_a_zero_radius_is_rejected_and_not_recorded()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = CircleOnlySearch(0);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var recordCountBefore = arrangeContext.LocationSearchRecords.Count();

        var response = await client.PostAsJsonAsync("/api/exploration/location-searches", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.LocationSearchRecords.Count().Should().Be(recordCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_record_a_location_search()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/exploration/location-searches", CircleOnlySearch(10));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_record_a_location_search()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync("/api/exploration/location-searches", CircleOnlySearch(10));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static RecordLocationSearchDto CircleOnlySearch(double radiusInKm)
    {
        return new RecordLocationSearchDto(45.25, 19.85, radiusInKm, null, null, null, null,
            null, null, null, null, 3);
    }
}
