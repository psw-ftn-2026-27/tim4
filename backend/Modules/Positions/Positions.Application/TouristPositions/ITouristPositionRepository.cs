using Positions.Domain.TouristPositions;

namespace Positions.Application.TouristPositions;

public interface ITouristPositionRepository
{
    Task<TouristPosition?> GetByTouristIdAsync(Guid touristId);
    void Add(TouristPosition position);
}
