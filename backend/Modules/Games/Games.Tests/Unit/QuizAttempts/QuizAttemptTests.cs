using FluentAssertions;
using Games.Domain.QuizAttempts;
using Shared.Domain.Exceptions;
using Xunit;

namespace Games.Tests.Unit.QuizAttempts;

public class QuizAttemptTests
{
    private static readonly DateTime AnsweredAt = new(2026, 11, 1, 10, 0, 0, DateTimeKind.Utc);

    private static QuizAttempt CreateSolvedAttempt()
    {
        var attempt = new QuizAttempt(Guid.NewGuid(), Guid.NewGuid());
        attempt.RecordAnswer(Guid.NewGuid(), [0], 100, true, AnsweredAt);
        return attempt;
    }

    [Fact]
    public void Attempt_is_not_solved_while_questions_remain_unanswered()
    {
        var attempt = new QuizAttempt(Guid.NewGuid(), Guid.NewGuid());

        attempt.RecordAnswer(Guid.NewGuid(), [0], 100, false, AnsweredAt);

        attempt.CompletedAt.Should().BeNull();
    }

    [Fact]
    public void Answer_to_the_last_question_solves_the_attempt_and_records_when_it_was_solved()
    {
        var attempt = new QuizAttempt(Guid.NewGuid(), Guid.NewGuid());

        attempt.RecordAnswer(Guid.NewGuid(), [0], 100, true, AnsweredAt);

        attempt.CompletedAt.Should().Be(AnsweredAt);
    }

    [Fact]
    public void Solved_attempt_accepts_no_new_answer()
    {
        var attempt = CreateSolvedAttempt();

        var answering = () => attempt.RecordAnswer(Guid.NewGuid(), [0], 100, true, AnsweredAt);

        answering.Should().Throw<DomainException>();
        attempt.Answers.Should().HaveCount(1);
    }

    [Fact]
    public void Solved_attempt_cannot_be_continued()
    {
        var attempt = CreateSolvedAttempt();

        var continuing = () => attempt.EnsureCanContinue();

        continuing.Should().Throw<DomainException>();
    }
}
