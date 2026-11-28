using Games.Domain.Quizzes;

namespace Games.Application.QuizAuthoring;

public sealed record ChallengeDto(Guid Id, string Name, QuizStatus Status, int TotalXp);
