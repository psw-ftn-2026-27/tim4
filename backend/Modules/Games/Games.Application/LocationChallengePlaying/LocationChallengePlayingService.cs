using Games.Application.LocationChallenges;
using Games.Domain.LocationChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengePlaying;

public sealed class LocationChallengePlayingService
{
    private readonly ILocationChallengeRepository _challenges;
    private readonly ILocationChallengeAttemptRepository _attempts;
    private readonly IUnitOfWork _unitOfWork;
    private readonly LocationChallengeActivation _activation;

    public LocationChallengePlayingService(
        ILocationChallengeRepository challenges, ILocationChallengeAttemptRepository attempts,
        IUnitOfWork unitOfWork, LocationChallengeActivation activation)
    {
        _challenges = challenges;
        _attempts = attempts;
        _unitOfWork = unitOfWork;
        _activation = activation;
    }

    public async Task ActivateAsync(Guid challengeId, Guid userId, CheckLocationDto position)
    {
        var attempt = await _attempts.GetByChallengeAndUserAsync(challengeId, userId);
        if (attempt is not null)
        {
            return;
        }

        var challenge = await GetChallengeAsync(challengeId);
        var distance = _activation.DistanceInMeters;
        _attempts.Add(challenge.Activate(userId, new ChallengeLocation(position.Latitude, position.Longitude), distance));
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task CheckLocationAsync(Guid challengeId, Guid userId, Guid pointId, CheckLocationDto position)
    {
        var attempt = await _attempts.GetByChallengeAndUserAsync(challengeId, userId)
            ?? throw new NotFoundException("Aktivni pokušaj nije pronađen.");
        var challenge = await GetChallengeAsync(challengeId);
        attempt.CheckLocation(challenge, pointId, new ChallengeLocation(position.Latitude, position.Longitude), DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<LocationChallenge> GetChallengeAsync(Guid challengeId)
    {
        return await _challenges.GetByIdAsync(challengeId)
            ?? throw new NotFoundException("Lokacijski izazov ne postoji.");
    }
}
