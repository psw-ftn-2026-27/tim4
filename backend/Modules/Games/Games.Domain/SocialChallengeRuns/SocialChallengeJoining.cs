using Games.Domain.LocationChallenges;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallengeRuns;

public sealed class SocialChallengeJoining
{
    public SocialChallengeRun Join(SocialChallenge challenge, SocialChallengeRun? activeRun,
        IReadOnlyList<SocialChallengeRun> solvedRuns, Guid userId, ChallengeLocation position, DateTime now)
    {
        challenge.EnsureCanBeJoined(position, now);

        if (activeRun is not null && (activeRun.ChallengeId != challenge.Id || activeRun.SolvedAt.HasValue))
        {
            throw new DomainException("Izvedba nije aktivna izvedba ovog izazova.");
        }

        if (solvedRuns.Any(run => run.ChallengeId == challenge.Id && run.SolvedAt.HasValue &&
                run.Participants.Any(participant => participant.UserId == userId)))
        {
            throw new DomainException("Već ste rešili ovaj izazov.");
        }

        var run = activeRun ?? new SocialChallengeRun(challenge.Id);
        run.Admit(userId, now);
        return run;
    }
}
