using Games.Application.Quizzes;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;

namespace Games.Application.QuizAuthoring;

public sealed class QuizAuthoringService
{
    private readonly IQuizRepository _quizRepository;
    private readonly IUnitOfWork _unitOfWork;

    public QuizAuthoringService(IQuizRepository quizRepository, IUnitOfWork unitOfWork)
    {
        _quizRepository = quizRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateAsync(QuizContentDto dto)
    {
        var quiz = new Quiz(dto.Name, dto.Instructions, CreateQuestions(dto.Questions));

        _quizRepository.Add(quiz);
        await _unitOfWork.SaveChangesAsync();

        return quiz.Id;
    }

    public async Task EditAsync(Guid quizId, QuizContentDto dto)
    {
        var quiz = await _quizRepository.GetByIdAsync(quizId);
        if (quiz is null)
        {
            throw new NotFoundException($"Quiz {quizId} does not exist.");
        }

        quiz.Edit(dto.Name, dto.Instructions, CreateQuestions(dto.Questions));
        await _unitOfWork.SaveChangesAsync();
    }

    private static List<Question> CreateQuestions(List<QuestionContentDto>? questions)
    {
        return (questions ?? [])
            .Select(question => new Question(
                question.Text,
                question.XpReward,
                (question.Options ?? [])
                    .Select(option => new AnswerOption(option.Text, option.IsCorrect))
                    .ToList()))
            .ToList();
    }
}
