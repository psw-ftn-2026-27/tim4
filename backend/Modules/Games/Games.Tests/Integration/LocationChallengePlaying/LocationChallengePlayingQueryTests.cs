using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Games.Domain.LocationChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.LocationChallengePlaying;

public sealed class LocationChallengePlayingQueryTests : BaseIntegrationTest
{
    private const string OriginalPhoto = "/images/original-gps-photo.jpg";
    private const double HiddenLatitude = 45.264762;
    private const double HiddenLongitude = 19.868537;
    private readonly Guid _challengeId;
    private readonly Guid _pointId;
    private readonly Guid _activeExplorerId = Guid.NewGuid();
    private static readonly DateTime CompletionTime = new(2026, 11, 18, 12, 0, 0, DateTimeKind.Utc);

    public LocationChallengePlayingQueryTests(GamesApiFactory factory) : base(factory)
    {
        var start = new ChallengeLocation(45.25, 19.85);
        var location = new ChallengeLocation(HiddenLatitude, HiddenLongitude);
        var point = new HiddenLocation(location, OriginalPhoto);
        var challenge = new LocationChallenge("Lokacijski izazov", start, 25, [point]);
        var attempt = challenge.Activate(WellKnownUsers.Explorer, start, 0);
        attempt.CheckLocation(challenge, point.PointId, location, CompletionTime);
        var activeAttempt = challenge.Activate(_activeExplorerId, start, 0);
        _challengeId = challenge.Id;
        _pointId = point.PointId;
        Factory.Reseed<GamesDbContext>([.. GamesSeed.All, challenge, attempt, activeAttempt]);
    }

    [Fact]
    public async Task An_active_attempt_returns_the_clue_for_each_point_to_its_explorer()
    {
        using var client = Factory.CreateClientFor(_activeExplorerId, "explorer");

        using var response = await client.GetAsync(Address());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var attempt = body.GetProperty("attempt");
        var distance = new ChallengeLocation(45.25, 19.85).DistanceTo(
            new ChallengeLocation(HiddenLatitude, HiddenLongitude));
        attempt.GetProperty("radiusInMeters").GetDouble().Should().BeInRange(distance, distance + 100);
        var point = attempt.GetProperty("points").EnumerateArray().Should().ContainSingle().Subject;
        point.GetProperty("id").GetGuid().Should().Be(_pointId);
        point.GetProperty("imageAddress").GetString().Should().Be(OriginalPhoto);
        point.GetProperty("discovered").GetBoolean().Should().BeFalse();
    }

    [Fact]
    public async Task Explorer_sees_their_completion_xp_and_photo_without_hidden_coordinates()
    {
        using var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        using var response = await client.GetAsync(Address());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        var attempt = body.GetProperty("attempt");
        attempt.GetProperty("completedAt").GetDateTime().Should().Be(CompletionTime);
        attempt.GetProperty("awardedXp").GetInt32().Should().Be(25);
        var point = attempt.GetProperty("points").EnumerateArray().Should().ContainSingle().Subject;
        point.GetProperty("id").GetGuid().Should().Be(_pointId);
        point.GetProperty("discovered").GetBoolean().Should().BeTrue();
        point.GetProperty("imageAddress").GetString().Should().Be(OriginalPhoto);
        body.GetRawText().Should().NotContain("45.264762").And.NotContain("19.868537");
    }

    [Fact]
    public async Task Explorer_without_an_attempt_sees_challenge_details_without_another_explorers_progress()
    {
        using var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        using var response = await client.GetAsync(Address());

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("challenge").GetProperty("name").GetString().Should().Be("Lokacijski izazov");
        body.GetProperty("attempt").ValueKind.Should().Be(JsonValueKind.Null);
        body.GetRawText().Should().NotContain(_pointId.ToString()).And.NotContain(OriginalPhoto);
    }

    private string Address() => $"/api/games/location-challenges/{_challengeId}/attempt";
}
