using Games.Application.SocialChallengePlaying;
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

    public async Task<List<SocialChallengeMapItemDto>> GetMapAsync(Guid userId)
    {
        var challenges = await _context.SocialChallenges.AsNoTracking()
            .Where(challenge => challenge.Status == SocialChallengeStatus.Published)
            .OrderBy(challenge => challenge.Name).ThenBy(challenge => challenge.Id)
            .Select(challenge => new
            {
                challenge.Id,
                challenge.Name,
                Latitude = challenge.Latitude!.Value,
                Longitude = challenge.Longitude!.Value,
                challenge.XpReward
            })
            .ToListAsync();

        var challengeIds = challenges.Select(challenge => challenge.Id).ToList();
        var awards = await _context.SocialChallengeRuns.AsNoTracking()
            .Where(run => run.SolvedAt != null && challengeIds.Contains(run.ChallengeId) &&
                run.Participants.Any(participant => participant.UserId == userId))
            .Select(run => new { run.ChallengeId, run.AwardedXp })
            .ToListAsync();
        var awardedXp = awards
            .DistinctBy(award => award.ChallengeId)
            .ToDictionary(award => award.ChallengeId, award => award.AwardedXp);

        return challenges
            .Select(challenge => new SocialChallengeMapItemDto(challenge.Id, challenge.Name, challenge.Latitude,
                challenge.Longitude, challenge.XpReward, awardedXp.ContainsKey(challenge.Id),
                awardedXp.TryGetValue(challenge.Id, out var xp) ? xp : null))
            .ToList();
    }

    private static IQueryable<SocialChallengeDto> ProjectChallenges(IQueryable<SocialChallenge> challenges)
    {
        return challenges.Select(challenge => new SocialChallengeDto(
            challenge.Id, challenge.Name, challenge.ActivityTime.Start, challenge.ActivityTime.End,
            challenge.RequiredParticipants, challenge.RangeInMeters, challenge.XpReward,
            challenge.CompletionMessage, challenge.Status, challenge.Latitude, challenge.Longitude));
    }
}
