using Exploration.Domain.Tours;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.LocationSearchRecords;

// A record of one location search a tourist ran on the map. Once made it never changes, so the
// aggregate has no method that changes its state.
public sealed class LocationSearchRecord : AggregateRoot
{
    public Guid TouristId { get; private set; }
    public Location Center { get; private set; }
    public double RadiusInKm { get; private set; }
    public LocationSearchCriteria Criteria { get; private set; }
    public int ResultCount { get; private set; }
    public DateTime SearchedAt { get; private set; }

    private LocationSearchRecord()
    {
        Center = null!;
        Criteria = null!;
    }

    public LocationSearchRecord(Guid touristId, Location center, double radiusInKm,
        LocationSearchCriteria criteria, int resultCount, DateTime searchedAt)
        : base(Guid.NewGuid())
    {
        if (!double.IsFinite(radiusInKm) || radiusInKm <= 0)
        {
            throw new DomainException("A location search requires a radius greater than zero.");
        }
        // A search that found no tour is still recorded, so zero is allowed.
        if (resultCount < 0)
        {
            throw new DomainException("A location search cannot find a negative number of tours.");
        }

        TouristId = touristId;
        Center = center;
        RadiusInKm = radiusInKm;
        Criteria = criteria;
        ResultCount = resultCount;
        SearchedAt = searchedAt;
    }
}
