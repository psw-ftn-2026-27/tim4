using Games.Application.LocationChallenges;
using Shared.Domain;

namespace Games.Application.LocationChallengeBrowsing;

public sealed class LocationChallengeBrowsingQueries
{
    private readonly ILocationChallengeReadRepository _repository;

    public LocationChallengeBrowsingQueries(ILocationChallengeReadRepository repository)
    {
        _repository = repository;
    }

    public Task<PageResult<LocationChallengeDto>> GetPageAsync(int page, int pageSize)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Clamp(page, 1, int.MaxValue / pageSize);
        return _repository.GetPageAsync(page, pageSize);
    }
}
