namespace Exploration.Application.TourExecutions;

// A key point as the queries read it, secret included. It never leaves the application layer:
// the queries pass the secret on only for a key point the session has reached.
public sealed record ExecutedKeyPointDto(Guid Id, string Name, double Latitude, double Longitude, string? Secret);
