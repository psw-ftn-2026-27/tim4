using Exploration.Application.TourExecutions;
using Exploration.Domain.TourExecutions;
using Microsoft.EntityFrameworkCore;

namespace Exploration.Infrastructure.Persistence.TourExecutions;

internal sealed class TourExecutionReadRepository : ITourExecutionReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourExecutionReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<ExecutedTourDto?> GetExecutedTourAsync(Guid tourId)
    {
        return _dbContext.Tours
            .Where(tour => tour.Id == tourId)
            .Select(tour => new ExecutedTourDto(
                tour.Id,
                tour.Name,
                tour.KeyPoints
                    .Select(keyPoint => new ExecutedKeyPointDto(
                        keyPoint.Id,
                        keyPoint.Name,
                        keyPoint.Location.Latitude,
                        keyPoint.Location.Longitude,
                        keyPoint.Secret))
                    .ToList()))
            .SingleOrDefaultAsync();
    }

    public Task<Guid?> GetInProgressTourIdAsync(Guid touristId)
    {
        return _dbContext.TourExecutions
            .Where(execution => execution.TouristId == touristId && execution.Status == TourExecutionStatus.InProgress)
            .Select(execution => (Guid?)execution.TourId)
            .FirstOrDefaultAsync();
    }
}
