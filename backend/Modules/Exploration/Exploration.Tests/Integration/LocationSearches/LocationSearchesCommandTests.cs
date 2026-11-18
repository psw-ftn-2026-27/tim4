using System.Net;
using System.Net.Http.Json;
using Exploration.Application.LocationSearches;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.LocationSearches;

public class LocationSearchesCommandTests : BaseIntegrationTest
{
    public LocationSearchesCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Location_search_is_recorded_with_its_circle_criteria_tourist_and_the_number_of_tours_the_server_found()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var center = TourSeed.PublishedMonasteries.KeyPoints[1].Location;
        var request = new RecordLocationSearchDto(center.Latitude, center.Longitude, 1, TourSeed.PublishedMonasteries.Name,
            [TourDifficulty.Hard], ["istorija"], TransportMode.Car, 0, 10, 5, 10);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var recordIdsBefore = arrangeContext.LocationSearchRecords.Select(record => record.Id).ToList();

        var response = await client.PostAsJsonAsync("/api/exploration/location-searches", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        var stored = assertContext.LocationSearchRecords.Single(record => !recordIdsBefore.Contains(record.Id));
        stored.TouristId.Should().Be(WellKnownUsers.Explorer);
        stored.Center.Should().Be(center);
        stored.RadiusInKm.Should().Be(1);
        stored.Criteria.Name.Should().Be(TourSeed.PublishedMonasteries.Name);
        stored.Criteria.Difficulties.Should().Equal(TourDifficulty.Hard);
        stored.Criteria.Tags.Should().Equal("istorija");
        stored.Criteria.Transport.Should().Be(TransportMode.Car);
        stored.Criteria.PriceFrom.Should().Be(0);
        stored.Criteria.PriceTo.Should().Be(10);
        stored.Criteria.LengthFrom.Should().Be(5);
        stored.Criteria.LengthTo.Should().Be(10);
        stored.ResultCount.Should().Be(1);
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
            null, null, null, null);
    }
}
