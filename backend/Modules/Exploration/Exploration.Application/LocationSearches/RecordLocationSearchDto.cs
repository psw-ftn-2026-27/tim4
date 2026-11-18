using Exploration.Domain.Tours;

namespace Exploration.Application.LocationSearches;

// The circle and the other criteria of a search the client ran, plus ResultCount: the total
// number of tours the search found, taken from the search response, not the size of the shown page.
public sealed record RecordLocationSearchDto(
    double Latitude,
    double Longitude,
    double RadiusInKm,
    string? Name,
    List<TourDifficulty>? Difficulties,
    List<string>? Tags,
    TransportMode? Transport,
    long? PriceFrom,
    long? PriceTo,
    double? LengthFrom,
    double? LengthTo,
    int ResultCount);
