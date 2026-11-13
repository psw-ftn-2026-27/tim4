using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed class Quiz : AggregateRoot
{
    private const int MinimumQuestionCount = 4;

    private readonly List<Question> _questions = [];

    public string Name { get; private set; }
    public string Instructions { get; private set; }
    public QuizStatus Status { get; private set; }
    public IReadOnlyList<Question> Questions => _questions;

    private Quiz()
    {
        Name = null!;
        Instructions = null!;
    }

    public Quiz(string name, string instructions, List<Question> questions)
        : base(Guid.NewGuid())
    {
        CheckContent(name, instructions, questions);

        Name = name;
        Instructions = instructions;
        _questions.AddRange(questions);
        Status = QuizStatus.Draft;
    }

    public void Edit(string name, string instructions, List<Question> questions)
    {
        if (Status != QuizStatus.Draft)
        {
            throw new DomainException("Only a draft quiz can be edited.");
        }
        CheckContent(name, instructions, questions);

        Name = name;
        Instructions = instructions;
        _questions.Clear();
        _questions.AddRange(questions);
    }

    public int TotalXp()
    {
        return _questions.Sum(question => question.XpReward);
    }

    private static void CheckContent(string name, string instructions, List<Question> questions)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A quiz requires a name.");
        }
        if (string.IsNullOrWhiteSpace(instructions))
        {
            throw new DomainException("A quiz requires instructions.");
        }
        if (questions is null || questions.Count < MinimumQuestionCount)
        {
            throw new DomainException($"A quiz requires at least {MinimumQuestionCount} questions.");
        }
    }
}
