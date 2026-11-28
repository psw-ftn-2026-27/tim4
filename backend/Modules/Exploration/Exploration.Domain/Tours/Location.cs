using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed record Location
{
    public double Latitude { get; private init; }
    public double Longitude { get; private init; }

    private Location() { }

    public Location(double latitude, double longitude)
    {
        if (!double.IsFinite(latitude) || latitude < -90 || latitude > 90)
        {
            throw new DomainException("A latitude must be between -90 and 90 degrees.");
        }
        if (!double.IsFinite(longitude) || longitude < -180 || longitude > 180)
        {
            throw new DomainException("A longitude must be between -180 and 180 degrees.");
        }

        Latitude = latitude;
        Longitude = longitude;
    }

    // The straight-line distance over the Earth's surface (the haversine formula), the same one the
    // platform's shared map uses for a tour's length.
    public double DistanceInKm(Location other)
    {
        const double earthRadiusInKm = 6371;
        var latitudeDelta = ToRadians(other.Latitude - Latitude);
        var longitudeDelta = ToRadians(other.Longitude - Longitude);
        var a = Math.Sin(latitudeDelta / 2) * Math.Sin(latitudeDelta / 2)
            + Math.Cos(ToRadians(Latitude)) * Math.Cos(ToRadians(other.Latitude))
            * Math.Sin(longitudeDelta / 2) * Math.Sin(longitudeDelta / 2);
        return 2 * earthRadiusInKm * Math.Asin(Math.Sqrt(a));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;
}
