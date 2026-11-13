using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallenges;

public sealed class SocialChallenge : AggregateRoot
{
    public string Name { get; private set; } = null!;
    public ActivityTime ActivityTime { get; private set; } = null!;
    public int RequiredParticipants { get; private set; }
    public decimal RangeInMeters { get; private set; }
    public int XpReward { get; private set; }
    public string CompletionMessage { get; private set; } = null!;
    public SocialChallengeStatus Status { get; private set; }

    private SocialChallenge() { }

    public SocialChallenge(string name, ActivityTime activityTime, int requiredParticipants,
        decimal rangeInMeters, int xpReward, string completionMessage) : base(Guid.NewGuid())
    {
        SetDetails(name, activityTime, requiredParticipants, rangeInMeters, xpReward, completionMessage);
        Status = SocialChallengeStatus.Draft;
    }

    public void UpdateDetails(string name, ActivityTime activityTime, int requiredParticipants,
        decimal rangeInMeters, int xpReward, string completionMessage)
    {
        if (Status != SocialChallengeStatus.Draft)
        {
            throw new DomainException("Moguće je izmeniti samo nacrt izazova.");
        }

        SetDetails(name, activityTime, requiredParticipants, rangeInMeters, xpReward, completionMessage);
    }

    public void EnsureCanBeDeleted()
    {
        if (Status != SocialChallengeStatus.Draft && Status != SocialChallengeStatus.Archived)
        {
            throw new DomainException("Moguće je obrisati samo nacrt ili arhivirani izazov.");
        }
    }

    private void SetDetails(string name, ActivityTime activityTime, int requiredParticipants,
        decimal rangeInMeters, int xpReward, string completionMessage)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Naziv izazova je obavezan.");
        }

        if (activityTime is null)
        {
            throw new DomainException("Vremenski interval je obavezan.");
        }

        if (requiredParticipants < 2)
        {
            throw new DomainException("Potrebna su najmanje dva učesnika.");
        }

        if (rangeInMeters <= 0)
        {
            throw new DomainException("Opseg u metrima mora biti veći od nule.");
        }

        if (xpReward <= 0)
        {
            throw new DomainException("XP nagrada mora biti veća od nule.");
        }

        if (string.IsNullOrWhiteSpace(completionMessage))
        {
            throw new DomainException("Završna poruka je obavezna.");
        }

        Name = name;
        ActivityTime = activityTime;
        RequiredParticipants = requiredParticipants;
        RangeInMeters = rangeInMeters;
        XpReward = xpReward;
        CompletionMessage = completionMessage;
    }
}
