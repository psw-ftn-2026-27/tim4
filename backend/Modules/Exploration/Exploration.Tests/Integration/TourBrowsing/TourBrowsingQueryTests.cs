using System.Net;
using System.Net.Http.Json;
using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourBrowsing;

public class TourBrowsingQueryTests : BaseIntegrationTest
{
    public TourBrowsingQueryTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Only_published_tours_are_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published", JsonOptions);

        tours!.Items.Should().OnlyContain(tour => tour.Status == TourStatus.Published);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.FortressWalk.Id);
        tours.TotalCount.Should().Be(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
    }

    [Fact]
    public async Task Archived_tour_is_not_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?pageSize=100", JsonOptions);

        tours!.Items.Should().NotContain(tour => tour.Id == TourSeed.ArchivedOldTownWalk.Id);
    }

    [Fact]
    public async Task Published_tours_are_listed_page_by_page()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?page=2&pageSize=1", JsonOptions);

        tours!.Items.Should().ContainSingle();
        tours.TotalCount.Should().Be(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
    }

    [Fact]
    public async Task Search_returns_only_tours_meeting_every_filled_criterion()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?name=vinograd&difficulties=Moderate&tags=priroda&transport=Bicycle", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedVineyardsByBicycleEasy.Id);
        tours.Items.Should().OnlyContain(tour => tour.Difficulty == TourDifficulty.Moderate);
    }

    [Fact]
    public async Task Search_with_several_difficulties_finds_tours_of_any_of_them()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?difficulties=Easy&difficulties=Hard", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedVineyardsByBicycleEasy.Id);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedMonasteries.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedWineAndHeritageWalk.Id);
    }

    [Fact]
    public async Task Tour_with_at_least_one_of_the_chosen_tags_is_found_and_tour_with_none_is_not()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?tags=istorija&tags=grad", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedMonasteries.Id);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedWineAndHeritageWalk.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
    }

    [Fact]
    public async Task Tag_search_ignores_letter_case()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?tags=ISTORIJA", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedMonasteries.Id);
    }

    [Fact]
    public async Task Title_search_ignores_letter_case()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?name=MANASTIRI", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedMonasteries.Id);
    }

    [Fact]
    public async Task Tour_without_a_time_for_the_chosen_transport_is_not_found()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?transport=Car", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedMonasteries.Id);
        tours.Items.Should().NotContain(tour => tour.Id == TourSeed.PublishedVineyards.Id);
    }

    [Fact]
    public async Task Descending_difficulty_sort_lists_harder_tours_before_easier_ones()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<TourDto>>("/api/exploration/tours/published?sortByDifficulty=Descending&pageSize=100", JsonOptions);

        tours!.Items.Select(tour => tour.Difficulty).Should().BeInDescendingOrder();
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedVineyardsByBicycleEasy.Id);
        tours.Items.Should().Contain(tour => tour.Id == TourSeed.PublishedMonasteries.Id);
    }

    [Fact]
    public async Task Visitor_sees_only_the_key_points_open_without_purchase_and_none_of_their_secrets()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var tour = TourSeed.PublishedVineyards;

        var details = await client.GetFromJsonAsync<TourDetailsDto>($"/api/exploration/tours/{tour.Id}", JsonOptions);

        details!.Name.Should().Be(tour.Name);
        details.LengthInKm.Should().Be(tour.LengthInKm);
        details.KeyPoints.Select(keyPoint => keyPoint.Id).Should()
            .Equal(tour.KeyPoints.Where(keyPoint => keyPoint.IsVisibleWithoutPurchase).Select(keyPoint => keyPoint.Id));
        details.KeyPoints.Should().OnlyContain(keyPoint => keyPoint.Secret == null);
    }

    [Fact]
    public async Task Author_sees_every_key_point_of_their_published_tour_with_its_secret()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.PublishedVineyards;

        var details = await client.GetFromJsonAsync<TourDetailsDto>($"/api/exploration/tours/{tour.Id}", JsonOptions);

        details!.KeyPoints.Select(keyPoint => keyPoint.Id).Should().Equal(tour.KeyPoints.Select(keyPoint => keyPoint.Id));
        details.KeyPoints.Select(keyPoint => keyPoint.Secret).Should().Equal(tour.KeyPoints.Select(keyPoint => keyPoint.Secret));
    }

    [Fact]
    public async Task Draft_tour_cannot_be_opened_by_a_visitor()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
