using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Domain.Quizzes;

public sealed class Question : Entity
{
    private const int MinimumOptionCount = 2;

    private readonly List<AnswerOption> _options = [];

    public string Text { get; private set; }
    public int XpReward { get; private set; }
    public IReadOnlyList<AnswerOption> Options => _options;

    private Question()
    {
        Text = null!;
    }

    public Question(string text, int xpReward, List<AnswerOption> options)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new DomainException("A question requires a text.");
        }
        if (xpReward <= 0)
        {
            throw new DomainException("A question requires a positive XP reward.");
        }
        if (options is null || options.Count < MinimumOptionCount)
        {
            throw new DomainException($"A question requires at least {MinimumOptionCount} answer options.");
        }
        if (!options.Any(option => option.IsCorrect))
        {
            throw new DomainException("A question requires at least one correct answer option.");
        }
        if (options.All(option => option.IsCorrect))
        {
            throw new DomainException("A question requires at least one incorrect answer option.");
        }

        Text = text;
        XpReward = xpReward;
        _options.AddRange(options);
    }
}
