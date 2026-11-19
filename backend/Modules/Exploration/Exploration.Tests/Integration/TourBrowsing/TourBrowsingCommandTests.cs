using System.Net;
using Exploration.Tests.Integration.Seeds;
using FluentAssertions;
using Payment.Infrastructure.Persistence;
using Shared.Tests;
using Xunit;

namespace Exploration.Tests.Integration.TourBrowsing;

public class TourBrowsingCommandTests : BaseIntegrationTest
{
    public TourBrowsingCommandTests(ExplorationApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Tourist_adds_a_tour_to_the_cart_and_the_cart_holds_an_item_for_that_tour()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");
        var tour = TourSeed.PublishedVineyards;
        using var arrangeContext = Factory.CreateContext<PaymentDbContext>();
        var itemCountBefore = arrangeContext.ShoppingCarts
            .Where(cart => cart.UserId == WellKnownUsers.SecondExplorer)
            .SelectMany(cart => cart.Items)
            .Count(item => item.EntityId == tour.Id);

        var response = await client.PostAsync($"/api/exploration/tours/{tour.Id}/cart", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.ShoppingCarts
            .Where(cart => cart.UserId == WellKnownUsers.SecondExplorer)
            .SelectMany(cart => cart.Items)
            .Count(item => item.EntityId == tour.Id)
            .Should().Be(itemCountBefore + 1);
    }

    [Fact]
    public async Task Anonymous_user_cannot_add_a_tour_to_the_cart()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/cart", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Administrator_cannot_add_a_tour_to_the_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.PostAsync($"/api/exploration/tours/{TourSeed.PublishedVineyards.Id}/cart", null);

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
