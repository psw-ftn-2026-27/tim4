using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed class LocationChallenge : AggregateRoot
{
    private readonly List<HiddenLocation> _hiddenLocations = [];

    public string Name { get; private set; }
    public double StartLatitude { get; private set; }
    public double StartLongitude { get; private set; }
    public int XpReward { get; private set; }
    public IReadOnlyList<HiddenLocation> HiddenLocations => _hiddenLocations.AsReadOnly();

    private LocationChallenge()
    {
        Name = null!;
    }

    public LocationChallenge(string name, ChallengeLocation start, int xpReward, IReadOnlyList<HiddenLocation> points)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 200)
        {
            throw new DomainException("Naziv izazova mora imati od 1 do 200 znakova.");
        }

        if (xpReward <= 0)
        {
            throw new DomainException("XP nagrada mora biti veća od nule.");
        }

        if (points.Count == 0 || points.Select(point => point.PointId).Distinct().Count() != points.Count)
        {
            throw new DomainException("Izazov mora imati jednu ili više skrivenih tačaka sa različitim identifikatorima.");
        }

        Name = name.Trim();
        StartLatitude = start.Latitude;
        StartLongitude = start.Longitude;
        XpReward = xpReward;
        _hiddenLocations.AddRange(points);
    }

    public LocationChallengeAttempt Activate(Guid userId, ChallengeLocation position, double activationDistanceInMeters)
    {
        var start = new ChallengeLocation(StartLatitude, StartLongitude);
        if (!double.IsFinite(activationDistanceInMeters) || activationDistanceInMeters < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(activationDistanceInMeters));
        }

        if (start.DistanceTo(position) > activationDistanceInMeters)
        {
            throw new DomainException("Približite se početnoj lokaciji izazova.");
        }

        var radius = _hiddenLocations.Max(point => start.DistanceTo(point.Location())) + Random.Shared.Next(0, 101);
        return new LocationChallengeAttempt(userId, Id, radius);
    }
}
