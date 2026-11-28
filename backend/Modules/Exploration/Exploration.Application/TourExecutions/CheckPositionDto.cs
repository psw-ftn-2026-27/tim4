namespace Exploration.Application.TourExecutions;

// Latitude and Longitude are the tourist's position as the client has it; the server does not
// read the stored position.
public sealed record CheckPositionDto(double Latitude, double Longitude);
