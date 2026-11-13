using FluentAssertions;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.Quizzes;

public class QuizTests
{
    private static Question CreateQuestion(int xpReward) =>
        new("Kako se zove čuveni sat na tvrđavi?", xpReward, [new AnswerOption("Sahat kula", true), new AnswerOption("Toranj sveta", false)]);

    private static List<Question> CreateQuestions(int count) =>
        Enumerable.Range(0, count).Select(_ => CreateQuestion(100)).ToList();

    [Fact]
    public void New_quiz_starts_as_a_draft()
    {
        var quiz = new Quiz("Tajne tvrđave", "Odgovori na pitanja.", CreateQuestions(4));

        quiz.Status.Should().Be(QuizStatus.Draft);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public void Quiz_requires_at_least_four_questions(int questionCount)
    {
        var creation = () => new Quiz("Tajne tvrđave", "Odgovori na pitanja.", CreateQuestions(questionCount));

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("", "Odgovori na pitanja.")]
    [InlineData("   ", "Odgovori na pitanja.")]
    [InlineData("Tajne tvrđave", "")]
    [InlineData("Tajne tvrđave", "   ")]
    public void Quiz_requires_a_name_and_instructions(string name, string instructions)
    {
        var creation = () => new Quiz(name, instructions, CreateQuestions(4));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Total_xp_of_a_quiz_is_the_sum_of_its_question_rewards()
    {
        var quiz = new Quiz("Tajne tvrđave", "Odgovori na pitanja.",
            [CreateQuestion(100), CreateQuestion(50), CreateQuestion(80), CreateQuestion(70)]);

        var totalXp = quiz.TotalXp();

        totalXp.Should().Be(300);
    }
}
