using Shared.Domain.Exceptions;

namespace Positions.Domain.TouristPositions;

public sealed record Coordinates
{
    public double Latitude { get; }
    public double Longitude { get; }

    public Coordinates(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
        {
            throw new DomainException("Geografska širina mora biti između -90 i 90 stepeni.");
        }
        if (longitude < -180 || longitude > 180)
        {
            throw new DomainException("Geografska dužina mora biti između -180 i 180 stepeni.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }
}
