namespace Exploration.Application.TourExecutions;

// The tour data the active tour screen shows, before the session adds its next key point.
public sealed record ExecutedTourDto(Guid TourId, string TourName, List<ActiveKeyPointDto> KeyPoints);
