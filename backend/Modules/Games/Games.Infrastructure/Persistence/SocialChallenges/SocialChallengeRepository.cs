using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.SocialChallenges;

internal sealed class SocialChallengeRepository : ISocialChallengeRepository
{
    private readonly GamesDbContext _context;

    public SocialChallengeRepository(GamesDbContext context)
    {
        _context = context;
    }

    public Task<SocialChallenge?> GetByIdAsync(Guid id)
    {
        return _context.SocialChallenges.SingleOrDefaultAsync(challenge => challenge.Id == id);
    }

    public void Add(SocialChallenge challenge)
    {
        _context.SocialChallenges.Add(challenge);
    }

    public void Remove(SocialChallenge challenge)
    {
        _context.SocialChallenges.Remove(challenge);
    }
}
