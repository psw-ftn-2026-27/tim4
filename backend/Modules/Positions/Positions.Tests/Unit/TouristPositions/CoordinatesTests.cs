using FluentAssertions;
using Positions.Domain.TouristPositions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Positions.Tests.Unit.TouristPositions;

public sealed class CoordinatesTests
{
    [Theory]
    [InlineData(-90.001, 0)]
    [InlineData(90.001, 0)]
    [InlineData(0, -180.001)]
    [InlineData(0, 180.001)]
    public void Coordinates_with_latitude_or_longitude_out_of_range_are_rejected(double latitude, double longitude)
    {
        var creation = () => new Coordinates(latitude, longitude);

        creation.Should().Throw<DomainException>();
    }
}
