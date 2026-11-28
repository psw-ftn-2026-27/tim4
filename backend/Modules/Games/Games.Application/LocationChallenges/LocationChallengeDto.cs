namespace Games.Application.LocationChallenges;

public sealed record LocationChallengeDto(
    Guid Id, string Name, double StartLatitude, double StartLongitude, int XpReward, int PointCount);
