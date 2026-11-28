using Games.Application.LocationChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.LocationChallengeAuthoring;

public sealed class LocationChallengeAuthoringQueries
{
    private readonly ILocationChallengeReadRepository _repository;

    public LocationChallengeAuthoringQueries(ILocationChallengeReadRepository repository)
    {
        _repository = repository;
    }

    public async Task<LocationChallengeDetailsDto> GetDetailsAsync(Guid id)
    {
        return await _repository.GetDetailsAsync(id)
            ?? throw new NotFoundException("Lokacijski izazov ne postoji.");
    }
}
