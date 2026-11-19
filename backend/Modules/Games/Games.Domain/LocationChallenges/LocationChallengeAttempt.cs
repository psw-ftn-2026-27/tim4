using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.LocationChallenges;

public sealed class LocationChallengeAttempt : AggregateRoot
{
    private const double DiscoveryDistanceInMeters = 5;
    private Guid[] _discoveredPointIds = [];

    public Guid UserId { get; private set; }
    public Guid ChallengeId { get; private set; }
    // Existing database column name; stored values are used directly as radii.
    public double DiameterInMeters { get; private set; }
    public IReadOnlyList<Guid> DiscoveredPointIds => Array.AsReadOnly(_discoveredPointIds);
    public DateTime? CompletedAt { get; private set; }
    public int AwardedXp { get; private set; }
    public int Version { get; private set; }

    private LocationChallengeAttempt() { }

    internal LocationChallengeAttempt(Guid userId, Guid challengeId, double radiusInMeters)
        : base(Guid.NewGuid())
    {
        UserId = userId;
        ChallengeId = challengeId;
        DiameterInMeters = radiusInMeters;
    }

    public void CheckLocation(LocationChallenge challenge, Guid pointId, ChallengeLocation position, DateTime utcNow)
    {
        if (challenge.Id != ChallengeId)
        {
            throw new DomainException("Tačka ne pripada aktiviranom izazovu.");
        }

        if (CompletedAt.HasValue || _discoveredPointIds.Contains(pointId))
        {
            return;
        }

        var point = challenge.HiddenLocations.SingleOrDefault(location => location.PointId == pointId)
            ?? throw new NotFoundException("Skrivena tačka nije pronađena.");

        if (point.Location().DistanceTo(position) > DiscoveryDistanceInMeters)
        {
            throw new DomainException("Niste dovoljno blizu skrivene tačke.");
        }

        _discoveredPointIds = [.. _discoveredPointIds, pointId];
        Version++;

        if (_discoveredPointIds.Length == challenge.HiddenLocations.Count)
        {
            CompletedAt = utcNow;
            AwardedXp = challenge.XpReward;
        }
    }
}
