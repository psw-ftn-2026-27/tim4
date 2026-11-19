using Games.Application.LocationChallengePlaying;
using Games.Application.LocationChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.LocationChallenges;

internal sealed class LocationChallengePlayReadRepository : ILocationChallengePlayReadRepository
{
    private readonly GamesDbContext _context;

    public LocationChallengePlayReadRepository(GamesDbContext context)
    {
        _context = context;
    }

    public Task<LocationChallengeDto?> GetChallengeAsync(Guid challengeId)
    {
        return _context.LocationChallenges.AsNoTracking()
            .Where(challenge => challenge.Id == challengeId)
            .Select(challenge => new LocationChallengeDto(challenge.Id, challenge.Name,
                challenge.StartLatitude, challenge.StartLongitude, challenge.XpReward, challenge.HiddenLocations.Count))
            .SingleOrDefaultAsync();
    }

    public async Task<LocationChallengeProgressDto?> GetProgressAsync(Guid challengeId, Guid userId)
    {
        var attempt = await _context.LocationChallengeAttempts.AsNoTracking()
            .Where(attempt => attempt.ChallengeId == challengeId && attempt.UserId == userId)
            .Select(attempt => new
            {
                attempt.DiameterInMeters,
                attempt.CompletedAt,
                attempt.AwardedXp,
                Discovered = EF.Property<Guid[]>(attempt, "_discoveredPointIds")
            }).SingleOrDefaultAsync();

        if (attempt is null)
        {
            return null;
        }

        var points = await _context.LocationChallenges.AsNoTracking()
            .Where(challenge => challenge.Id == challengeId)
            .SelectMany(challenge => challenge.HiddenLocations)
            .OrderBy(point => point.PointId)
            .Select(point => new { point.PointId, point.ImageAddress })
            .ToListAsync();

        return new LocationChallengeProgressDto(attempt.DiameterInMeters, attempt.CompletedAt, attempt.AwardedXp,
            points.Select(point => new LocationChallengePointProgressDto(
                point.PointId, attempt.Discovered.Contains(point.PointId), point.ImageAddress)).ToList());
    }
}
