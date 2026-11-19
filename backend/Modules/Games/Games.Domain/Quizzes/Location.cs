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

    public double DistanceInMetersTo(Location other)
    {
        const double earthRadiusInMeters = 6_371_000;
        const double radiansPerDegree = Math.PI / 180;
        var latitude = Latitude * radiansPerDegree;
        var otherLatitude = other.Latitude * radiansPerDegree;
        var latitudeTerm = Math.Sin((otherLatitude - latitude) / 2);
        var longitudeTerm = Math.Sin((other.Longitude - Longitude) * radiansPerDegree / 2);
        var a = latitudeTerm * latitudeTerm +
            Math.Cos(latitude) * Math.Cos(otherLatitude) * longitudeTerm * longitudeTerm;
        return 2 * earthRadiusInMeters * Math.Asin(Math.Sqrt(Math.Clamp(a, 0, 1)));
    }
}
