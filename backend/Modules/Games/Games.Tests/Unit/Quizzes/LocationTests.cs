using FluentAssertions;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.Quizzes;

public class LocationTests
{
    [Theory]
    [InlineData(-90.0001, 19.8628)]
    [InlineData(90.0001, 19.8628)]
    [InlineData(45.2517, -180.0001)]
    [InlineData(45.2517, 180.0001)]
    public void Location_outside_the_latitude_or_longitude_range_cannot_exist(double latitude, double longitude)
    {
        var creation = () => new Location(latitude, longitude);

        creation.Should().Throw<DomainException>();
    }
}
