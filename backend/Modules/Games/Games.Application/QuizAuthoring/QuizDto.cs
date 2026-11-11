using Games.Domain.Quizzes;

namespace Games.Application.QuizAuthoring;

public sealed record QuizDto(Guid Id, string Name, string Instructions, QuizStatus Status, List<QuestionDto> Questions);
