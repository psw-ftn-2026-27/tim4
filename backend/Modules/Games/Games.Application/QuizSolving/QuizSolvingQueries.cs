using Games.Application.QuizAttempts;
using Games.Application.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizSolving;

public sealed class QuizSolvingQueries
{
    private readonly IQuizReadRepository _quizReadRepository;
    private readonly IQuizRepository _quizRepository;
    private readonly IQuizAttemptRepository _attemptRepository;

    public QuizSolvingQueries(IQuizReadRepository quizReadRepository, IQuizRepository quizRepository,
        IQuizAttemptRepository attemptRepository)
    {
        _quizReadRepository = quizReadRepository;
        _quizRepository = quizRepository;
        _attemptRepository = attemptRepository;
    }

    public Task<List<QuizMapItemDto>> GetQuizMapAsync(Guid playerId)
    {
        return _quizReadRepository.GetQuizMapAsync(playerId);
    }

    public async Task<QuizAttemptDto> GetAttemptAsync(Guid quizId, Guid playerId)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        if (quiz is null)
        {
            throw new NotFoundException($"Quiz {quizId} does not exist.");
        }
        var attempt = await _attemptRepository.GetByQuizAndPlayerAsync(quizId, playerId);
        if (attempt is null)
        {
            throw new NotFoundException($"Quiz {quizId} has not been started.");
        }

        var currentQuestion = attempt.CompletedAt is null
            ? quiz.Questions
                .Where(question => attempt.Answers.All(answer => answer.QuestionId != question.Id))
                .Select(question => new CurrentQuestionDto(
                    question.Id,
                    question.Text,
                    question.Options.Select(option => option.Text).ToList()))
                .FirstOrDefault()
            : null;
        var result = attempt.CompletedAt is null
            ? null
            : new QuizResultDto(attempt.EarnedXp(), quiz.TotalXp(), attempt.EarnedXp() * 100 / quiz.TotalXp());

        return new QuizAttemptDto(quiz.Id, quiz.Name, quiz.Instructions, attempt.Answers.Count,
            quiz.Questions.Count, currentQuestion, result);
    }
}
