using System.Net;
using FluentAssertions;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengePreparation;

public sealed class SocialChallengeLifecycleCommandTests : BaseIntegrationTest
{
    private const string Url = "/api/games/social-challenges";

    public SocialChallengeLifecycleCommandTests(GamesApiFactory factory) : base(factory) { }

    [Theory]
    [InlineData("publish", SocialChallengeStatus.Published)]
    [InlineData("retire", SocialChallengeStatus.Retired)]
    [InlineData("archive", SocialChallengeStatus.Archived)]
    public async Task Administrator_advances_the_challenge_lifecycle(string action, SocialChallengeStatus expected)
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var challenge = action switch
        {
            "publish" => SocialChallengeSeed.EveningDraft,
            "retire" => SocialChallengeSeed.Published,
            _ => SocialChallengeSeed.Retired
        };

        var response = await client.PostAsync($"{Url}/{challenge.Id}/{action}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<GamesDbContext>();
        context.SocialChallenges.Single(item => item.Id == challenge.Id).Status.Should().Be(expected);
    }

    [Theory]
    [InlineData("publish")]
    [InlineData("retire")]
    [InlineData("archive")]
    public async Task A_missing_challenge_cannot_change_state(string action)
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var id = Guid.NewGuid();

        var response = await client.PostAsync($"{Url}/{id}/{action}", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var context = Factory.CreateContext<GamesDbContext>();
        context.SocialChallenges.Any(item => item.Id == id).Should().BeFalse();
    }

    [Theory]
    [InlineData("publish")]
    [InlineData("retire")]
    [InlineData("archive")]
    public async Task An_invalid_lifecycle_transition_does_not_change_the_challenge(string action)
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var challenge = action == "publish" ? SocialChallengeSeed.Published : SocialChallengeSeed.EveningDraft;

        var response = await client.PostAsync($"{Url}/{challenge.Id}/{action}", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<GamesDbContext>();
        context.SocialChallenges.Single(item => item.Id == challenge.Id).Status.Should().Be(challenge.Status);
    }

    [Theory]
    [InlineData("publish", false, HttpStatusCode.Unauthorized)]
    [InlineData("retire", false, HttpStatusCode.Unauthorized)]
    [InlineData("archive", false, HttpStatusCode.Unauthorized)]
    [InlineData("publish", true, HttpStatusCode.Forbidden)]
    [InlineData("retire", true, HttpStatusCode.Forbidden)]
    [InlineData("archive", true, HttpStatusCode.Forbidden)]
    public async Task Only_administrators_can_change_challenge_state(string action, bool signedIn, HttpStatusCode expected)
    {
        using var client = signedIn
            ? Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer")
            : Factory.CreateClient();

        var response = await client.PostAsync($"{Url}/{SocialChallengeSeed.EveningDraft.Id}/{action}", null);

        response.StatusCode.Should().Be(expected);
    }
}
