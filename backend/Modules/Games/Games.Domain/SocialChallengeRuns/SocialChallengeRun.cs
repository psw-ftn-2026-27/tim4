using Games.Domain.Experience;
using Games.Domain.LocationChallenges;
using Games.Domain.SocialChallenges;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallengeRuns;

public sealed class SocialChallengeRun : AggregateRoot
{
    private static readonly TimeSpan ActivityWindow = TimeSpan.FromSeconds(10);
    private readonly List<RunParticipant> _participants = [];

    public Guid ChallengeId { get; private set; }
    public IReadOnlyList<RunParticipant> Participants => _participants;
    public DateTime? SolvedAt { get; private set; }
    public int AwardedXp { get; private set; }

    private SocialChallengeRun() { }

    internal SocialChallengeRun(Guid challengeId)
        : base(Guid.NewGuid())
    {
        ChallengeId = challengeId;
    }

    internal void Admit(Guid userId, DateTime now)
    {
        EnsureNotSolved();
        _participants.RemoveAll(participant => participant.UserId == userId);
        _participants.Add(new RunParticipant(userId, now));
    }

    public IReadOnlyList<EarnedXp> ReportLocation(SocialChallenge challenge, Guid userId, ChallengeLocation position,
        DateTime now)
    {
        if (challenge.Id != ChallengeId)
        {
            throw new DomainException("Izvedba ne pripada ovom izazovu.");
        }

        EnsureNotSolved();
        if (_participants.All(participant => participant.UserId != userId))
        {
            throw new DomainException("Niste uključeni u izvedbu izazova.");
        }

        _participants.RemoveAll(participant => participant.UserId == userId);
        if (challenge.IsInRange(position))
        {
            _participants.Add(new RunParticipant(userId, now));
        }

        _participants.RemoveAll(participant => !IsActive(participant, now));
        if (ActiveParticipantCount(now) < challenge.RequiredParticipants)
        {
            return [];
        }

        SolvedAt = now;
        AwardedXp = challenge.XpReward;
        return _participants
            .Select(participant => new EarnedXp(participant.UserId, ChallengeType.Social, ChallengeId, AwardedXp, now))
            .ToList();
    }

    public int ActiveParticipantCount(DateTime now)
    {
        return _participants.Count(participant => IsActive(participant, now));
    }

    public bool IsActiveParticipant(Guid userId, DateTime now)
    {
        return _participants.Any(participant => participant.UserId == userId && IsActive(participant, now));
    }

    private static bool IsActive(RunParticipant participant, DateTime now)
    {
        return now - participant.LastLocationAt <= ActivityWindow;
    }

    private void EnsureNotSolved()
    {
        if (SolvedAt.HasValue)
        {
            throw new DomainException("Izvedba izazova je već rešena.");
        }
    }
}
