namespace Games.Application.QuizSolving;

public sealed record QuizAttemptDto(
    Guid QuizId,
    string Name,
    string Instructions,
    int AnsweredCount,
    int QuestionCount,
    CurrentQuestionDto? CurrentQuestion,
    QuizResultDto? Result);

public sealed record CurrentQuestionDto(Guid Id, string Text, List<string> Options);

public sealed record QuizResultDto(double EarnedXp, int TotalXp, double Percentage);
