using Shared.Domain.Exceptions;

namespace Games.Domain.SocialChallenges;

public sealed record ActivityTime
{
    public TimeOnly Start { get; }
    public TimeOnly End { get; }

    public ActivityTime(TimeOnly start, TimeOnly end)
    {
        if (start >= end)
        {
            throw new DomainException("Vreme završetka mora biti posle vremena početka istog dana.");
        }

        Start = start;
        End = end;
    }
}
