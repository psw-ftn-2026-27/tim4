namespace Exploration.Application.TourExecutions;

// The tourist's session in progress. ReachedKeyPointCount and KeyPoints.Count make the progress.
public sealed record ActiveTourDto(
    Guid ExecutionId,
    Guid TourId,
    string TourName,
    List<ActiveKeyPointDto> KeyPoints,
    Guid NextKeyPointId,
    int ReachedKeyPointCount,
    List<ReachedKeyPointDto> ReachedKeyPoints);
