using Exploration.Domain.Tours;

namespace Exploration.Application.LocationSearches;

// The circle and the other criteria of a search the client ran. The number of tours found is
// not sent: the server runs the search again and counts them itself.
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
    double? LengthTo);
