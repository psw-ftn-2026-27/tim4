using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.QuizSolving;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizSolving;

public class QuizSolvingCommandTests : BaseIntegrationTest
{
    public QuizSolvingCommandTests(GamesApiFactory factory) : base(factory) { }

    private static readonly QuizPositionDto NearMonasteriesQuiz = new(
        QuizSeed.PublishedMonasteriesQuiz.Location.Latitude, QuizSeed.PublishedMonasteriesQuiz.Location.Longitude);

    private static readonly QuizPositionDto FarFromMonasteriesQuiz = new(45.2551, 19.8452);

    [Fact]
    public async Task Explorer_near_a_quiz_starts_it_and_the_attempt_is_stored()
    {
        var playerId = Guid.NewGuid();
        var client = Factory.CreateClientFor(playerId, "explorer");
        var quizId = QuizSeed.PublishedMonasteriesQuiz.Id;

        var response = await client.PostAsJsonAsync($"/api/games/quizzes/{quizId}/attempt", NearMonasteriesQuiz);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.QuizAttempts.Should().ContainSingle(attempt =>
            attempt.QuizId == quizId && attempt.PlayerId == playerId && attempt.CompletedAt == null);
    }

    [Fact]
    public async Task Explorer_too_far_from_a_quiz_cannot_start_it_and_no_attempt_is_created()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.QuizAttempts.Count();

        var response = await client.PostAsJsonAsync(
            $"/api/games/quizzes/{QuizSeed.PublishedMonasteriesQuiz.Id}/attempt", FarFromMonasteriesQuiz);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.QuizAttempts.Count().Should().Be(attemptCountBefore);
    }

    [Fact]
    public async Task Explorer_cannot_start_a_quiz_that_does_not_exist()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.QuizAttempts.Count();

        var response = await client.PostAsJsonAsync($"/api/games/quizzes/{Guid.NewGuid()}/attempt", NearMonasteriesQuiz);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.QuizAttempts.Count().Should().Be(attemptCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_start_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/games/quizzes/{QuizSeed.PublishedMonasteriesQuiz.Id}/attempt", NearMonasteriesQuiz);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_start_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsJsonAsync(
            $"/api/games/quizzes/{QuizSeed.PublishedMonasteriesQuiz.Id}/attempt", NearMonasteriesQuiz);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Answer_to_the_last_question_is_stored_and_the_attempt_is_solved()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var seeded = QuizAttemptSeed.ExplorerMonasteriesAttemptBeforeLastQuestion;
        var lastQuestionId = QuizSeed.PublishedMonasteriesQuiz.Questions[^1].Id;

        var response = await client.PostAsJsonAsync(
            $"/api/games/quizzes/{seeded.QuizId}/attempt/questions/{lastQuestionId}/answer", new AnswerDto([0]));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var attempt = assertContext.QuizAttempts.Single(attempt => attempt.Id == seeded.Id);
        attempt.Answers.Should().HaveCount(seeded.Answers.Count + 1);
        attempt.Answers[^1].QuestionId.Should().Be(lastQuestionId);
        attempt.Answers[^1].SelectedOptions.Should().Equal(0);
        attempt.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Answer_to_an_already_answered_question_is_rejected_and_not_stored()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var seeded = QuizAttemptSeed.ExplorerMonasteriesAttemptBeforeLastQuestion;
        var answeredQuestionId = QuizSeed.PublishedMonasteriesQuiz.Questions[0].Id;

        var response = await client.PostAsJsonAsync(
            $"/api/games/quizzes/{seeded.QuizId}/attempt/questions/{answeredQuestionId}/answer", new AnswerDto([1]));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var attempt = assertContext.QuizAttempts.Single(attempt => attempt.Id == seeded.Id);
        attempt.Answers.Should().HaveCount(seeded.Answers.Count);
        attempt.CompletedAt.Should().BeNull();
    }

    [Fact]
    public async Task Answer_without_a_started_attempt_is_rejected()
    {
        var playerId = Guid.NewGuid();
        var client = Factory.CreateClientFor(playerId, "explorer");
        var quiz = QuizSeed.PublishedMonasteriesQuiz;

        var response = await client.PostAsJsonAsync(
            $"/api/games/quizzes/{quiz.Id}/attempt/questions/{quiz.Questions[0].Id}/answer", new AnswerDto([0]));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.QuizAttempts.Should().NotContain(attempt => attempt.PlayerId == playerId);
    }

    [Fact]
    public async Task Anonymous_user_cannot_answer_a_question()
    {
        var client = Factory.CreateClient();
        var quiz = QuizSeed.PublishedMonasteriesQuiz;

        var response = await client.PostAsJsonAsync(
            $"/api/games/quizzes/{quiz.Id}/attempt/questions/{quiz.Questions[^1].Id}/answer", new AnswerDto([0]));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_answer_a_question()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var quiz = QuizSeed.PublishedMonasteriesQuiz;

        var response = await client.PostAsJsonAsync(
            $"/api/games/quizzes/{quiz.Id}/attempt/questions/{quiz.Questions[^1].Id}/answer", new AnswerDto([0]));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
