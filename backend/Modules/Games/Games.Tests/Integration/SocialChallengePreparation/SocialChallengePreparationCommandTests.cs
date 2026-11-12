using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.SocialChallengePreparation;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengePreparation;

public sealed class SocialChallengePreparationCommandTests : BaseIntegrationTest
{
    private const string Url = "/api/games/social-challenges";

    public SocialChallengePreparationCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_receives_the_identifier_of_the_saved_challenge()
    {
        using var client = AdministratorClient();
        var details = NewDetails();

        var response = await client.PostAsJsonAsync(Url, details);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var id = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var context = Factory.CreateContext<GamesDbContext>();
        var stored = context.SocialChallenges.Single(challenge => challenge.Id == id);
        stored.Name.Should().Be(details.Name);
        stored.ActivityTime.Start.Should().Be(details.Start);
        stored.ActivityTime.End.Should().Be(details.End);
    }

    [Fact]
    public async Task Administrator_saves_changed_details_of_a_draft()
    {
        using var client = AdministratorClient();
        var id = SocialChallengeSeed.EveningDraft.Id;
        var details = NewDetails();

        var response = await client.PutAsJsonAsync($"{Url}/{id}", details);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<GamesDbContext>();
        var stored = context.SocialChallenges.Single(challenge => challenge.Id == id);
        stored.Should().BeEquivalentTo(new
        {
            details.Name,
            ActivityTime = new { details.Start, details.End },
            details.RequiredParticipants,
            details.RangeInMeters,
            details.XpReward,
            details.CompletionMessage
        });
    }

    [Fact]
    public async Task Administrator_removes_the_selected_draft()
    {
        using var client = AdministratorClient();
        var id = SocialChallengeSeed.EveningDraft.Id;

        var response = await client.DeleteAsync($"{Url}/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<GamesDbContext>();
        context.SocialChallenges.Any(challenge => challenge.Id == id).Should().BeFalse();
        context.SocialChallenges.Any(challenge => challenge.Id == SocialChallengeSeed.MorningDraft.Id).Should().BeTrue();
    }

    [Fact]
    public async Task Administrator_cannot_create_a_challenge_with_invalid_details()
    {
        using var client = AdministratorClient();
        var details = NewDetails() with { Name = "" };
        using var before = Factory.CreateContext<GamesDbContext>();
        var count = before.SocialChallenges.Count();

        var response = await client.PostAsJsonAsync(Url, details);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var after = Factory.CreateContext<GamesDbContext>();
        after.SocialChallenges.Count().Should().Be(count);
    }

    [Fact]
    public async Task Administrator_cannot_change_a_challenge_that_no_longer_exists()
    {
        using var client = AdministratorClient();
        var id = Guid.NewGuid();
        var details = NewDetails();

        var response = await client.PutAsJsonAsync($"{Url}/{id}", details);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var context = Factory.CreateContext<GamesDbContext>();
        context.SocialChallenges.Any(challenge => challenge.Id == id).Should().BeFalse();
    }

    [Fact]
    public async Task Administrator_cannot_remove_a_challenge_that_no_longer_exists()
    {
        using var client = AdministratorClient();
        var id = Guid.NewGuid();
        using var before = Factory.CreateContext<GamesDbContext>();
        var count = before.SocialChallenges.Count();

        var response = await client.DeleteAsync($"{Url}/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var after = Factory.CreateContext<GamesDbContext>();
        after.SocialChallenges.Count().Should().Be(count);
    }

    [Fact]
    public async Task Administrator_cannot_save_invalid_draft_details()
    {
        using var client = AdministratorClient();
        var draft = SocialChallengeSeed.EveningDraft;
        var details = NewDetails() with { XpReward = 0 };

        var response = await client.PutAsJsonAsync($"{Url}/{draft.Id}", details);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<GamesDbContext>();
        var stored = context.SocialChallenges.Single(challenge => challenge.Id == draft.Id);
        stored.Name.Should().Be(draft.Name);
        stored.XpReward.Should().Be(draft.XpReward);
    }

    [Theory]
    [InlineData(false, HttpStatusCode.Unauthorized)]
    [InlineData(true, HttpStatusCode.Forbidden)]
    public async Task Only_administrators_can_create_challenges(bool signedIn, HttpStatusCode expected)
    {
        using var client = NonAdministratorClient(signedIn);

        var response = await client.PostAsJsonAsync(Url, NewDetails());

        response.StatusCode.Should().Be(expected);
    }

    [Theory]
    [InlineData(false, HttpStatusCode.Unauthorized)]
    [InlineData(true, HttpStatusCode.Forbidden)]
    public async Task Only_administrators_can_edit_challenges(bool signedIn, HttpStatusCode expected)
    {
        using var client = NonAdministratorClient(signedIn);

        var response = await client.PutAsJsonAsync($"{Url}/{SocialChallengeSeed.EveningDraft.Id}", NewDetails());

        response.StatusCode.Should().Be(expected);
    }

    [Theory]
    [InlineData(false, HttpStatusCode.Unauthorized)]
    [InlineData(true, HttpStatusCode.Forbidden)]
    public async Task Only_administrators_can_delete_challenges(bool signedIn, HttpStatusCode expected)
    {
        using var client = NonAdministratorClient(signedIn);

        var response = await client.DeleteAsync($"{Url}/{SocialChallengeSeed.EveningDraft.Id}");

        response.StatusCode.Should().Be(expected);
    }

    private HttpClient AdministratorClient()
    {
        return Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
    }

    private HttpClient NonAdministratorClient(bool signedIn)
    {
        return signedIn ? Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer") : Factory.CreateClient();
    }

    private static SocialChallengeDetailsDto NewDetails()
    {
        return new SocialChallengeDetailsDto("Susret u parku", new TimeOnly(12, 30), new TimeOnly(14, 0),
            8, 30.5m, 100, "Čestitamo!");
    }
}
