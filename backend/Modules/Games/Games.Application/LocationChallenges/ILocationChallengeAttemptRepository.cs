using Games.Domain.LocationChallenges;

namespace Games.Application.LocationChallenges;

public interface ILocationChallengeAttemptRepository
{
    Task<LocationChallengeAttempt?> GetByChallengeAndUserAsync(Guid challengeId, Guid userId);
    void Add(LocationChallengeAttempt attempt);
}
