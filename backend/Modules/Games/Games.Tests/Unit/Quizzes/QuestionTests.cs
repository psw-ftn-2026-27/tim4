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

    [Theory]
    [InlineData(new[] { 0, 1 }, 100.0)]
    [InlineData(new[] { 0 }, 50.0)]
    [InlineData(new[] { 0, 1, 2 }, 75.0)]
    [InlineData(new[] { 0, 2 }, 25.0)]
    [InlineData(new[] { 0, 2, 3 }, 0.0)]
    [InlineData(new[] { 2, 3 }, 0.0)]
    [InlineData(new int[0], 0.0)]
    public void Correct_options_share_the_question_xp_incorrect_ones_take_a_quarter_and_the_score_is_never_negative(
        int[] selectedOptions, double expectedXp)
    {
        var question = new Question("Koji manastiri su na Fruškoj gori?", 100,
        [
            new AnswerOption("Grgeteg", true),
            new AnswerOption("Novo Hopovo", true),
            new AnswerOption("Žiča", false),
            new AnswerOption("Studenica", false)
        ]);

        var xp = question.Score(selectedOptions);

        xp.Should().Be(expectedXp);
    }
}
