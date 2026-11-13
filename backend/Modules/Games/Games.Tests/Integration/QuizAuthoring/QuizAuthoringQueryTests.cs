using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.QuizAuthoring;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.QuizAuthoring;

public class QuizAuthoringQueryTests : BaseIntegrationTest
{
    public QuizAuthoringQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Challenge_list_shows_the_seeded_quizzes_with_their_total_xp()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var challenges = await client.GetFromJsonAsync<List<ChallengeDto>>("/api/games/quizzes", JsonOptions);

        challenges.Should().BeEquivalentTo(QuizSeed.All.Select(quiz =>
            new ChallengeDto(quiz.Id, quiz.Name, quiz.Status, quiz.TotalXp())));
    }

    [Fact]
    public async Task Quiz_view_shows_its_questions_and_answer_options()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var seeded = QuizSeed.DraftFortressQuiz;

        var quiz = await client.GetFromJsonAsync<QuizDto>($"/api/games/quizzes/{seeded.Id}", JsonOptions);

        quiz!.Id.Should().Be(seeded.Id);
        quiz.Questions.Should().BeEquivalentTo(
            seeded.Questions.Select(question => new QuestionDto(
                question.Id,
                question.Text,
                question.XpReward,
                question.Options.Select(option => new AnswerOptionDto(option.Text, option.IsCorrect)).ToList())),
            options => options.WithStrictOrdering());
    }
}
