using Shared.Domain.Exceptions;

namespace Exploration.Application.TourExecutions;

public sealed class TourExecutionsQueries
{
    private readonly ITourExecutionRepository _executionRepository;
    private readonly ITourExecutionReadRepository _executionReadRepository;

    public TourExecutionsQueries(ITourExecutionRepository executionRepository,
        ITourExecutionReadRepository executionReadRepository)
    {
        _executionRepository = executionRepository;
        _executionReadRepository = executionReadRepository;
    }

    // Null when the tourist has no tour in progress.
    public async Task<ActiveTourDto?> GetActiveAsync(Guid touristId)
    {
        var execution = await _executionRepository.GetInProgressByTouristAsync(touristId);
        if (execution is null)
        {
            return null;
        }

        var tour = await _executionReadRepository.GetExecutedTourAsync(execution.TourId)
            ?? throw new NotFoundException($"Tour {execution.TourId} does not exist.");
        var nextKeyPointId = execution.NextKeyPointId(tour.KeyPoints.Select(keyPoint => keyPoint.Id).ToList());
        // The next key point is the first one not reached, so every key point before it is reached.
        var reachedCount = tour.KeyPoints.FindIndex(keyPoint => keyPoint.Id == nextKeyPointId);
        return new ActiveTourDto(execution.Id, tour.TourId, tour.TourName, tour.KeyPoints, nextKeyPointId, reachedCount);
    }
}
