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
}
