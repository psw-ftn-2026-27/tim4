using Games.Domain.Experience;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.QuizAttempts;

public sealed class QuizAttempt : AggregateRoot
{
    private readonly List<GivenAnswer> _answers = [];

    public Guid QuizId { get; private set; }
    public Guid PlayerId { get; private set; }
    public IReadOnlyList<GivenAnswer> Answers => _answers;
    public DateTime? CompletedAt { get; private set; }

    private QuizAttempt() { }

    public QuizAttempt(Guid quizId, Guid playerId)
        : base(Guid.NewGuid())
    {
        QuizId = quizId;
        PlayerId = playerId;
    }

    public EarnedXp? RecordAnswer(Guid questionId, IReadOnlyList<int> selectedOptions, double earnedXp,
        bool isLastQuestion, DateTime now)
    {
        EnsureCanContinue();
        if (_answers.Any(answer => answer.QuestionId == questionId))
        {
            throw new DomainException("This question has already been answered.");
        }

        _answers.Add(new GivenAnswer(questionId, selectedOptions, earnedXp));
        if (!isLastQuestion)
        {
            return null;
        }

        CompletedAt = now;
        var roundedXp = (int)Math.Round(EarnedXp(), MidpointRounding.AwayFromZero);
        return new EarnedXp(PlayerId, ChallengeType.Quiz, QuizId, roundedXp, now);
    }

    public void EnsureCanContinue()
    {
        if (CompletedAt.HasValue)
        {
            throw new DomainException("This quiz has already been solved.");
        }
    }

    public double EarnedXp()
    {
        return _answers.Sum(answer => answer.EarnedXp);
    }
}
