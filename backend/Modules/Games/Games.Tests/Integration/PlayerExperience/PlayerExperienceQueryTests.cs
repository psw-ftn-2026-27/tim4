using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.PlayerExperience;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.PlayerExperience;

public sealed class PlayerExperienceQueryTests : BaseIntegrationTest
{
    private const string Url = "/api/games/experience";

    public PlayerExperienceQueryTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Explorer_sees_the_sum_of_all_xp_they_earned()
    {
        var userId = WellKnownUsers.SecondExplorer;
        using var client = Factory.CreateClientFor(userId, "explorer");
        var expectedXp = EarnedXpSeed.All.Where(award => award.UserId == userId).Sum(award => award.Xp);

        using var response = await client.GetAsync(Url);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TotalXpDto>(JsonOptions);
        body!.TotalXp.Should().Be(expectedXp);
    }

    [Fact]
    public async Task Explorer_who_has_earned_nothing_sees_zero_xp()
    {
        using var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        using var response = await client.GetAsync(Url);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TotalXpDto>(JsonOptions);
        body!.TotalXp.Should().Be(0);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("administrator", HttpStatusCode.Forbidden)]
    public async Task Only_an_explorer_can_see_their_xp(string? role, HttpStatusCode expected)
    {
        using var client = role is null ? Factory.CreateClient() : Factory.CreateClientFor(WellKnownUsers.Administrator, role);

        using var response = await client.GetAsync(Url);

        response.StatusCode.Should().Be(expected);
    }
}
