using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourPreferences;
using Exploration.Domain.Tours;
using Exploration.Infrastructure.Persistence;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourPreferences;

public class TourPreferencesCommandTests : BaseIntegrationTest
{
    public TourPreferencesCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Tourist_without_a_preference_saves_one()
    {
        var touristId = Guid.NewGuid();
        var client = Factory.CreateClientFor(touristId, "explorer");
        var request = new TourPreferenceDto([TourDifficulty.Easy], TransportMode.Bicycle, ["priroda"], true);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.PutAsJsonAsync("/api/exploration/preferences", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore + 1);
        assertContext.TourPreferences.Should().ContainSingle(preference => preference.TouristId == touristId);
    }

    [Fact]
    public async Task Tourist_with_a_preference_changes_it_instead_of_adding_another()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var request = new TourPreferenceDto([TourDifficulty.Hard], null, ["planina"], false);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.PutAsJsonAsync("/api/exploration/preferences", request);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore);
        var stored = assertContext.TourPreferences.Single(preference => preference.Id == TourPreferenceSeed.ExplorersPreference.Id);
        stored.Criteria.Difficulties.Should().Equal(TourDifficulty.Hard);
        stored.Criteria.Transport.Should().BeNull();
        stored.Criteria.Tags.Should().Equal("planina");
        stored.OpenSearchWithPreferences.Should().BeFalse();
    }

    [Fact]
    public async Task Preference_without_any_criterion_is_rejected()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");
        var request = new TourPreferenceDto([], null, [], true);
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.PutAsJsonAsync("/api/exploration/preferences", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_save_a_preference()
    {
        var client = Factory.CreateClient();
        var request = new TourPreferenceDto([TourDifficulty.Easy], null, [], true);

        var response = await client.PutAsJsonAsync("/api/exploration/preferences", request);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_save_a_preference()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var request = new TourPreferenceDto([TourDifficulty.Easy], null, [], true);

        var response = await client.PutAsJsonAsync("/api/exploration/preferences", request);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Tourist_deletes_their_preference()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.DeleteAsync("/api/exploration/preferences");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Should().NotContain(preference => preference.Id == TourPreferenceSeed.ExplorersPreference.Id);
    }

    [Fact]
    public async Task Tourist_without_a_preference_cannot_delete_one()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");
        using var arrangeContext = Factory.CreateContext<ExplorationDbContext>();
        var preferenceCountBefore = arrangeContext.TourPreferences.Count();

        var response = await client.DeleteAsync("/api/exploration/preferences");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var assertContext = Factory.CreateContext<ExplorationDbContext>();
        assertContext.TourPreferences.Count().Should().Be(preferenceCountBefore);
    }

    [Fact]
    public async Task Anonymous_user_cannot_delete_a_preference()
    {
        var client = Factory.CreateClient();

        var response = await client.DeleteAsync("/api/exploration/preferences");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_delete_a_preference()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.DeleteAsync("/api/exploration/preferences");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
