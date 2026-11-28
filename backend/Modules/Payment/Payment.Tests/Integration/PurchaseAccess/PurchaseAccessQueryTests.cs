using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Payment.Contracts;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.PurchaseAccess;

public sealed class PurchaseAccessQueryTests : BaseIntegrationTest
{
    public PurchaseAccessQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Completed_purchases_grant_access_to_the_buyer_including_free_content(bool free)
    {
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        var purchase = free ? PurchaseSeed.CompletedFree : PurchaseSeed.Completed;
        var content = purchase.Items.Single().EntityId;

        var purchased = await api.HasPurchasedAsync(PurchaseSeed.Buyer, content);

        purchased.Should().BeTrue();
    }

    [Fact]
    public async Task Unpurchased_content_does_not_grant_access()
    {
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var purchased = await api.HasPurchasedAsync(PurchaseSeed.Buyer, ProductSeed.ExpensiveOffer.EntityId);

        purchased.Should().BeFalse();
    }

    [Fact]
    public async Task Another_buyers_purchase_does_not_grant_access()
    {
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var purchased = await api.HasPurchasedAsync(WellKnownUsers.Explorer, ProductSeed.ExistingOffer.EntityId);

        purchased.Should().BeFalse();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Cart_items_do_not_grant_access_before_purchase(bool prepared)
    {
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        var cart = prepared ? ShoppingCartSeed.Ready : ShoppingCartSeed.Unprepared;

        var purchased = await api.HasPurchasedAsync(cart.UserId, cart.Items.Single().EntityId);

        purchased.Should().BeFalse();
    }

    [Fact]
    public async Task A_buyers_owned_content_list_contains_all_completed_purchases()
    {
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        var expected = PurchaseSeed.All.Where(purchase => purchase.UserId == PurchaseSeed.Buyer)
            .SelectMany(purchase => purchase.Items).Select(item => item.EntityId);

        var owned = await api.GetPurchasedEntityIdsAsync(PurchaseSeed.Buyer);

        owned.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task A_user_without_purchases_gets_an_empty_owned_content_list()
    {
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var owned = await api.GetPurchasedEntityIdsAsync(WellKnownUsers.Explorer);

        owned.Should().BeEmpty();
    }
}
