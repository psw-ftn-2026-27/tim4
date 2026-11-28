using Exploration.Domain.Tours;

namespace Exploration.Application.TourBrowsing;

// Latitude, Longitude, and RadiusInKm describe a circle on the map; it applies only when all
// three are given.
public sealed record PublishedTourSearchDto(
    string? Name,
    List<TourDifficulty>? Difficulties,
    List<string>? Tags,
    TransportMode? Transport,
    long? PriceFrom,
    long? PriceTo,
    double? LengthFrom,
    double? LengthTo,
    TourSort? Sort,
    double? Latitude,
    double? Longitude,
    double? RadiusInKm);
