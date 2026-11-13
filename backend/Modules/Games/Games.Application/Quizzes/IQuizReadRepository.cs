using Games.Application.QuizAuthoring;

namespace Games.Application.Quizzes;

public interface IQuizReadRepository
{
    Task<List<ChallengeDto>> GetChallengesAsync();
    Task<QuizDto?> GetByIdAsync(Guid id);
}
