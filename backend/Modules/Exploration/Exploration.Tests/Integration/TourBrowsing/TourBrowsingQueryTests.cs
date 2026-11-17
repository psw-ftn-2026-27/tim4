using System.Net.Http.Json;
using Exploration.Application.TourBrowsing;
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

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published", JsonOptions);

        tours!.Items.Should().OnlyContain(tour => tour.Tour.Status == TourStatus.Published);
        tours.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.FortressWalk.Id);
        tours.TotalCount.Should().Be(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
    }

    [Fact]
    public async Task Archived_tour_is_not_listed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?pageSize=100", JsonOptions);

        tours!.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.ArchivedOldTownWalk.Id);
    }

    [Fact]
    public async Task Published_tours_are_listed_page_by_page()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?page=2&pageSize=1", JsonOptions);

        tours!.Items.Should().ContainSingle();
        tours.TotalCount.Should().Be(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
    }

    [Fact]
    public async Task Search_returns_only_tours_meeting_every_filled_criterion()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?name=vinograd&difficulties=Moderate&tags=priroda&transport=Bicycle", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedVineyardsByBicycleEasy.Id);
        tours.Items.Should().OnlyContain(tour => tour.Tour.Difficulty == TourDifficulty.Moderate);
    }

    [Fact]
    public async Task Search_with_several_difficulties_finds_tours_of_any_of_them()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?difficulties=Easy&difficulties=Hard", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedVineyardsByBicycleEasy.Id);
        tours.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedMonasteries.Id);
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedWineAndHeritageWalk.Id);
    }

    [Fact]
    public async Task Tour_with_at_least_one_of_the_chosen_tags_is_found_and_tour_with_none_is_not()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?tags=istorija&tags=grad", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedMonasteries.Id);
        tours.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedWineAndHeritageWalk.Id);
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedVineyards.Id);
    }

    [Fact]
    public async Task Tag_search_ignores_letter_case()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?tags=ISTORIJA", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedMonasteries.Id);
    }

    [Fact]
    public async Task Title_search_ignores_letter_case()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?name=MANASTIRI", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedMonasteries.Id);
    }

    [Fact]
    public async Task Tour_without_a_time_for_the_chosen_transport_is_not_found()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?transport=Car", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedMonasteries.Id);
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedVineyards.Id);
    }

    [Fact]
    public async Task Descending_difficulty_sort_lists_harder_tours_before_easier_ones()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?sort=DifficultyDescending&pageSize=100", JsonOptions);

        tours!.Items.Select(tour => tour.Tour.Difficulty).Should().BeInDescendingOrder();
        tours.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedVineyardsByBicycleEasy.Id);
        tours.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedMonasteries.Id);
    }

    [Fact]
    public async Task Price_range_finds_tours_priced_within_it_and_no_tour_without_a_price()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?priceFrom=20&priceTo=50&pageSize=100", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedVineyards.Id && tour.Price == TourProductSeed.VineyardsFor30.Price);
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedVineyardsByBicycleEasy.Id);
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedWineAndHeritageWalk.Id);
        tours.Items.Should().OnlyContain(tour => tour.Price >= 20 && tour.Price <= 50);
    }

    [Fact]
    public async Task Free_tour_is_found_by_a_range_starting_at_zero()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?priceFrom=0&priceTo=10&pageSize=100", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedMonasteries.Id && tour.Price == 0);
    }

    [Fact]
    public async Task Descending_price_sort_lists_tours_without_a_price_last()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?sort=PriceDescending&pageSize=100", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedWineAndHeritageWalk.Id);
        tours.Items.TakeWhile(tour => tour.Price is not null).Select(tour => tour.Price).Should().BeInDescendingOrder();
        tours.Items.SkipWhile(tour => tour.Price is not null).Should().OnlyContain(tour => tour.Price == null);
    }

    [Fact]
    public async Task Range_with_bounds_in_reverse_order_finds_the_tours_between_them()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?priceFrom=50&priceTo=20&pageSize=100", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedVineyardsByBicycleEasy.Id);
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedMonasteries.Id);
    }

    [Fact]
    public async Task Second_page_of_the_price_sort_continues_the_order_of_the_first()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?sort=PriceAscending&page=2&pageSize=1", JsonOptions);

        tours!.Items.Should().ContainSingle(tour => tour.Tour.Id == TourSeed.PublishedVineyards.Id);
        tours.TotalCount.Should().Be(TourSeed.All.Count(tour => tour.Status == TourStatus.Published));
    }
}
