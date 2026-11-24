using Shared.Domain;

namespace Games.Domain.Experience;

public sealed class EarnedXp : AggregateRoot
{
    public Guid UserId { get; private set; }
    public ChallengeType ChallengeType { get; private set; }
    public Guid ChallengeId { get; private set; }
    public int Xp { get; private set; }
    public DateTime EarnedAt { get; private set; }

    private EarnedXp() { }

    public EarnedXp(Guid userId, ChallengeType challengeType, Guid challengeId, int xp, DateTime earnedAt)
        : base(Guid.NewGuid())
    {
        UserId = userId;
        ChallengeType = challengeType;
        ChallengeId = challengeId;
        Xp = xp;
        EarnedAt = earnedAt;
    }
}
