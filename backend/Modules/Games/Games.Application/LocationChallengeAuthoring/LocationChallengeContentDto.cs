namespace Games.Application.LocationChallengeAuthoring;

public sealed record LocationChallengeContentDto(
    string Name, double StartLatitude, double StartLongitude, int XpReward, List<HiddenLocationContentDto>? Points);

public sealed record HiddenLocationContentDto(double? Latitude, double? Longitude, byte[] Image);
