using Games.Application.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizAuthoring;

public sealed class QuizAuthoringQueries
{
    private readonly IQuizReadRepository _quizReadRepository;

    public QuizAuthoringQueries(IQuizReadRepository quizReadRepository)
    {
        _quizReadRepository = quizReadRepository;
    }

    public Task<List<ChallengeDto>> GetChallengesAsync()
    {
        return _quizReadRepository.GetChallengesAsync();
    }

    public async Task<QuizDto> GetQuizAsync(Guid quizId)
    {
        var quiz = await _quizReadRepository.GetByIdAsync(quizId);
        if (quiz is null)
        {
            throw new NotFoundException($"Quiz {quizId} does not exist.");
        }
        return quiz;
    }
}
