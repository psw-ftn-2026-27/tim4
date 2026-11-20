using Games.Domain.LocationChallenges;
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
    public double? Latitude { get; private set; }
    public double? Longitude { get; private set; }
    public SocialChallengeStatus Status { get; private set; }

    private SocialChallenge() { }

    public SocialChallenge(string name, ActivityTime activityTime, int requiredParticipants,
        decimal rangeInMeters, int xpReward, string completionMessage, double? latitude = null, double? longitude = null) : base(Guid.NewGuid())
    {
        SetDetails(name, activityTime, requiredParticipants, rangeInMeters, xpReward, completionMessage, latitude, longitude);
        Status = SocialChallengeStatus.Draft;
    }

    public void UpdateDetails(string name, ActivityTime activityTime, int requiredParticipants,
        decimal rangeInMeters, int xpReward, string completionMessage, double? latitude = null, double? longitude = null)
    {
        if (Status != SocialChallengeStatus.Draft)
        {
            throw new DomainException("Moguće je izmeniti samo nacrt izazova.");
        }

        SetDetails(name, activityTime, requiredParticipants, rangeInMeters, xpReward, completionMessage, latitude, longitude);
    }

    public void Publish()
    {
        if (Status != SocialChallengeStatus.Draft)
        {
            throw new DomainException("Moguće je objaviti samo nacrt izazova.");
        }

        _ = new ChallengeLocation(Latitude, Longitude);
        Status = SocialChallengeStatus.Published;
    }

    public void Retire()
    {
        if (Status != SocialChallengeStatus.Published)
        {
            throw new DomainException("Moguće je povući samo objavljen izazov.");
        }

        Status = SocialChallengeStatus.Retired;
    }

    public void Archive()
    {
        if (Status != SocialChallengeStatus.Retired)
        {
            throw new DomainException("Moguće je arhivirati samo povučen izazov.");
        }

        Status = SocialChallengeStatus.Archived;
    }

    public void EnsureCanBeJoined(ChallengeLocation position, DateTime now)
    {
        if (Status != SocialChallengeStatus.Published)
        {
            throw new DomainException("Moguće je aktivirati samo objavljen izazov.");
        }

        if (!IsInRange(position))
        {
            throw new DomainException("Niste u opsegu izazova.");
        }

        var time = TimeOnly.FromDateTime(now.ToLocalTime());
        if (time < ActivityTime.Start || time > ActivityTime.End)
        {
            throw new DomainException("Izazov trenutno nije aktivan.");
        }
    }

    internal bool IsInRange(ChallengeLocation position)
    {
        var location = new ChallengeLocation(Latitude, Longitude);
        return location.DistanceTo(position) <= (double)RangeInMeters;
    }

    public void EnsureCanBeDeleted()
    {
        if (Status != SocialChallengeStatus.Draft && Status != SocialChallengeStatus.Archived)
        {
            throw new DomainException("Moguće je obrisati samo nacrt ili arhivirani izazov.");
        }
    }

    private void SetDetails(string name, ActivityTime activityTime, int requiredParticipants,
        decimal rangeInMeters, int xpReward, string completionMessage, double? latitude = null, double? longitude = null)
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

        var location = new ChallengeLocation(latitude, longitude);

        Latitude = location.Latitude;
        Longitude = location.Longitude;
        Name = name;
        ActivityTime = activityTime;
        RequiredParticipants = requiredParticipants;
        RangeInMeters = rangeInMeters;
        XpReward = xpReward;
        CompletionMessage = completionMessage;
    }
}
