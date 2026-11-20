using Games.Application.SocialChallengeRuns;
using Games.Domain.SocialChallengeRuns;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.SocialChallengeRuns;

internal sealed class SocialChallengeRunRepository : ISocialChallengeRunRepository
{
    private readonly GamesDbContext _context;

    public SocialChallengeRunRepository(GamesDbContext context)
    {
        _context = context;
    }

    public Task<SocialChallengeRun?> GetActiveByChallengeAsync(Guid challengeId)
    {
        return _context.SocialChallengeRuns
            .FirstOrDefaultAsync(run => run.ChallengeId == challengeId && run.SolvedAt == null);
    }

    public Task<List<SocialChallengeRun>> GetSolvedByChallengeAndParticipantAsync(Guid challengeId, Guid userId)
    {
        return _context.SocialChallengeRuns
            .Where(run => run.ChallengeId == challengeId && run.SolvedAt != null &&
                run.Participants.Any(participant => participant.UserId == userId))
            .ToListAsync();
    }

    public void Add(SocialChallengeRun run)
    {
        _context.SocialChallengeRuns.Add(run);
    }
}
