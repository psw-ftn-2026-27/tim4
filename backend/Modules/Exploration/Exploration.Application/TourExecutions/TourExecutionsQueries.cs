using Exploration.Domain.TourExecutions;
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

        var tour = await GetExecutedTourAsync(execution);
        var keyPoints = tour.KeyPoints
            .Select(keyPoint => new ActiveKeyPointDto(keyPoint.Id, keyPoint.Name, keyPoint.Latitude, keyPoint.Longitude))
            .ToList();
        var nextKeyPointId = execution.NextKeyPointId(keyPoints.Select(keyPoint => keyPoint.Id).ToList());
        return new ActiveTourDto(execution.Id, tour.TourId, tour.TourName, keyPoints, nextKeyPointId,
            execution.ReachedKeyPointCount, ReachedKeyPoints(execution, tour));
    }

    public async Task<TourExecutionDto> GetExecutionAsync(Guid touristId, Guid executionId)
    {
        var execution = await _executionRepository.GetByTouristAsync(touristId, executionId)
            ?? throw new NotFoundException($"Tour execution {executionId} does not exist.");

        var tour = await GetExecutedTourAsync(execution);
        return new TourExecutionDto(execution.Id, tour.TourName, execution.Status, ReachedKeyPoints(execution, tour));
    }

    private async Task<ExecutedTourDto> GetExecutedTourAsync(TourExecution execution)
    {
        return await _executionReadRepository.GetExecutedTourAsync(execution.TourId)
            ?? throw new NotFoundException($"Tour {execution.TourId} does not exist.");
    }

    // Key points are reached in tour order, so tour order is also the order they were reached in.
    private static List<ReachedKeyPointDto> ReachedKeyPoints(TourExecution execution, ExecutedTourDto tour)
    {
        return tour.KeyPoints
            .Where(keyPoint => execution.HasReached(keyPoint.Id))
            .Select(keyPoint => new ReachedKeyPointDto(keyPoint.Id, keyPoint.Name, keyPoint.Secret))
            .ToList();
    }
}
