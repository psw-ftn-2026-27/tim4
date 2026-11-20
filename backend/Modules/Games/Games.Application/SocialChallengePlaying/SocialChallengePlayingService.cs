using Games.Application.SocialChallengeRuns;
using Games.Application.SocialChallenges;
using Games.Domain.LocationChallenges;
using Games.Domain.SocialChallengeRuns;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengePlaying;

public sealed class SocialChallengePlayingService
{
    private readonly ISocialChallengeRepository _challenges;
    private readonly ISocialChallengeRunRepository _runs;
    private readonly SocialChallengeJoining _joining;
    private readonly IUnitOfWork _unitOfWork;

    public SocialChallengePlayingService(ISocialChallengeRepository challenges, ISocialChallengeRunRepository runs,
        SocialChallengeJoining joining, IUnitOfWork unitOfWork)
    {
        _challenges = challenges;
        _runs = runs;
        _joining = joining;
        _unitOfWork = unitOfWork;
    }

    public async Task ActivateAsync(Guid challengeId, Guid userId, SocialChallengePositionDto position)
    {
        var challenge = await GetChallengeAsync(challengeId);
        var activeRun = await _runs.GetActiveByChallengeAsync(challengeId);
        var solvedRuns = await _runs.GetSolvedByChallengeAndParticipantAsync(challengeId, userId);

        var run = _joining.Join(challenge, activeRun, solvedRuns, userId,
            new ChallengeLocation(position.Latitude, position.Longitude), DateTime.UtcNow);
        if (activeRun is null)
        {
            _runs.Add(run);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ReportLocationAsync(Guid challengeId, Guid userId, SocialChallengePositionDto position)
    {
        var challenge = await GetChallengeAsync(challengeId);
        var run = await _runs.GetActiveByChallengeAsync(challengeId)
            ?? throw new NotFoundException("Aktivna izvedba izazova ne postoji.");

        run.ReportLocation(challenge, userId, new ChallengeLocation(position.Latitude, position.Longitude), DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<SocialChallenge> GetChallengeAsync(Guid challengeId)
    {
        return await _challenges.GetByIdAsync(challengeId)
            ?? throw new NotFoundException("Društveni izazov ne postoji.");
    }
}
