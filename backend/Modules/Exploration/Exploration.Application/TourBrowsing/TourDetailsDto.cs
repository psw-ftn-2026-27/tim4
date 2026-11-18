using Exploration.Application.Tours;
using Exploration.Domain.Tours;

namespace Exploration.Application.TourBrowsing;

public sealed record TourDetailsDto(
    Guid Id,
    Guid AuthorId,
    string Name,
    string Description,
    TourDifficulty Difficulty,
    List<string> Tags,
    double LengthInKm,
    List<TransportTimeDto> TransportTimes,
    List<TourKeyPointDto> KeyPoints);
