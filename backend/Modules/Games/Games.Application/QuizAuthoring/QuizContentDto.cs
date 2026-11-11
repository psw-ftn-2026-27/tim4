namespace Games.Application.QuizAuthoring;

public sealed record QuizContentDto(string Name, string Instructions, List<QuestionContentDto> Questions);
