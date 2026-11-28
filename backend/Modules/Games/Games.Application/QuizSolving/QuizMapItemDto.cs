namespace Games.Application.QuizSolving;

public sealed record QuizMapItemDto(
    Guid Id,
    string Name,
    string Instructions,
    double Latitude,
    double Longitude,
    QuizProgress Progress,
    double? EarnedXp);
