using Shared.Domain;

namespace Positions.Domain.TouristPositions;

public sealed class TouristPosition : AggregateRoot
{
    public Guid TouristId { get; private set; }
    public Coordinates Coordinates { get; private set; }

    private TouristPosition()
    {
        Coordinates = null!;
    }

    public TouristPosition(Guid touristId, Coordinates coordinates)
        : base(Guid.NewGuid())
    {
        TouristId = touristId;
        Coordinates = coordinates;
    }

    public void MoveTo(Coordinates coordinates)
    {
        Coordinates = coordinates;
    }
}
