using Exploration.Domain.Tours;

namespace Exploration.Application.TourBrowsing;

public sealed record PublishedTourSearchDto(
    string? Name,
    TourDifficulty? Difficulty,
    List<string>? Tags,
    TransportMode? Transport,
    DifficultySort? SortByDifficulty);
