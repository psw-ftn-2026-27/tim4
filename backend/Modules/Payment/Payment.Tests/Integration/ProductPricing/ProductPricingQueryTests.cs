using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Payment.Contracts;
using Payment.Tests.Integration.Seeds;
using Xunit;

namespace Payment.Tests.Integration.ProductPricing;

public sealed class ProductPricingQueryTests : BaseIntegrationTest
{
    public ProductPricingQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Prices_distinguish_paid_free_and_unpriced_content_in_the_same_batch()
    {
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        var paid = ProductSeed.ExistingOffer;
        var free = ProductSeed.FreeOffer;
        var unpriced = Guid.NewGuid();
        var expected = new Dictionary<Guid, long?>
        {
            [paid.EntityId] = paid.Price,
            [free.EntityId] = 0,
            [unpriced] = null
        };

        var prices = await api.GetPricesByEntityIdsAsync([paid.EntityId, free.EntityId, unpriced, paid.EntityId]);

        prices.Should().BeEquivalentTo(expected);
    }

    [Fact]
    public async Task An_empty_selection_has_no_prices()
    {
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IPaymentApi>();

        var prices = await api.GetPricesByEntityIdsAsync([]);

        prices.Should().BeEmpty();
    }

    [Fact]
    public async Task Content_without_products_has_explicit_missing_prices()
    {
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        var first = Guid.NewGuid();
        var second = Guid.NewGuid();
        var expected = new Dictionary<Guid, long?> { [first] = null, [second] = null };

        var prices = await api.GetPricesByEntityIdsAsync([first, second]);

        prices.Should().BeEquivalentTo(expected);
    }
}
