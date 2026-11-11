using Games.Application.SocialChallengePreparation;
using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Games.Infrastructure.Persistence.SocialChallenges;

internal sealed class SocialChallengeReadRepository : ISocialChallengeReadRepository
{
    private readonly GamesDbContext _context;

    public SocialChallengeReadRepository(GamesDbContext context)
    {
        _context = context;
    }

    public async Task<PageResult<SocialChallengeDto>> GetAllAsync(int page, int pageSize)
    {
        var challenges = _context.SocialChallenges.AsNoTracking();
        var totalCount = await challenges.CountAsync();
        var requestedPage = challenges.OrderBy(challenge => challenge.Name).ThenBy(challenge => challenge.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize);
        var items = await ProjectChallenges(requestedPage)
            .ToListAsync();
        return new PageResult<SocialChallengeDto>(items, totalCount);
    }

    public Task<SocialChallengeDto?> GetByIdAsync(Guid id)
    {
        var challenges = _context.SocialChallenges.AsNoTracking().Where(challenge => challenge.Id == id);
        return ProjectChallenges(challenges).SingleOrDefaultAsync();
    }

    private static IQueryable<SocialChallengeDto> ProjectChallenges(IQueryable<SocialChallenge> challenges)
    {
        return challenges.Select(challenge => new SocialChallengeDto(
            challenge.Id, challenge.Name, challenge.ActivityTime.Start, challenge.ActivityTime.End,
            challenge.RequiredParticipants, challenge.RangeInMeters, challenge.XpReward,
            challenge.CompletionMessage, challenge.Status));
    }
}
