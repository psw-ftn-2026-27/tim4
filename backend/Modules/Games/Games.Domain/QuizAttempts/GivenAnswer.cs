namespace Games.Domain.QuizAttempts;

public sealed record GivenAnswer
{
    public Guid QuestionId { get; private init; }
    public IReadOnlyList<int> SelectedOptions { get; private init; }
    public double EarnedXp { get; private init; }

    private GivenAnswer()
    {
        SelectedOptions = null!;
    }

    public GivenAnswer(Guid questionId, IReadOnlyList<int> selectedOptions, double earnedXp)
    {
        QuestionId = questionId;
        SelectedOptions = selectedOptions.ToList();
        EarnedXp = earnedXp;
    }
}
