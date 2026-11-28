using Games.Application.LocationChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengePlaying;

public sealed class LocationChallengePlayingQueries
{
    private readonly ILocationChallengePlayReadRepository _repository;

    public LocationChallengePlayingQueries(
        ILocationChallengePlayReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<LocationChallengePlayDto> GetAsync(Guid challengeId, Guid userId)
    {
        var challenge = await _repository.GetChallengeAsync(challengeId)
            ?? throw new NotFoundException("Lokacijski izazov ne postoji.");
        var attempt = await _repository.GetProgressAsync(challengeId, userId);
        return new LocationChallengePlayDto(challenge, attempt);
    }
}
