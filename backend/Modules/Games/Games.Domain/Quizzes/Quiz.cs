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
    public Location Location { get; private set; }
    public int RangeInMeters { get; private set; }

    private Quiz()
    {
        Name = null!;
        Instructions = null!;
        Location = null!;
    }

    public Quiz(string name, string instructions, List<Question> questions, Location location, int rangeInMeters)
        : base(Guid.NewGuid())
    {
        CheckContent(name, instructions, questions, location, rangeInMeters);

        Name = name;
        Instructions = instructions;
        _questions.AddRange(questions);
        Location = location;
        RangeInMeters = rangeInMeters;
        Status = QuizStatus.Draft;
    }

    public void Edit(string name, string instructions, List<Question> questions, Location location, int rangeInMeters)
    {
        if (Status != QuizStatus.Draft)
        {
            throw new DomainException("Only a draft quiz can be edited.");
        }
        CheckContent(name, instructions, questions, location, rangeInMeters);

        Name = name;
        Instructions = instructions;
        _questions.Clear();
        _questions.AddRange(questions);
        Location = location;
        RangeInMeters = rangeInMeters;
    }

    public void Publish()
    {
        if (Status != QuizStatus.Draft && Status != QuizStatus.Retired)
        {
            throw new DomainException("Only a draft or retired quiz can be published.");
        }

        Status = QuizStatus.Published;
    }

    public void Retire()
    {
        if (Status != QuizStatus.Published)
        {
            throw new DomainException("Only a published quiz can be retired.");
        }

        Status = QuizStatus.Retired;
    }

    public void Archive()
    {
        if (Status != QuizStatus.Retired)
        {
            throw new DomainException("Only a retired quiz can be archived.");
        }

        Status = QuizStatus.Archived;
    }

    public void EnsureCanBeDeleted()
    {
        if (Status != QuizStatus.Draft && Status != QuizStatus.Archived)
        {
            throw new DomainException("Only a draft or archived quiz can be deleted.");
        }
    }

    public void EnsureCanStart(Location position)
    {
        if (Status != QuizStatus.Published)
        {
            throw new DomainException("Only a published quiz can be solved.");
        }
        if (Location.DistanceInMetersTo(position) > RangeInMeters)
        {
            throw new DomainException("Move closer to the quiz location to start it.");
        }
    }

    public int TotalXp()
    {
        return _questions.Sum(question => question.XpReward);
    }

    private static void CheckContent(string name, string instructions, List<Question> questions,
        Location location, int rangeInMeters)
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
        if (location is null)
        {
            throw new DomainException("A quiz requires a location.");
        }
        if (rangeInMeters <= 0)
        {
            throw new DomainException("A quiz requires a positive activation range in meters.");
        }
    }
}
