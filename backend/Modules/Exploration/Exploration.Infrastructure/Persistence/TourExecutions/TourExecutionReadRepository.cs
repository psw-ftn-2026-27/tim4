using Exploration.Application.TourExecutions;
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
}
