using Games.Application.LocationChallenges;
using Games.Domain.LocationChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.LocationChallenges;

internal sealed class LocationChallengeAttemptRepository : ILocationChallengeAttemptRepository
{
    private readonly GamesDbContext _context;

    public LocationChallengeAttemptRepository(GamesDbContext context)
    {
        _context = context;
    }

    public Task<LocationChallengeAttempt?> GetByChallengeAndUserAsync(Guid challengeId, Guid userId)
    {
        return _context.LocationChallengeAttempts.SingleOrDefaultAsync(attempt =>
            attempt.ChallengeId == challengeId && attempt.UserId == userId);
    }

    public void Add(LocationChallengeAttempt attempt)
    {
        _context.LocationChallengeAttempts.Add(attempt);
    }
}
