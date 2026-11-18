using Exploration.Domain.Tours;

namespace Exploration.Domain.LocationSearchRecords;

// The criteria of a location search other than the circle. Unlike a preference, a search may
// leave every one of them out.
public sealed record LocationSearchCriteria
{
    public string? Name { get; private init; }
    public List<TourDifficulty> Difficulties { get; private init; }
    public List<string> Tags { get; private init; }
    public TransportMode? Transport { get; private init; }
    public long? PriceFrom { get; private init; }
    public long? PriceTo { get; private init; }
    public double? LengthFrom { get; private init; }
    public double? LengthTo { get; private init; }

    private LocationSearchCriteria()
    {
        Difficulties = null!;
        Tags = null!;
    }

    public LocationSearchCriteria(string? name, List<TourDifficulty>? difficulties, List<string>? tags,
        TransportMode? transport, long? priceFrom, long? priceTo, double? lengthFrom, double? lengthTo)
    {
        Name = name;
        Difficulties = difficulties ?? [];
        Tags = tags ?? [];
        Transport = transport;
        PriceFrom = priceFrom;
        PriceTo = priceTo;
        LengthFrom = lengthFrom;
        LengthTo = lengthTo;
    }
}
