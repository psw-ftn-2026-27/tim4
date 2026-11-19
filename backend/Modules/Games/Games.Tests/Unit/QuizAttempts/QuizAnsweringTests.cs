using FluentAssertions;
using Games.Domain.QuizAttempts;
using Games.Domain.Quizzes;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.QuizAttempts;

public class QuizAnsweringTests
{
    private static Quiz CreatePublishedQuiz()
    {
        var questions = Enumerable.Range(0, 4)
            .Select(_ => new Question("Kako se zove čuveni sat na tvrđavi?", 100,
                [new AnswerOption("Sahat kula", true), new AnswerOption("Toranj sveta", false)]))
            .ToList();
        var quiz = new Quiz("Tajne tvrđave", "Odgovori na pitanja.", questions, new Location(45.2517, 19.8628), 150);
        quiz.Publish();
        return quiz;
    }

    [Fact]
    public void Answer_to_a_question_that_is_not_next_in_order_is_rejected()
    {
        var quiz = CreatePublishedQuiz();
        var attempt = new QuizAttempt(quiz.Id, Guid.NewGuid());

        var answering = () => new QuizAnswering().Answer(quiz, attempt, quiz.Questions[1].Id, [0], DateTime.UtcNow);

        answering.Should().Throw<DomainException>();
        attempt.Answers.Should().BeEmpty();
    }
}
