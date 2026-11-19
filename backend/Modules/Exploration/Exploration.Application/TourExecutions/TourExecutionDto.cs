using Exploration.Domain.TourExecutions;

namespace Exploration.Application.TourExecutions;

// One of the tourist's sessions, in any status.
public sealed record TourExecutionDto(
    Guid ExecutionId,
    string TourName,
    TourExecutionStatus Status,
    List<ReachedKeyPointDto> ReachedKeyPoints);
