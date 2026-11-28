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
    public async Task Range_with_only_a_lower_bound_finds_tours_priced_at_or_above_it()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?priceFrom=25&pageSize=100", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedVineyardsByBicycleEasy.Id);
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedMonasteries.Id);
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedWineAndHeritageWalk.Id);
    }

    [Fact]
    public async Task Range_with_only_an_upper_bound_finds_tours_priced_at_or_below_it()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?priceTo=25&pageSize=100", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedMonasteries.Id);
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedVineyardsByBicycleEasy.Id);
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedWineAndHeritageWalk.Id);
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

    [Fact]
    public async Task Length_range_finds_tours_with_a_length_within_it_including_its_bounds()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?lengthFrom=1.3&lengthTo=2.1&pageSize=100", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedVineyardsByBicycleEasy.Id && tour.Tour.LengthInKm == TourSeed.PublishedVineyardsByBicycleEasy.LengthInKm);
        tours.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedWineAndHeritageWalk.Id && tour.Tour.LengthInKm == TourSeed.PublishedWineAndHeritageWalk.LengthInKm);
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedMonasteries.Id);
    }

    [Fact]
    public async Task Length_range_with_bounds_in_reverse_order_finds_the_tours_between_them()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?lengthFrom=2.1&lengthTo=1.3&pageSize=100", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedVineyardsByBicycleEasy.Id);
        tours.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedWineAndHeritageWalk.Id);
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedMonasteries.Id);
    }

    [Fact]
    public async Task Length_range_together_with_a_price_range_finds_only_tours_meeting_both()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?lengthFrom=1&priceFrom=20&pageSize=100", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedVineyardsByBicycleEasy.Id);
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedVineyards.Id);
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedMonasteries.Id);
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedWineAndHeritageWalk.Id);
    }

    [Fact]
    public async Task Circle_finds_a_tour_with_only_a_later_key_point_in_it_pinned_at_its_first_open_key_point_and_not_a_tour_with_every_key_point_outside()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var laterKeyPoint = TourSeed.PublishedMonasteries.KeyPoints[1].Location;
        var firstKeyPoint = TourSeed.PublishedMonasteries.KeyPoints[0].Location;

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>(FormattableString.Invariant($"/api/exploration/tours/published?latitude={laterKeyPoint.Latitude}&longitude={laterKeyPoint.Longitude}&radiusInKm=1&pageSize=100"), JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedMonasteries.Id
            && tour.PinLocation == new LocationDto(firstKeyPoint.Latitude, firstKeyPoint.Longitude));
        tours.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedVineyards.Id);
    }

    [Fact]
    public async Task Tour_with_only_a_key_point_hidden_until_purchase_in_the_circle_is_not_found()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var hiddenKeyPoint = TourSeed.PublishedFortressWithHiddenFirstKeyPoint.KeyPoints[0].Location;

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>(FormattableString.Invariant($"/api/exploration/tours/published?latitude={hiddenKeyPoint.Latitude}&longitude={hiddenKeyPoint.Longitude}&radiusInKm=0.5&pageSize=100"), JsonOptions);

        tours!.Items.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedFortressWithHiddenFirstKeyPoint.Id);
    }

    [Fact]
    public async Task Tour_whose_first_key_point_is_hidden_until_purchase_is_pinned_at_its_first_open_key_point()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var firstOpenKeyPoint = TourSeed.PublishedFortressWithHiddenFirstKeyPoint.KeyPoints[1].Location;

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?pageSize=100", JsonOptions);

        tours!.Items.Should().ContainSingle(tour => tour.Tour.Id == TourSeed.PublishedFortressWithHiddenFirstKeyPoint.Id)
            .Which.PinLocation.Should().Be(new LocationDto(firstOpenKeyPoint.Latitude, firstOpenKeyPoint.Longitude));
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
    public async Task Buyer_sees_every_key_point_including_those_hidden_until_purchase_and_none_of_their_secrets()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.PublishedFortressWithHiddenFirstKeyPoint;

        var details = await client.GetFromJsonAsync<TourDetailsDto>($"/api/exploration/tours/{tour.Id}", JsonOptions);

        details!.KeyPoints.Select(keyPoint => keyPoint.Id).Should().Equal(tour.KeyPoints.Select(keyPoint => keyPoint.Id));
        details.KeyPoints.Should().Contain(keyPoint => !keyPoint.IsVisibleWithoutPurchase);
        details.KeyPoints.Should().OnlyContain(keyPoint => keyPoint.Secret == null);
    }

    [Fact]
    public async Task Buyer_is_told_the_tour_is_bought()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var tour = TourSeed.PublishedVineyardsByBicycleEasy;

        var details = await client.GetFromJsonAsync<TourDetailsDto>($"/api/exploration/tours/{tour.Id}", JsonOptions);

        details!.IsPurchased.Should().BeTrue();
    }

    [Fact]
    public async Task Visitor_who_did_not_buy_the_tour_is_told_it_is_not_bought()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var tour = TourSeed.PublishedVineyards;

        var details = await client.GetFromJsonAsync<TourDetailsDto>($"/api/exploration/tours/{tour.Id}", JsonOptions);

        details!.IsPurchased.Should().BeFalse();
    }

    [Fact]
    public async Task Published_list_marks_the_tour_in_progress_only_for_its_tourist()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var inProgressTourId = TourExecutionSeed.ExplorersVineyardsByBicycleWithFirstKeyPointReached.TourId;

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?pageSize=100", JsonOptions);

        tours!.Items.Should().ContainSingle(tour => tour.IsInProgress)
            .Which.Tour.Id.Should().Be(inProgressTourId);
    }

    [Fact]
    public async Task Published_list_does_not_mark_another_tourists_tour_in_progress()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var inProgressTourId = TourExecutionSeed.ExplorersVineyardsByBicycleWithFirstKeyPointReached.TourId;

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?pageSize=100", JsonOptions);

        tours!.Items.Should().Contain(tour => tour.Tour.Id == inProgressTourId);
        tours.Items.Should().OnlyContain(tour => !tour.IsInProgress);
    }

    [Fact]
    public async Task Published_list_marks_only_the_tours_the_tourist_bought()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<PageResult<PublishedTourDto>>("/api/exploration/tours/published?pageSize=100", JsonOptions);

        tours!.Items.Where(tour => tour.IsPurchased).Select(tour => tour.Tour.Id).Should().BeEquivalentTo(
            [TourSeed.PublishedVineyardsByBicycleEasy.Id, TourSeed.PublishedFortressWithHiddenFirstKeyPoint.Id]);
        tours.Items.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedMonasteries.Id && !tour.IsPurchased);
    }

    [Fact]
    public async Task Visitor_sees_the_price_of_the_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var tour = TourSeed.PublishedVineyards;

        var details = await client.GetFromJsonAsync<TourDetailsDto>($"/api/exploration/tours/{tour.Id}", JsonOptions);

        details!.Price.Should().Be(TourProductSeed.VineyardsFor30.Price);
    }

    [Fact]
    public async Task Tourist_is_told_the_tour_is_in_progress()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var inProgressTourId = TourExecutionSeed.ExplorersVineyardsByBicycleWithFirstKeyPointReached.TourId;

        var details = await client.GetFromJsonAsync<TourDetailsDto>($"/api/exploration/tours/{inProgressTourId}", JsonOptions);

        details!.IsInProgress.Should().BeTrue();
    }

    [Fact]
    public async Task Another_tourist_is_told_the_tour_is_not_in_progress()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var inProgressTourId = TourExecutionSeed.ExplorersVineyardsByBicycleWithFirstKeyPointReached.TourId;

        var details = await client.GetFromJsonAsync<TourDetailsDto>($"/api/exploration/tours/{inProgressTourId}", JsonOptions);

        details!.IsInProgress.Should().BeFalse();
    }

    [Fact]
    public async Task Buyer_sees_the_details_of_an_archived_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var tour = TourSeed.ArchivedOldTownWalk;

        var details = await client.GetFromJsonAsync<TourDetailsDto>($"/api/exploration/tours/{tour.Id}", JsonOptions);

        details!.Id.Should().Be(tour.Id);
        details.IsPurchased.Should().BeTrue();
    }

    [Fact]
    public async Task Tourist_who_did_not_buy_an_archived_tour_is_told_it_does_not_exist()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var response = await client.GetAsync($"/api/exploration/tours/{TourSeed.ArchivedOldTownWalk.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Purchased_list_holds_the_paid_free_and_archived_tours_the_tourist_bought_and_not_a_tour_only_another_tourist_bought()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var tours = await client.GetFromJsonAsync<List<PurchasedTourDto>>("/api/exploration/tours/purchased", JsonOptions);

        tours.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedMonasteries.Id);
        tours.Should().Contain(tour => tour.Tour.Id == TourSeed.ArchivedOldTownWalk.Id && tour.Tour.Status == TourStatus.Archived);
        tours.Should().NotContain(tour => tour.Tour.Id == TourSeed.PublishedVineyardsByBicycleEasy.Id);
    }

    [Fact]
    public async Task Bought_product_that_is_not_a_tour_is_left_out_of_the_purchased_list()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var tours = await client.GetFromJsonAsync<List<PurchasedTourDto>>("/api/exploration/tours/purchased", JsonOptions);

        tours.Should().Contain(tour => tour.Tour.Id == TourSeed.PublishedVineyardsByBicycleEasy.Id);
        tours.Should().NotContain(tour => tour.Tour.Id == TourProductSeed.GameForFree.EntityId);
        tours.Should().OnlyContain(tour => TourSeed.All.Select(seeded => seeded.Id).Contains(tour.Tour.Id));
    }

    [Fact]
    public async Task Tourist_without_purchases_gets_an_empty_purchased_list()
    {
        var client = Factory.CreateClientFor(Guid.NewGuid(), "explorer");

        var tours = await client.GetFromJsonAsync<List<PurchasedTourDto>>("/api/exploration/tours/purchased", JsonOptions);

        tours.Should().BeEmpty();
    }

    [Fact]
    public async Task Purchased_list_marks_the_tour_in_progress()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var inProgressTourId = TourExecutionSeed.ExplorersVineyardsByBicycleWithFirstKeyPointReached.TourId;

        var tours = await client.GetFromJsonAsync<List<PurchasedTourDto>>("/api/exploration/tours/purchased", JsonOptions);

        tours.Should().ContainSingle(tour => tour.IsInProgress)
            .Which.Tour.Id.Should().Be(inProgressTourId);
    }

    [Fact]
    public async Task Purchased_tour_is_pinned_at_its_first_key_point_even_when_it_is_hidden_until_purchase()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var hiddenFirstKeyPoint = TourSeed.PublishedFortressWithHiddenFirstKeyPoint.KeyPoints[0].Location;

        var tours = await client.GetFromJsonAsync<List<PurchasedTourDto>>("/api/exploration/tours/purchased", JsonOptions);

        tours.Should().ContainSingle(tour => tour.Tour.Id == TourSeed.PublishedFortressWithHiddenFirstKeyPoint.Id)
            .Which.PinLocation.Should().Be(new LocationDto(hiddenFirstKeyPoint.Latitude, hiddenFirstKeyPoint.Longitude));
    }

    [Fact]
    public async Task Anonymous_user_has_no_purchased_list()
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync("/api/exploration/tours/purchased");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_has_no_purchased_list()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync("/api/exploration/tours/purchased");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Draft_tour_cannot_be_opened_by_a_visitor()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.GetAsync($"/api/exploration/tours/{TourSeed.FortressWalk.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}
