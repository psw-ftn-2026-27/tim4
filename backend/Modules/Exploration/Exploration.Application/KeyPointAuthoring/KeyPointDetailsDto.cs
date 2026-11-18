namespace Exploration.Application.KeyPointAuthoring;

public sealed record KeyPointDetailsDto(
    string Name,
    string Description,
    string? Secret,
    double Latitude,
    double Longitude,
    bool IsVisibleWithoutPurchase,
    double TourLengthInKm);
