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

    private static Location CreateLocation() => new(45.2517, 19.8628);

    private static Quiz CreateQuizIn(QuizStatus status)
    {
        var quiz = new Quiz("Tajne tvrđave", "Odgovori na pitanja.", CreateQuestions(4), CreateLocation(), 150);
        if (status != QuizStatus.Draft)
        {
            quiz.Publish();
        }
        if (status == QuizStatus.Retired || status == QuizStatus.Archived)
        {
            quiz.Retire();
        }
        if (status == QuizStatus.Archived)
        {
            quiz.Archive();
        }
        return quiz;
    }

    [Fact]
    public void New_quiz_starts_as_a_draft()
    {
        var quiz = new Quiz("Tajne tvrđave", "Odgovori na pitanja.", CreateQuestions(4), CreateLocation(), 150);

        quiz.Status.Should().Be(QuizStatus.Draft);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(3)]
    public void Quiz_requires_at_least_four_questions(int questionCount)
    {
        var creation = () => new Quiz("Tajne tvrđave", "Odgovori na pitanja.", CreateQuestions(questionCount), CreateLocation(), 150);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("", "Odgovori na pitanja.")]
    [InlineData("   ", "Odgovori na pitanja.")]
    [InlineData("Tajne tvrđave", "")]
    [InlineData("Tajne tvrđave", "   ")]
    public void Quiz_requires_a_name_and_instructions(string name, string instructions)
    {
        var creation = () => new Quiz(name, instructions, CreateQuestions(4), CreateLocation(), 150);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Quiz_without_a_location_cannot_exist()
    {
        var creation = () => new Quiz("Tajne tvrđave", "Odgovori na pitanja.", CreateQuestions(4), null!, 150);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Quiz_with_an_activation_range_that_is_not_positive_cannot_exist(int rangeInMeters)
    {
        var creation = () => new Quiz("Tajne tvrđave", "Odgovori na pitanja.", CreateQuestions(4), CreateLocation(), rangeInMeters);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(QuizStatus.Published)]
    [InlineData(QuizStatus.Retired)]
    [InlineData(QuizStatus.Archived)]
    public void Quiz_that_is_not_a_draft_cannot_be_edited(QuizStatus status)
    {
        var quiz = CreateQuizIn(status);

        var editing = () => quiz.Edit("Nove tajne", "Nova uputstva.", CreateQuestions(5), CreateLocation(), 300);

        editing.Should().Throw<DomainException>();
        quiz.Name.Should().Be("Tajne tvrđave");
    }

    [Theory]
    [InlineData(QuizStatus.Draft)]
    [InlineData(QuizStatus.Retired)]
    public void Draft_or_retired_quiz_becomes_published(QuizStatus status)
    {
        var quiz = CreateQuizIn(status);

        quiz.Publish();

        quiz.Status.Should().Be(QuizStatus.Published);
    }

    [Theory]
    [InlineData(QuizStatus.Published)]
    [InlineData(QuizStatus.Archived)]
    public void Published_or_archived_quiz_cannot_be_published(QuizStatus status)
    {
        var quiz = CreateQuizIn(status);

        var publishing = () => quiz.Publish();

        publishing.Should().Throw<DomainException>();
        quiz.Status.Should().Be(status);
    }

    [Fact]
    public void Published_quiz_becomes_retired()
    {
        var quiz = CreateQuizIn(QuizStatus.Published);

        quiz.Retire();

        quiz.Status.Should().Be(QuizStatus.Retired);
    }

    [Theory]
    [InlineData(QuizStatus.Draft)]
    [InlineData(QuizStatus.Retired)]
    [InlineData(QuizStatus.Archived)]
    public void Quiz_that_is_not_published_cannot_be_retired(QuizStatus status)
    {
        var quiz = CreateQuizIn(status);

        var retiring = () => quiz.Retire();

        retiring.Should().Throw<DomainException>();
        quiz.Status.Should().Be(status);
    }

    [Fact]
    public void Retired_quiz_becomes_archived()
    {
        var quiz = CreateQuizIn(QuizStatus.Retired);

        quiz.Archive();

        quiz.Status.Should().Be(QuizStatus.Archived);
    }

    [Theory]
    [InlineData(QuizStatus.Draft)]
    [InlineData(QuizStatus.Published)]
    [InlineData(QuizStatus.Archived)]
    public void Quiz_that_is_not_retired_cannot_be_archived(QuizStatus status)
    {
        var quiz = CreateQuizIn(status);

        var archiving = () => quiz.Archive();

        archiving.Should().Throw<DomainException>();
        quiz.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(QuizStatus.Draft)]
    [InlineData(QuizStatus.Archived)]
    public void Draft_or_archived_quiz_may_be_deleted(QuizStatus status)
    {
        var quiz = CreateQuizIn(status);

        var checking = () => quiz.EnsureCanBeDeleted();

        checking.Should().NotThrow();
    }

    [Theory]
    [InlineData(QuizStatus.Published)]
    [InlineData(QuizStatus.Retired)]
    public void Published_or_retired_quiz_cannot_be_deleted(QuizStatus status)
    {
        var quiz = CreateQuizIn(status);

        var checking = () => quiz.EnsureCanBeDeleted();

        checking.Should().Throw<DomainException>();
        quiz.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(QuizStatus.Draft)]
    [InlineData(QuizStatus.Retired)]
    [InlineData(QuizStatus.Archived)]
    public void Quiz_that_is_not_published_cannot_be_solved(QuizStatus status)
    {
        var quiz = CreateQuizIn(status);

        var starting = () => quiz.EnsureCanStart(CreateLocation());

        starting.Should().Throw<DomainException>();
    }

    [Fact]
    public void Published_quiz_cannot_be_solved_from_outside_its_activation_range()
    {
        var quiz = CreateQuizIn(QuizStatus.Published);

        var starting = () => quiz.EnsureCanStart(new Location(45.2551, 19.8452));

        starting.Should().Throw<DomainException>();
    }

    [Fact]
    public void Total_xp_of_a_quiz_is_the_sum_of_its_question_rewards()
    {
        var quiz = new Quiz("Tajne tvrđave", "Odgovori na pitanja.",
            [CreateQuestion(100), CreateQuestion(50), CreateQuestion(80), CreateQuestion(70)], CreateLocation(), 150);

        var totalXp = quiz.TotalXp();

        totalXp.Should().Be(300);
    }
}
