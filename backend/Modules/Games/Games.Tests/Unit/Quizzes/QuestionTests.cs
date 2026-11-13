using FluentAssertions;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.Quizzes;

public class QuestionTests
{
    [Fact]
    public void Question_requires_at_least_two_answer_options()
    {
        var creation = () => new Question("Iznad koje reke se nalazi tvrđava?", 50, [new AnswerOption("Dunav", true)]);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Question_requires_both_a_correct_and_an_incorrect_answer_option(bool allOptionsCorrect)
    {
        var creation = () => new Question("Iznad koje reke se nalazi tvrđava?", 50,
            [new AnswerOption("Dunav", allOptionsCorrect), new AnswerOption("Sava", allOptionsCorrect)]);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Question_requires_a_positive_xp_reward(int xpReward)
    {
        var creation = () => new Question("Iznad koje reke se nalazi tvrđava?", xpReward,
            [new AnswerOption("Dunav", true), new AnswerOption("Sava", false)]);

        creation.Should().Throw<DomainException>();
    }
}
