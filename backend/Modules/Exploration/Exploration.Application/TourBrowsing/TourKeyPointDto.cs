namespace Exploration.Application.TourBrowsing;

public sealed record TourKeyPointDto(
    Guid Id,
    string Name,
    string Description,
    string ImageAddress,
    string? Secret,
    double Latitude,
    double Longitude,
    bool IsVisibleWithoutPurchase);
