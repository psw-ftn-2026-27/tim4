using Games.Domain.SocialChallengeRuns;

namespace Games.Application.SocialChallengeRuns;

public interface ISocialChallengeRunRepository
{
    Task<SocialChallengeRun?> GetActiveByChallengeAsync(Guid challengeId);
    Task<List<SocialChallengeRun>> GetSolvedByChallengeAndParticipantAsync(Guid challengeId, Guid userId);
    void Add(SocialChallengeRun run);
}
