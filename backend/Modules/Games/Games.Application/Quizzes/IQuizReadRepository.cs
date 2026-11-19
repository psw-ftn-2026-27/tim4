using Games.Application.QuizAuthoring;
using Games.Application.QuizSolving;

namespace Games.Application.Quizzes;

public interface IQuizReadRepository
{
    Task<List<ChallengeDto>> GetChallengesAsync();
    Task<QuizDto?> GetByIdAsync(Guid id);
    Task<List<QuizMapItemDto>> GetQuizMapAsync(Guid playerId);
}
