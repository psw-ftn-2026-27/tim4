using Exploration.Domain.Tours;

namespace Exploration.Application.TourBrowsing;

public sealed record PublishedTourSearchDto(
    string? Name,
    List<TourDifficulty>? Difficulties,
    List<string>? Tags,
    TransportMode? Transport,
    long? PriceFrom,
    long? PriceTo,
    TourSort? Sort);
