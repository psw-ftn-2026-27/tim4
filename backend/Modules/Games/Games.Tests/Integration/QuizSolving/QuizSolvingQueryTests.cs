using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.QuizSolving;
using Games.Domain.Quizzes;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizSolving;

public class QuizSolvingQueryTests : BaseIntegrationTest
{
    public QuizSolvingQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Result_of_a_solved_quiz_shows_the_earned_xp_the_total_xp_and_the_percentage()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var seeded = QuizAttemptSeed.SecondExplorerSolvedMonasteriesAttempt;
        var quiz = QuizSeed.PublishedMonasteriesQuiz;

        var attempt = await client.GetFromJsonAsync<QuizAttemptDto>($"/api/games/quizzes/{quiz.Id}/attempt", JsonOptions);

        attempt!.CurrentQuestion.Should().BeNull();
        attempt.Result.Should().Be(new QuizResultDto(
            seeded.EarnedXp(), quiz.TotalXp(), seeded.EarnedXp() * 100 / quiz.TotalXp()));
    }

    [Fact]
    public async Task Quiz_map_shows_the_published_quizzes_with_the_players_progress_and_no_drafts()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var quizzes = await client.GetFromJsonAsync<List<QuizMapItemDto>>("/api/games/quizzes/map", JsonOptions);

        quizzes.Should().BeEquivalentTo(QuizSeed.All
            .Where(quiz => quiz.Status == QuizStatus.Published)
            .Select(quiz => new QuizMapItemDto(quiz.Id, quiz.Name, quiz.Instructions,
                quiz.Location.Latitude, quiz.Location.Longitude,
                quiz.Id == QuizAttemptSeed.ExplorerMonasteriesAttemptBeforeLastQuestion.QuizId
                    ? QuizProgress.Started
                    : QuizProgress.NotStarted,
                null)));
        quizzes.Should().NotContain(quiz => QuizSeed.All
            .Where(seeded => seeded.Status == QuizStatus.Draft)
            .Select(seeded => seeded.Id)
            .Contains(quiz.Id));
    }
}
