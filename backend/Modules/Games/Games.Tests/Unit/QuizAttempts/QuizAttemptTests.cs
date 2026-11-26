using FluentAssertions;
using Games.Domain.Experience;
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
    public void Attempt_is_not_solved_while_questions_remain_unanswered_and_earns_no_xp()
    {
        var attempt = new QuizAttempt(Guid.NewGuid(), Guid.NewGuid());

        var award = attempt.RecordAnswer(Guid.NewGuid(), [0], 100, false, AnsweredAt);

        attempt.CompletedAt.Should().BeNull();
        award.Should().BeNull();
    }

    [Fact]
    public void Solving_the_attempt_awards_the_player_the_xp_earned_on_all_answers()
    {
        var quizId = Guid.NewGuid();
        var playerId = Guid.NewGuid();
        var attempt = new QuizAttempt(quizId, playerId);
        attempt.RecordAnswer(Guid.NewGuid(), [0], 40, false, AnsweredAt);

        var award = attempt.RecordAnswer(Guid.NewGuid(), [0], 60, true, AnsweredAt);

        award.Should().NotBeNull();
        award!.UserId.Should().Be(playerId);
        award.ChallengeType.Should().Be(ChallengeType.Quiz);
        award.ChallengeId.Should().Be(quizId);
        award.Xp.Should().Be(100);
        award.EarnedAt.Should().Be(AnsweredAt);
    }

    [Theory]
    [InlineData(12.4, 12)]
    [InlineData(12.5, 13)]
    [InlineData(12.6, 13)]
    public void Earned_xp_is_rounded_to_the_nearest_whole_number(double earnedXp, int expectedXp)
    {
        var attempt = new QuizAttempt(Guid.NewGuid(), Guid.NewGuid());

        var award = attempt.RecordAnswer(Guid.NewGuid(), [0], earnedXp, true, AnsweredAt);

        award!.Xp.Should().Be(expectedXp);
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
