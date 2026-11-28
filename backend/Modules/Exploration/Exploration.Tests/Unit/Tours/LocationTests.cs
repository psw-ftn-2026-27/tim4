using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class LocationTests
{
    [Theory]
    [InlineData(-90.1, 19.8452)]
    [InlineData(90.1, 19.8452)]
    [InlineData(45.2551, -180.1)]
    [InlineData(45.2551, 180.1)]
    [InlineData(double.NaN, 19.8452)]
    [InlineData(double.PositiveInfinity, 19.8452)]
    [InlineData(double.NegativeInfinity, 19.8452)]
    [InlineData(45.2551, double.NaN)]
    [InlineData(45.2551, double.PositiveInfinity)]
    [InlineData(45.2551, double.NegativeInfinity)]
    public void Location_must_be_a_point_on_the_globe(double latitude, double longitude)
    {
        var creation = () => new Location(latitude, longitude);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(-90, 19.8452)]
    [InlineData(90, 19.8452)]
    [InlineData(45.2551, -180)]
    [InlineData(45.2551, 180)]
    public void Location_can_lie_on_the_edge_of_the_allowed_range(double latitude, double longitude)
    {
        var location = new Location(latitude, longitude);

        location.Latitude.Should().Be(latitude);
        location.Longitude.Should().Be(longitude);
    }

    [Fact]
    public void Distance_from_a_location_to_itself_is_zero()
    {
        var location = new Location(45.2551, 19.8452);

        var distance = location.DistanceInKm(new Location(45.2551, 19.8452));

        distance.Should().Be(0);
    }

    [Theory]
    [InlineData(45, 19, 46, 19)]
    [InlineData(46, 19, 45, 19)]
    [InlineData(0, 0, 0, 1)]
    public void One_degree_along_a_meridian_or_the_equator_is_about_111_km(double fromLatitude, double fromLongitude, double toLatitude, double toLongitude)
    {
        var from = new Location(fromLatitude, fromLongitude);

        var distance = from.DistanceInKm(new Location(toLatitude, toLongitude));

        distance.Should().BeApproximately(111.195, 0.001);
    }

    [Fact]
    public void One_degree_of_longitude_is_shorter_away_from_the_equator()
    {
        var atEquator = new Location(0, 19).DistanceInKm(new Location(0, 20));

        var atNoviSad = new Location(45.2551, 19).DistanceInKm(new Location(45.2551, 20));

        atNoviSad.Should().BeLessThan(atEquator);
    }
}
