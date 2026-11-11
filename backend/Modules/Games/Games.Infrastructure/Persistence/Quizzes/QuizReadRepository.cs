using Games.Application.QuizAuthoring;
using Games.Application.Quizzes;
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
                    .ToList()))
            .FirstOrDefaultAsync();
    }
}
