using Positions.Application.TouristPositions;
using Positions.Domain.TouristPositions;

namespace Positions.Application.PositionSimulation;

public sealed class PositionSimulationService
{
    private readonly ITouristPositionRepository _positionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public PositionSimulationService(ITouristPositionRepository positionRepository, IUnitOfWork unitOfWork)
    {
        _positionRepository = positionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task SetAsync(Guid touristId, CoordinatesDto dto)
    {
        var coordinates = new Coordinates(dto.Latitude, dto.Longitude);
        var position = await _positionRepository.GetByTouristIdAsync(touristId);

        if (position is null)
        {
            _positionRepository.Add(new TouristPosition(touristId, coordinates));
        }
        else
        {
            position.MoveTo(coordinates);
        }
        await _unitOfWork.SaveChangesAsync();
    }
}
