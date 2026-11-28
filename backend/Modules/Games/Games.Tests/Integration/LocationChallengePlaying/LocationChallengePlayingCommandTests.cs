using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.LocationChallengePlaying;
using Games.Domain.Experience;
using Games.Domain.LocationChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengePlaying;

public sealed class LocationChallengePlayingCommandTests : BaseIntegrationTest
{
    private static readonly LocationChallenge Challenge = LocationChallengeSeed.SinglePointChallenge;
    private static readonly HiddenLocation Point = Challenge.HiddenLocations[0];

    public LocationChallengePlayingCommandTests(GamesApiFactory factory) : base(factory) { }

    [Fact]
    public async Task An_explorer_can_activate_when_the_configured_distance_allows_it()
    {
        using var configuredHost = Factory.WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Games:LocationChallenges:ActivationDistanceInMeters"] = "20"
                })));
        var userId = Guid.NewGuid();
        using var client = configuredHost.CreateClient();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", Factory.CreateAccessToken(userId, "explorer"));

        using var response = await client.PostAsJsonAsync(
            AttemptAddress(), new CheckLocationDto(Challenge.StartLatitude, Challenge.StartLongitude));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent, await response.Content.ReadAsStringAsync());
        using var context = Factory.CreateContext<GamesDbContext>();
        context.LocationChallengeAttempts.Should().ContainSingle(attempt =>
            attempt.ChallengeId == Challenge.Id && attempt.UserId == userId && attempt.CompletedAt == null);
    }

    [Fact]
    public async Task An_explorer_cannot_bypass_the_activation_distance()
    {
        using var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var attemptCountBefore = arrangeContext.LocationChallengeAttempts.Count();

        using var response = await client.PostAsJsonAsync(
            AttemptAddress(), new CheckLocationDto(Point.Latitude, Point.Longitude));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<GamesDbContext>();
        context.LocationChallengeAttempts.Count().Should().Be(attemptCountBefore);
    }

    [Fact]
    public async Task The_last_discovery_is_saved_with_its_completion_time_and_award()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var attemptId = LocationChallengeAttemptSeed.ExplorerActiveAttempt.Id;
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var awardCountBefore = arrangeContext.XpAwards.Count();

        using var response = await client.PostAsJsonAsync(Address(), new CheckLocationDto(Point.Latitude, Point.Longitude));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<GamesDbContext>();
        var attempt = context.LocationChallengeAttempts.Single(attempt => attempt.Id == attemptId);
        attempt.DiscoveredPointIds.Should().ContainSingle().Which.Should().Be(Point.PointId);
        attempt.CompletedAt.Should().NotBeNull();
        attempt.AwardedXp.Should().Be(25);
        context.XpAwards.Count().Should().Be(awardCountBefore + 1);
        var award = context.XpAwards.Single(award => award.UserId == WellKnownUsers.Explorer
            && award.ChallengeType == ChallengeType.Location && award.ChallengeId == Challenge.Id);
        award.Xp.Should().Be(attempt.AwardedXp);
        award.EarnedAt.Should().Be(attempt.CompletedAt!.Value);
    }

    [Fact]
    public async Task Repeating_the_last_discovery_keeps_a_single_xp_award()
    {
        var award = EarnedXpSeed.SecondExplorerLocationAward;
        using var client = Factory.CreateClientFor(award.UserId, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var awardCountBefore = arrangeContext.XpAwards.Count();

        using var response = await client.PostAsJsonAsync(Address(), new CheckLocationDto(Point.Latitude, Point.Longitude));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<GamesDbContext>();
        context.XpAwards.Count().Should().Be(awardCountBefore);
        context.XpAwards.Should().Contain(item => item.Id == award.Id);
    }

    [Fact]
    public async Task Checking_from_too_far_away_does_not_record_progress_or_xp()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var attemptId = LocationChallengeAttemptSeed.ExplorerActiveAttempt.Id;
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var awardCountBefore = arrangeContext.XpAwards.Count();

        using var response = await client.PostAsJsonAsync(
            Address(), new CheckLocationDto(Challenge.StartLatitude, Challenge.StartLongitude));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<GamesDbContext>();
        var attempt = context.LocationChallengeAttempts.Single(attempt => attempt.Id == attemptId);
        attempt.DiscoveredPointIds.Should().BeEmpty();
        attempt.AwardedXp.Should().Be(0);
        attempt.CompletedAt.Should().BeNull();
        context.XpAwards.Count().Should().Be(awardCountBefore);
    }

    [Fact]
    public async Task An_explorer_cannot_check_a_point_on_another_explorers_attempt()
    {
        using var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        using var response = await client.PostAsJsonAsync(Address(), new CheckLocationDto(Point.Latitude, Point.Longitude));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("administrator", HttpStatusCode.Forbidden)]
    public async Task Only_an_explorer_can_check_a_location(string? role, HttpStatusCode expected)
    {
        using var client = role is null ? Factory.CreateClient() : Factory.CreateClientFor(WellKnownUsers.Administrator, role);

        using var response = await client.PostAsJsonAsync(Address(), new CheckLocationDto(Point.Latitude, Point.Longitude));

        response.StatusCode.Should().Be(expected);
    }

    private static string AttemptAddress() => $"/api/games/location-challenges/{Challenge.Id}/attempt";
    private static string Address() => $"{AttemptAddress()}/points/{Point.PointId}/check";
}
