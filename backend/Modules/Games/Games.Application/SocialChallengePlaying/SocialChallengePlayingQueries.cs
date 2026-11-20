using Games.Application.SocialChallengeRuns;
using Games.Application.SocialChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengePlaying;

public sealed class SocialChallengePlayingQueries
{
    private readonly ISocialChallengeReadRepository _readRepository;
    private readonly ISocialChallengeRepository _challenges;
    private readonly ISocialChallengeRunRepository _runs;

    public SocialChallengePlayingQueries(ISocialChallengeReadRepository readRepository,
        ISocialChallengeRepository challenges, ISocialChallengeRunRepository runs)
    {
        _readRepository = readRepository;
        _challenges = challenges;
        _runs = runs;
    }

    public Task<List<SocialChallengeMapItemDto>> GetMapAsync(Guid userId)
    {
        return _readRepository.GetMapAsync(userId);
    }

    public async Task<SocialChallengeRunDto> GetRunAsync(Guid challengeId, Guid userId)
    {
        var challenge = await _challenges.GetByIdAsync(challengeId)
            ?? throw new NotFoundException("Društveni izazov ne postoji.");
        var activeRun = await _runs.GetActiveByChallengeAsync(challengeId);
        var solvedRun = (await _runs.GetSolvedByChallengeAndParticipantAsync(challengeId, userId)).FirstOrDefault();
        var now = DateTime.UtcNow;

        return new SocialChallengeRunDto(
            activeRun?.IsActiveParticipant(userId, now) ?? false,
            activeRun?.ActiveParticipantCount(now) ?? 0,
            challenge.RequiredParticipants,
            solvedRun is null ? null : challenge.CompletionMessage,
            solvedRun?.AwardedXp);
    }
}
