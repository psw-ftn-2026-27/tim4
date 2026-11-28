using Microsoft.EntityFrameworkCore;
using Positions.Application.PositionSimulation;
using Positions.Application.TouristPositions;

namespace Positions.Infrastructure.Persistence.TouristPositions;

internal sealed class TouristPositionReadRepository : ITouristPositionReadRepository
{
    private readonly PositionsDbContext _dbContext;

    public TouristPositionReadRepository(PositionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<CoordinatesDto?> GetByTouristIdAsync(Guid touristId)
    {
        return _dbContext.TouristPositions
            .Where(position => position.TouristId == touristId)
            .Select(position => new CoordinatesDto(position.Coordinates.Latitude, position.Coordinates.Longitude))
            .FirstOrDefaultAsync();
    }
}
