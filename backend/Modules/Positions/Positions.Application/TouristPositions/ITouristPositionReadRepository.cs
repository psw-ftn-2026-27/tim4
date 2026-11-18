using Positions.Application.PositionSimulation;

namespace Positions.Application.TouristPositions;

public interface ITouristPositionReadRepository
{
    Task<CoordinatesDto?> GetByTouristIdAsync(Guid touristId);
}
