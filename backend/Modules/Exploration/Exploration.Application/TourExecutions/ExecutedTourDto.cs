namespace Exploration.Application.TourExecutions;

// The tour data a session's screens show, before the session adds what it has reached.
public sealed record ExecutedTourDto(Guid TourId, string TourName, List<ExecutedKeyPointDto> KeyPoints);
