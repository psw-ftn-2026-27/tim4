using Games.Application.QuizAuthoring;
using Games.Application.QuizSolving;
using Games.Application.Quizzes;
using Games.Domain.Quizzes;
using Microsoft.EntityFrameworkCore;

namespace Games.Infrastructure.Persistence.Quizzes;

internal sealed class QuizReadRepository : IQuizReadRepository
{
    private readonly GamesDbContext _dbContext;

    public QuizReadRepository(GamesDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<ChallengeDto>> GetChallengesAsync()
    {
        return _dbContext.Quizzes
            .OrderBy(quiz => quiz.Name)
            .Select(quiz => new ChallengeDto(
                quiz.Id,
                quiz.Name,
                quiz.Status,
                quiz.Questions.Sum(question => question.XpReward)))
            .ToListAsync();
    }

    public Task<QuizDto?> GetByIdAsync(Guid id)
    {
        return _dbContext.Quizzes
            .Where(quiz => quiz.Id == id)
            .Select(quiz => new QuizDto(
                quiz.Id,
                quiz.Name,
                quiz.Instructions,
                quiz.Status,
                quiz.Questions
                    .Select(question => new QuestionDto(
                        question.Id,
                        question.Text,
                        question.XpReward,
                        question.Options
                            .Select(option => new AnswerOptionDto(option.Text, option.IsCorrect))
                            .ToList()))
                    .ToList(),
                quiz.Location.Latitude,
                quiz.Location.Longitude,
                quiz.RangeInMeters))
            .FirstOrDefaultAsync();
    }

    public async Task<List<QuizMapItemDto>> GetQuizMapAsync(Guid playerId)
    {
        var quizzes = await _dbContext.Quizzes
            .Where(quiz => quiz.Status == QuizStatus.Published)
            .OrderBy(quiz => quiz.Name)
            .Select(quiz => new
            {
                quiz.Id,
                quiz.Name,
                quiz.Instructions,
                quiz.Location.Latitude,
                quiz.Location.Longitude
            })
            .ToListAsync();

        var quizIds = quizzes.Select(quiz => quiz.Id).ToList();
        var attempts = await _dbContext.QuizAttempts
            .Where(attempt => attempt.PlayerId == playerId && quizIds.Contains(attempt.QuizId))
            .Select(attempt => new
            {
                attempt.QuizId,
                attempt.CompletedAt,
                EarnedXp = attempt.Answers.Sum(answer => answer.EarnedXp)
            })
            .ToDictionaryAsync(attempt => attempt.QuizId);

        return quizzes
            .Select(quiz =>
            {
                var attempt = attempts.GetValueOrDefault(quiz.Id);
                var progress = attempt is null
                    ? QuizProgress.NotStarted
                    : attempt.CompletedAt is null ? QuizProgress.Started : QuizProgress.Solved;
                return new QuizMapItemDto(quiz.Id, quiz.Name, quiz.Instructions, quiz.Latitude, quiz.Longitude,
                    progress, progress == QuizProgress.Solved ? attempt!.EarnedXp : null);
            })
            .ToList();
    }
}
