namespace Games.Application.QuizAuthoring;

public sealed record QuestionDto(Guid Id, string Text, int XpReward, List<AnswerOptionDto> Options);
