using Microsoft.EntityFrameworkCore;
using Positions.Application.TouristPositions;
using Positions.Domain.TouristPositions;

namespace Positions.Infrastructure.Persistence.TouristPositions;

internal sealed class TouristPositionRepository : ITouristPositionRepository
{
    private readonly PositionsDbContext _dbContext;

    public TouristPositionRepository(PositionsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<TouristPosition?> GetByTouristIdAsync(Guid touristId)
    {
        return _dbContext.TouristPositions.FirstOrDefaultAsync(position => position.TouristId == touristId);
    }

    public void Add(TouristPosition position)
    {
        _dbContext.TouristPositions.Add(position);
    }
}
