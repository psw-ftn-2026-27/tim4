using Exploration.Domain.LocationSearchRecords;
using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.LocationSearchRecords;

public class LocationSearchRecordTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Record_with_a_radius_not_greater_than_zero_is_rejected(double radiusInKm)
    {
        var creation = () => CreateRecord(radiusInKm, 3);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Record_of_a_search_that_found_no_tour_is_made()
    {
        var record = CreateRecord(10, 0);

        record.ResultCount.Should().Be(0);
    }

    [Fact]
    public void Record_with_a_negative_number_of_found_tours_is_rejected()
    {
        var creation = () => CreateRecord(10, -1);

        creation.Should().Throw<DomainException>();
    }

    private static LocationSearchRecord CreateRecord(double radiusInKm, int resultCount)
    {
        var criteria = new LocationSearchCriteria(null, null, null, null, null, null, null, null);
        return new LocationSearchRecord(Guid.NewGuid(), new Location(45.25, 19.85), radiusInKm, criteria,
            resultCount, DateTime.UtcNow);
    }
}
