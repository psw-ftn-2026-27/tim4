using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.SocialChallengePreparation;
using Games.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengePreparation;

public sealed class SocialChallengePreparationQueryTests : BaseIntegrationTest
{
    private const string Url = "/api/games/social-challenges";

    public SocialChallengePreparationQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Administrator_sees_drafts_from_the_requested_page()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var page = await client.GetFromJsonAsync<PageResult<SocialChallengeDto>>(
            $"{Url}?page=2&pageSize=1", JsonOptions);

        page!.TotalCount.Should().Be(SocialChallengeSeed.All.Length);
        page.Items.Should().ContainSingle().Which.Id.Should().Be(SocialChallengeSeed.MorningDraft.Id);
    }

    [Fact]
    public async Task Administrator_sees_the_selected_challenge_details()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var expected = SocialChallengeSeed.EveningDraft;

        var challenge = await client.GetFromJsonAsync<SocialChallengeDto>($"{Url}/{expected.Id}", JsonOptions);

        challenge.Should().BeEquivalentTo(new
        {
            expected.Id,
            expected.Name,
            expected.ActivityTime.Start,
            expected.ActivityTime.End,
            expected.RequiredParticipants,
            expected.RangeInMeters,
            expected.XpReward,
            expected.CompletionMessage,
            expected.Status
        });
    }

    [Fact]
    public async Task Administrator_is_told_when_the_selected_challenge_does_not_exist()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync($"{Url}/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
