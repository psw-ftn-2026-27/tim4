using System.Net.Http.Json;
using Exploration.Application.TourPreferences;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourPreferences;

public class TourPreferencesQueryTests : BaseIntegrationTest
{
    public TourPreferencesQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Tourist_gets_their_own_preference_and_not_another_tourists_one()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var preference = await client.GetFromJsonAsync<TourPreferenceDto>("/api/exploration/preferences", JsonOptions);

        var expected = TourPreferenceSeed.ExplorersPreference;
        preference!.Difficulties.Should().Equal(expected.Criteria.Difficulties);
        preference.Transport.Should().Be(expected.Criteria.Transport);
        preference.Tags.Should().Equal(expected.Criteria.Tags);
        preference.OpenSearchWithPreferences.Should().Be(expected.OpenSearchWithPreferences);
    }
}
