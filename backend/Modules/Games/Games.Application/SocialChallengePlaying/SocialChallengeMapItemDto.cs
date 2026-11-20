namespace Games.Application.SocialChallengePlaying;

public sealed record SocialChallengeMapItemDto(
    Guid Id,
    string Name,
    double Latitude,
    double Longitude,
    int XpReward,
    bool Solved,
    int? AwardedXp);
