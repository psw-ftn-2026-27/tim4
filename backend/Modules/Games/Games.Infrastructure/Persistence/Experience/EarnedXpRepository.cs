using Games.Application.Experience;
using Games.Domain.Experience;

namespace Games.Infrastructure.Persistence.Experience;

internal sealed class EarnedXpRepository : IEarnedXpRepository
{
    private readonly GamesDbContext _context;

    public EarnedXpRepository(GamesDbContext context)
    {
        _context = context;
    }

    public void Add(EarnedXp earnedXp)
    {
        _context.XpAwards.Add(earnedXp);
    }
}
