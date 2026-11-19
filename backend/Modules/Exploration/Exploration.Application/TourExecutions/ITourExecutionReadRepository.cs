namespace Exploration.Application.TourExecutions;

public interface ITourExecutionReadRepository
{
    // Null when the tour does not exist. Holds every key point, in tour order.
    Task<ExecutedTourDto?> GetExecutedTourAsync(Guid tourId);
}
