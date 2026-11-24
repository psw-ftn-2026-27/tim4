using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Games.Application.SocialChallengePlaying;
using Games.Domain.Experience;
using Games.Domain.LocationChallenges;
using Games.Domain.SocialChallengeRuns;
using Games.Domain.SocialChallenges;
using Games.Infrastructure.Persistence;
using Games.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Games.Tests.Integration.SocialChallengePlaying;

public sealed class SocialChallengePlayingCommandTests : BaseIntegrationTest
{
    private const string Url = "/api/games/social-challenges";

    private static readonly SocialChallengePositionDto NearAllDayChallenge = new(
        SocialChallengeSeed.PublishedAllDay.Latitude!.Value, SocialChallengeSeed.PublishedAllDay.Longitude!.Value);
    private static readonly SocialChallengePositionDto FarFromAllDayChallenge = new(45.2551, 19.8452);

    private readonly SocialChallenge _waitingChallenge;
    private readonly SocialChallengeRun _waitingRun;
    private readonly Guid _waitingPlayer = Guid.NewGuid();

    public SocialChallengePlayingCommandTests(GamesApiFactory factory) : base(factory)
    {
        var location = new ChallengeLocation(45.2396, 19.8227);
        _waitingChallenge = new SocialChallenge("Susret koji čeka", new ActivityTime(TimeOnly.MinValue, TimeOnly.MaxValue),
            2, 20, 30, "Stigli ste!", location.Latitude, location.Longitude);
        _waitingChallenge.Publish();
        var now = DateTime.UtcNow;
        _waitingRun = new SocialChallengeJoining().Join(_waitingChallenge, null, [], Guid.NewGuid(), location, now);
        new SocialChallengeJoining().Join(_waitingChallenge, _waitingRun, [], _waitingPlayer, location, now);
        Factory.Reseed<GamesDbContext>([.. GamesSeed.All, _waitingChallenge, _waitingRun]);
    }

    [Fact]
    public async Task Explorer_near_a_challenge_with_a_solved_run_becomes_a_participant_of_a_new_run()
    {
        var playerId = Guid.NewGuid();
        using var client = Factory.CreateClientFor(playerId, "explorer");
        var challengeId = SocialChallengeSeed.PublishedAllDay.Id;

        var response = await client.PostAsJsonAsync($"{Url}/{challengeId}/run", NearAllDayChallenge);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<GamesDbContext>();
        var run = context.SocialChallengeRuns.Single(run => run.ChallengeId == challengeId && run.SolvedAt == null);
        run.Id.Should().NotBe(SocialChallengeRunSeed.SolvedAllDayRun.Id);
        run.Participants.Should().ContainSingle().Which.UserId.Should().Be(playerId);
    }

    [Fact]
    public async Task Activation_from_outside_the_range_is_rejected_and_no_run_is_created()
    {
        using var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var runCountBefore = arrangeContext.SocialChallengeRuns.Count();

        var response = await client.PostAsJsonAsync(
            $"{Url}/{SocialChallengeSeed.PublishedAllDay.Id}/run", FarFromAllDayChallenge);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<GamesDbContext>();
        context.SocialChallengeRuns.Count().Should().Be(runCountBefore);
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("administrator", HttpStatusCode.Forbidden)]
    public async Task Only_an_explorer_can_activate_a_challenge(string? role, HttpStatusCode expected)
    {
        using var client = role is null ? Factory.CreateClient() : Factory.CreateClientFor(WellKnownUsers.Administrator, role);

        var response = await client.PostAsJsonAsync(
            $"{Url}/{SocialChallengeSeed.PublishedAllDay.Id}/run", NearAllDayChallenge);

        response.StatusCode.Should().Be(expected);
    }

    [Fact]
    public async Task Location_that_completes_the_required_players_solves_the_run_and_stores_the_award()
    {
        using var client = Factory.CreateClientFor(_waitingPlayer, "explorer");
        using var arrangeContext = Factory.CreateContext<GamesDbContext>();
        var awardCountBefore = arrangeContext.XpAwards.Count();

        var response = await client.PostAsJsonAsync(
            $"{Url}/{_waitingChallenge.Id}/run/location", new SocialChallengePositionDto(45.2396, 19.8227));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<GamesDbContext>();
        var run = context.SocialChallengeRuns.Single(run => run.Id == _waitingRun.Id);
        run.SolvedAt.Should().NotBeNull();
        run.AwardedXp.Should().Be(_waitingChallenge.XpReward);
        run.Participants.Select(participant => participant.UserId).Should()
            .BeEquivalentTo(_waitingRun.Participants.Select(participant => participant.UserId));
        context.XpAwards.Count().Should().Be(awardCountBefore + _waitingRun.Participants.Count);
        context.XpAwards
            .Where(award => award.ChallengeType == ChallengeType.Social && award.ChallengeId == _waitingChallenge.Id)
            .Select(award => award.UserId).Should()
            .BeEquivalentTo(_waitingRun.Participants.Select(participant => participant.UserId));
    }

    [Fact]
    public async Task Location_for_a_challenge_that_does_not_exist_is_rejected()
    {
        using var client = Factory.CreateClientFor(_waitingPlayer, "explorer");
        var challengeId = Guid.NewGuid();

        var response = await client.PostAsJsonAsync(
            $"{Url}/{challengeId}/run/location", new SocialChallengePositionDto(45.2396, 19.8227));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var context = Factory.CreateContext<GamesDbContext>();
        context.SocialChallengeRuns.Any(run => run.ChallengeId == challengeId).Should().BeFalse();
    }

    [Theory]
    [InlineData(null, HttpStatusCode.Unauthorized)]
    [InlineData("administrator", HttpStatusCode.Forbidden)]
    public async Task Only_an_explorer_can_report_a_location(string? role, HttpStatusCode expected)
    {
        using var client = role is null ? Factory.CreateClient() : Factory.CreateClientFor(WellKnownUsers.Administrator, role);

        var response = await client.PostAsJsonAsync(
            $"{Url}/{_waitingChallenge.Id}/run/location", new SocialChallengePositionDto(45.2396, 19.8227));

        response.StatusCode.Should().Be(expected);
    }
}
