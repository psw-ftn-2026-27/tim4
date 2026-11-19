using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed record ChallengeLocation
{
    public double Latitude { get; }
    public double Longitude { get; }

    public ChallengeLocation(double? latitude, double? longitude)
    {
        if (latitude is not double validLatitude || !double.IsFinite(validLatitude) || validLatitude is < -90 or > 90 ||
            longitude is not double validLongitude || !double.IsFinite(validLongitude) || validLongitude is < -180 or > 180)
        {
            throw new DomainException("Koordinate lokacije nisu važeće.");
        }

        Latitude = validLatitude;
        Longitude = validLongitude;
    }

    public double DistanceTo(ChallengeLocation other)
    {
        const double earthRadiusInMeters = 6_371_000;
        const double radiansPerDegree = Math.PI / 180;
        var latitude = Latitude * radiansPerDegree;
        var otherLatitude = other.Latitude * radiansPerDegree;
        var halfLongitudeDifference = (other.Longitude - Longitude) * radiansPerDegree / 2;
        var latitudeTerm = Math.Sin((otherLatitude - latitude) / 2);
        var longitudeTerm = Math.Sin(halfLongitudeDifference);
        var a = latitudeTerm * latitudeTerm +
            Math.Cos(latitude) * Math.Cos(otherLatitude) * longitudeTerm * longitudeTerm;
        return 2 * earthRadiusInMeters * Math.Asin(Math.Sqrt(Math.Clamp(a, 0, 1)));
    }
}
