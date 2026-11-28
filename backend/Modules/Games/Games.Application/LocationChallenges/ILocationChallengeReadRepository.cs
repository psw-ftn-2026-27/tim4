using Games.Application.LocationChallengeAuthoring;
using Shared.Domain;

namespace Games.Application.LocationChallenges;

public interface ILocationChallengeReadRepository
{
    Task<PageResult<LocationChallengeDto>> GetPageAsync(int page, int pageSize);
    Task<LocationChallengeDetailsDto?> GetDetailsAsync(Guid id);
}
