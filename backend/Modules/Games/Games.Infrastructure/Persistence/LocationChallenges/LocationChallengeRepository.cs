using Games.Application.LocationChallenges;
using Games.Domain.LocationChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.LocationChallenges;

internal sealed class LocationChallengeRepository : ILocationChallengeRepository
{
    private readonly GamesDbContext _context;

    public LocationChallengeRepository(GamesDbContext context)
    {
        _context = context;
    }

    public Task<LocationChallenge?> GetByIdAsync(Guid id)
    {
        return _context.LocationChallenges.SingleOrDefaultAsync(challenge => challenge.Id == id);
    }

    public void Add(LocationChallenge challenge)
    {
        _context.LocationChallenges.Add(challenge);
    }
}
