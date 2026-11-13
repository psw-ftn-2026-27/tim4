using Games.Domain.SocialChallenges;

namespace Games.Application.SocialChallengePreparation;

public sealed record SocialChallengeDto(Guid Id, string Name, TimeOnly Start, TimeOnly End,
    int RequiredParticipants, decimal RangeInMeters, int XpReward, string CompletionMessage,
    SocialChallengeStatus Status);
