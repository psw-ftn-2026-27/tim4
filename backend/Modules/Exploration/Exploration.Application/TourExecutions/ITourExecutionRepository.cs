using Exploration.Domain.TourExecutions;

namespace Exploration.Application.TourExecutions;

public interface ITourExecutionRepository
{
    // Null when the tourist has no tour in progress.
    Task<TourExecution?> GetInProgressByTouristAsync(Guid touristId);
    void Add(TourExecution execution);
}
