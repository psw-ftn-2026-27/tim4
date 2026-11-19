namespace Games.Application.QuizAuthoring;

public sealed record QuizContentDto(
    string Name,
    string Instructions,
    List<QuestionContentDto> Questions,
    double Latitude,
    double Longitude,
    int RangeInMeters);
