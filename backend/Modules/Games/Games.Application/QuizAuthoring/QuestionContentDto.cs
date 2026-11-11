namespace Games.Application.QuizAuthoring;

public sealed record QuestionContentDto(string Text, int XpReward, List<AnswerOptionDto> Options);
