using Games.Application.LocationChallenges;

namespace Games.Application.LocationChallengeAuthoring;

public sealed record LocationChallengeDetailsDto(LocationChallengeDto Challenge, List<HiddenLocationDto> Points);

public sealed record HiddenLocationDto(Guid Id, double Latitude, double Longitude, string ImageAddress);
