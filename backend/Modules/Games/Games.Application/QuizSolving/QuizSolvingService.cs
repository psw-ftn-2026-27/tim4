using Games.Application.Experience;
using Games.Application.QuizAttempts;
using Games.Application.Quizzes;
using Games.Domain.QuizAttempts;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizSolving;

public sealed class QuizSolvingService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IQuizAttemptRepository _attemptRepository;
    private readonly IEarnedXpRepository _earnedXpRepository;
    private readonly QuizAnswering _quizAnswering;
    private readonly IUnitOfWork _unitOfWork;

    public QuizSolvingService(IQuizRepository quizRepository, IQuizAttemptRepository attemptRepository,
        IEarnedXpRepository earnedXpRepository, QuizAnswering quizAnswering, IUnitOfWork unitOfWork)
    {
        _quizRepository = quizRepository;
        _attemptRepository = attemptRepository;
        _earnedXpRepository = earnedXpRepository;
        _quizAnswering = quizAnswering;
        _unitOfWork = unitOfWork;
    }

    public async Task StartAsync(Guid quizId, Guid playerId, QuizPositionDto position)
    {
        var quiz = await GetQuizAsync(quizId);
        quiz.EnsureCanStart(new Location(position.Latitude, position.Longitude));

        var attempt = await _attemptRepository.GetByQuizAndPlayerAsync(quizId, playerId);
        if (attempt is null)
        {
            _attemptRepository.Add(new QuizAttempt(quizId, playerId));
        }
        else
        {
            attempt.EnsureCanContinue();
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task AnswerAsync(Guid quizId, Guid playerId, Guid questionId, AnswerDto dto)
    {
        var quiz = await GetQuizAsync(quizId);
        var attempt = await _attemptRepository.GetByQuizAndPlayerAsync(quizId, playerId);
        if (attempt is null)
        {
            throw new NotFoundException($"Quiz {quizId} has not been started.");
        }

        var award = _quizAnswering.Answer(quiz, attempt, questionId, dto.SelectedOptions ?? [], DateTime.UtcNow);
        if (award is not null)
        {
            _earnedXpRepository.Add(award);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<Quiz> GetQuizAsync(Guid quizId)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        if (quiz is null)
        {
            throw new NotFoundException($"Quiz {quizId} does not exist.");
        }

        return quiz;
    }
}
