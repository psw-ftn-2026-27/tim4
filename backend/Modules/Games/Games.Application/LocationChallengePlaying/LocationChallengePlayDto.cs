using Games.Application.LocationChallenges;

namespace Games.Application.LocationChallengePlaying;

public sealed record LocationChallengePlayDto(
    LocationChallengeDto Challenge, LocationChallengeProgressDto? Attempt);

public sealed record LocationChallengeProgressDto(
    double RadiusInMeters, DateTime? CompletedAt, int AwardedXp, List<LocationChallengePointProgressDto> Points);

public sealed record LocationChallengePointProgressDto(Guid Id, bool Discovered, string ImageAddress);
