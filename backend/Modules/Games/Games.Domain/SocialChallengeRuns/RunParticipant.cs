namespace Games.Domain.SocialChallengeRuns;

public sealed record RunParticipant
{
    public Guid UserId { get; private init; }
    public DateTime LastLocationAt { get; private init; }

    private RunParticipant() { }

    public RunParticipant(Guid userId, DateTime lastLocationAt)
    {
        UserId = userId;
        LastLocationAt = lastLocationAt;
    }
}
