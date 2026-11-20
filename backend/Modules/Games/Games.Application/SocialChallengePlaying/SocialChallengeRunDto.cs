namespace Games.Application.SocialChallengePlaying;

public sealed record SocialChallengeRunDto(
    bool IsParticipant,
    int ActiveParticipantCount,
    int RequiredParticipants,
    string? CompletionMessage,
    int? AwardedXp);
