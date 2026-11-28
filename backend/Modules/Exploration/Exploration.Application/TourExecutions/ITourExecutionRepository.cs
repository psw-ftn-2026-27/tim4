using Exploration.Domain.TourExecutions;

namespace Exploration.Application.TourExecutions;

public interface ITourExecutionRepository
{
    // Null when the tourist has no tour in progress.
    Task<TourExecution?> GetInProgressByTouristAsync(Guid touristId);
    // Null when the session does not exist or belongs to another tourist.
    Task<TourExecution?> GetByTouristAsync(Guid touristId, Guid executionId);
    void Add(TourExecution execution);
}
