using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.QuizAuthoring;
using Games.Domain.Quizzes;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizAuthoring;

public class QuizAuthoringCommandTests : BaseIntegrationTest
{
    private static readonly QuizContentDto ValidContent = new("Fruškogorski manastiri", "Odgovori na pitanja o manastirima.",
    [
        new QuestionContentDto("Koji manastir je zadužbina Brankovića?", 100, [new AnswerOptionDto("Krušedol", true), new AnswerOptionDto("Studenica", false)]),
        new QuestionContentDto("Na kojoj planini su manastiri?", 50, [new AnswerOptionDto("Fruška gora", true), new AnswerOptionDto("Tara", false)]),
        new QuestionContentDto("Koji manastiri su na Fruškoj gori?", 80, [new AnswerOptionDto("Grgeteg", true), new AnswerOptionDto("Novo Hopovo", true), new AnswerOptionDto("Žiča", false)]),
        new QuestionContentDto("Koliko je manastira sačuvano na Fruškoj gori?", 70, [new AnswerOptionDto("Šesnaest", true), new AnswerOptionDto("Dva", false)])
    ]);

    public QuizAuthoringCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_creates_a_draft_quiz_with_its_questions()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.PostAsJsonAsync("/api/games/quizzes", ValidContent);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var createdId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore + 1);
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == createdId);
        stored.Status.Should().Be(QuizStatus.Draft);
        stored.Name.Should().Be(ValidContent.Name);
        stored.Questions.Select(question => question.Text)
            .Should().Equal(ValidContent.Questions.Select(question => question.Text));
        stored.Questions[2].Options.Should().HaveCount(3);
    }

    [Fact]
    public async Task Quiz_with_too_few_questions_is_not_created()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var content = ValidContent with { Questions = ValidContent.Questions.Take(3).ToList() };
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.PostAsJsonAsync("/api/games/quizzes", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_create_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/games/quizzes", ValidContent);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_create_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync("/api/games/quizzes", ValidContent);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Administrator_replaces_the_content_of_a_draft_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var quizId = QuizSeed.DraftFortressQuiz.Id;

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{quizId}", ValidContent);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        var stored = assertContext.Quizzes.Single(quiz => quiz.Id == quizId);
        stored.Name.Should().Be(ValidContent.Name);
        stored.Instructions.Should().Be(ValidContent.Instructions);
        stored.Questions.Select(question => question.Text)
            .Should().Equal(ValidContent.Questions.Select(question => question.Text));
    }

    [Fact]
    public async Task Editing_a_quiz_that_does_not_exist_is_rejected()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var quizCountBefore = arrangeContext.Quizzes.Count();

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{Guid.NewGuid()}", ValidContent);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<GamesDbContext>();
        assertContext.Quizzes.Count().Should().Be(quizCountBefore);
        assertContext.Quizzes.Should().NotContain(quiz => quiz.Name == ValidContent.Name);
    }

    [Fact]
    public async Task Anonymous_user_cannot_edit_a_quiz()
    {
        var client = Factory.CreateClient();

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{QuizSeed.DraftFortressQuiz.Id}", ValidContent);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Explorer_cannot_edit_a_quiz()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PutAsJsonAsync($"/api/games/quizzes/{QuizSeed.DraftFortressQuiz.Id}", ValidContent);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
