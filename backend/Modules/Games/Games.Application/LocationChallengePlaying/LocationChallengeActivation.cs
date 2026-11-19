namespace Games.Application.LocationChallengePlaying;

public sealed class LocationChallengeActivation
{
    public double DistanceInMeters { get; }

    public LocationChallengeActivation(double? distanceInMeters)
    {
        if (distanceInMeters is double distance && (!double.IsFinite(distance) || distance < 0))
        {
            throw new ArgumentOutOfRangeException(nameof(distanceInMeters));
        }

        DistanceInMeters = distanceInMeters ?? 50;
    }

}
