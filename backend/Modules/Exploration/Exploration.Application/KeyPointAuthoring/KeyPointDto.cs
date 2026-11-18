namespace Exploration.Application.KeyPointAuthoring;

public sealed record KeyPointDto(
    Guid Id,
    string Name,
    string Description,
    string ImageAddress,
    string? Secret,
    double Latitude,
    double Longitude,
    bool IsVisibleWithoutPurchase);
