using Positions.Application.TouristPositions;

namespace Positions.Application.PositionSimulation;

public sealed class PositionSimulationQueries
{
    private readonly ITouristPositionReadRepository _positionReadRepository;

    public PositionSimulationQueries(ITouristPositionReadRepository positionReadRepository)
    {
        _positionReadRepository = positionReadRepository;
    }

    public Task<CoordinatesDto?> GetMineAsync(Guid touristId)
    {
        return _positionReadRepository.GetByTouristIdAsync(touristId);
    }
}
