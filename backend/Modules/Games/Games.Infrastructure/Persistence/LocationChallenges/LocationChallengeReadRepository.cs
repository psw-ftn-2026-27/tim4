using Games.Application.LocationChallengeAuthoring;
using Games.Application.LocationChallenges;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Games.Infrastructure.Persistence.LocationChallenges;

internal sealed class LocationChallengeReadRepository : ILocationChallengeReadRepository
{
    private readonly GamesDbContext _context;

    public LocationChallengeReadRepository(GamesDbContext context)
    {
        _context = context;
    }

    public async Task<PageResult<LocationChallengeDto>> GetPageAsync(int page, int pageSize)
    {
        var challenges = _context.LocationChallenges.AsNoTracking();
        var total = await challenges.CountAsync();
        var items = await challenges.OrderBy(challenge => challenge.Name).ThenBy(challenge => challenge.Id)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(challenge => new LocationChallengeDto(challenge.Id, challenge.Name,
                challenge.StartLatitude, challenge.StartLongitude, challenge.XpReward, challenge.HiddenLocations.Count))
            .ToListAsync();

        return new PageResult<LocationChallengeDto>(items, total);
    }

    public Task<LocationChallengeDetailsDto?> GetDetailsAsync(Guid id)
    {
        return _context.LocationChallenges.AsNoTracking().Where(challenge => challenge.Id == id)
            .Select(challenge => new LocationChallengeDetailsDto(
                new LocationChallengeDto(challenge.Id, challenge.Name, challenge.StartLatitude,
                    challenge.StartLongitude, challenge.XpReward, challenge.HiddenLocations.Count),
                challenge.HiddenLocations.OrderBy(point => point.PointId).Select(point => new HiddenLocationDto(
                    point.PointId, point.Latitude, point.Longitude, point.ImageAddress)).ToList()))
            .SingleOrDefaultAsync();
    }
}
