using System.Text.Json.Serialization;

namespace Games.Application.SocialChallengePreparation;

public sealed record SocialChallengeDetailsDto(string Name, [property: JsonRequired] TimeOnly Start,
    [property: JsonRequired] TimeOnly End, int RequiredParticipants, decimal RangeInMeters,
    int XpReward, string CompletionMessage);
