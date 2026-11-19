using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed record Location
{
    public double Latitude { get; }
    public double Longitude { get; }

    public Location(double latitude, double longitude)
    {
        if (latitude < -90 || latitude > 90)
        {
            throw new DomainException("Latitude must be between -90 and 90 degrees.");
        }
        if (longitude < -180 || longitude > 180)
        {
            throw new DomainException("Longitude must be between -180 and 180 degrees.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }
}
