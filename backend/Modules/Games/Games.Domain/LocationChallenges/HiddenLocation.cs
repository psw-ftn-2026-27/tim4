using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed class HiddenLocation
{
    public Guid PointId { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string ImageAddress { get; private set; }

    private HiddenLocation()
    {
        ImageAddress = null!;
    }

    public HiddenLocation(ChallengeLocation location, string imageAddress)
    {
        if (string.IsNullOrWhiteSpace(imageAddress))
        {
            throw new DomainException("Skrivena tačka mora imati fotografiju.");
        }

        PointId = Guid.NewGuid();
        Latitude = location.Latitude;
        Longitude = location.Longitude;
        ImageAddress = imageAddress;
    }

    public ChallengeLocation Location() => new(Latitude, Longitude);
}
