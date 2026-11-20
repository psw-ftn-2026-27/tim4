namespace Exploration.Application.TourExecutions;

public interface ITourExecutionReadRepository
{
    // Null when the tour does not exist. Holds every key point, in tour order.
    Task<ExecutedTourDto?> GetExecutedTourAsync(Guid tourId);
    // The tour of the tourist's session in progress; null when the tourist has no tour in progress.
    Task<Guid?> GetInProgressTourIdAsync(Guid touristId);
}
