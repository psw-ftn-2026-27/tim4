using Games.Application.LocationChallengePlaying;

namespace Games.Application.LocationChallenges;

public interface ILocationChallengePlayReadRepository
{
    Task<LocationChallengeDto?> GetChallengeAsync(Guid challengeId);
    Task<LocationChallengeProgressDto?> GetProgressAsync(Guid challengeId, Guid userId);
}
