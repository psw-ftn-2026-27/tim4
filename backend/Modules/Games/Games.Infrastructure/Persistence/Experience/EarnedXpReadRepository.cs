using Games.Application.Experience;
using Games.Application.PlayerExperience;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.Experience;

internal sealed class EarnedXpReadRepository : IEarnedXpReadRepository
{
    private readonly GamesDbContext _context;

    public EarnedXpReadRepository(GamesDbContext context)
    {
        _context = context;
    }

    public async Task<TotalXpDto> GetTotalXpAsync(Guid userId)
    {
        var totalXp = await _context.XpAwards.AsNoTracking()
            .Where(award => award.UserId == userId)
            .SumAsync(award => award.Xp);
        return new TotalXpDto(totalXp);
    }
}
