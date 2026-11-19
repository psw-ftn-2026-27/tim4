using System.Net;
using FluentAssertions;
using Games.Domain.Quizzes;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizLifecycle;

public class QuizLifecycleCommandTests : BaseIntegrationTest
{
    public QuizLifecycleCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_publishes_a_draft_quiz_and_it_is_stored_as_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var quizId = QuizSeed.DraftFortressQuiz.Id;

        var response = await client.PostAsync($"/api/games/quizzes/{quizId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Single(quiz => quiz.Id == quizId).Status.Should().Be(QuizStatus.Published);
    }

    [Fact]
    public async Task Publishing_an_archived_quiz_is_rejected_and_the_quiz_stays_archived()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var quizId = QuizSeed.ArchivedVineyardsQuiz.Id;

        var response = await client.PostAsync($"/api/games/quizzes/{quizId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Single(quiz => quiz.Id == quizId).Status.Should().Be(QuizStatus.Archived);
    }

    [Fact]
    public async Task Publishing_a_quiz_that_does_not_exist_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var publishedCountBefore = arrangeContext.Quizzes.Count(quiz => quiz.Status == QuizStatus.Published);

        var response = await client.PostAsync($"/api/games/quizzes/{Guid.NewGuid()}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count(quiz => quiz.Status == QuizStatus.Published).Should().Be(publishedCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_publish_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.DraftFortressQuiz.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_publish_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.DraftFortressQuiz.Id}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_retires_a_published_quiz_and_it_is_stored_as_retired()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var quizId = QuizSeed.PublishedMonasteriesQuiz.Id;

        var response = await client.PostAsync($"/api/games/quizzes/{quizId}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Single(quiz => quiz.Id == quizId).Status.Should().Be(QuizStatus.Retired);
    }

    [Fact]
    public async Task Retiring_a_draft_quiz_is_rejected_and_the_quiz_stays_a_draft()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var quizId = QuizSeed.DraftFortressQuiz.Id;

        var response = await client.PostAsync($"/api/games/quizzes/{quizId}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Single(quiz => quiz.Id == quizId).Status.Should().Be(QuizStatus.Draft);
    }

    [Fact]
    public async Task Retiring_a_quiz_that_does_not_exist_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var retiredCountBefore = arrangeContext.Quizzes.Count(quiz => quiz.Status == QuizStatus.Retired);

        var response = await client.PostAsync($"/api/games/quizzes/{Guid.NewGuid()}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count(quiz => quiz.Status == QuizStatus.Retired).Should().Be(retiredCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_retire_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedMonasteriesQuiz.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_retire_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.PublishedMonasteriesQuiz.Id}/retire", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_archives_a_retired_quiz_and_it_is_stored_as_archived()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var quizId = QuizSeed.RetiredOldTownQuiz.Id;

        var response = await client.PostAsync($"/api/games/quizzes/{quizId}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Single(quiz => quiz.Id == quizId).Status.Should().Be(QuizStatus.Archived);
    }

    [Fact]
    public async Task Archiving_a_published_quiz_is_rejected_and_the_quiz_stays_published()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var quizId = QuizSeed.PublishedMonasteriesQuiz.Id;

        var response = await client.PostAsync($"/api/games/quizzes/{quizId}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Single(quiz => quiz.Id == quizId).Status.Should().Be(QuizStatus.Published);
    }

    [Fact]
    public async Task Archiving_a_quiz_that_does_not_exist_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var archivedCountBefore = arrangeContext.Quizzes.Count(quiz => quiz.Status == QuizStatus.Archived);

        var response = await client.PostAsync($"/api/games/quizzes/{Guid.NewGuid()}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count(quiz => quiz.Status == QuizStatus.Archived).Should().Be(archivedCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_archive_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.RetiredOldTownQuiz.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_archive_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync($"/api/games/quizzes/{QuizSeed.RetiredOldTownQuiz.Id}/archive", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_deletes_a_draft_quiz_and_it_no_longer_exists()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var quizId = QuizSeed.DraftFortressQuiz.Id;
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.DeleteAsync($"/api/games/quizzes/{quizId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore - 1);
        assertContext.Quizzes.Should().NotContain(quiz => quiz.Id == quizId);
    }

    [Fact]
    public async Task Deleting_a_published_quiz_is_rejected_and_the_quiz_remains()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var quizId = QuizSeed.PublishedMonasteriesQuiz.Id;

        var response = await client.DeleteAsync($"/api/games/quizzes/{quizId}");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Single(quiz => quiz.Id == quizId).Status.Should().Be(QuizStatus.Published);
    }

    [Fact]
    public async Task Deleting_a_quiz_that_does_not_exist_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.DeleteAsync($"/api/games/quizzes/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync($"/api/games/quizzes/{QuizSeed.DraftFortressQuiz.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_delete_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync($"/api/games/quizzes/{QuizSeed.DraftFortressQuiz.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
