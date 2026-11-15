using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Payment.Contracts;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Domain.Exceptions;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.ProductOffering;

public sealed class ProductOfferingCommandTests : BaseIntegrationTest
{
    public ProductOfferingCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Theory]
    [InlineData(0L)]
    [InlineData(40L)]
    public async Task An_owner_saves_a_new_price_without_changing_the_product_identity(long price)
    {
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        var original = ProductSeed.ExistingOffer;

        await api.UpdateProductPriceAsync(original.EntityId, original.OwnerId, price);

        using var context = Factory.CreateContext<PaymentDbContext>();
        var product = context.Products.Single(product => product.EntityId == original.EntityId);
        product.Id.Should().Be(original.Id);
        product.OwnerId.Should().Be(original.OwnerId);
        product.Name.Should().Be(original.Name);
        product.Price.Should().Be(price);
        var preparedCart = context.ShoppingCarts.Include(cart => cart.Items)
            .Single(cart => cart.Id == ShoppingCartSeed.MultipleItems.Id);
        preparedCart.Items.Single(item => item.ProductId == original.Id).Price.Should().Be(original.Price);
        var completedPurchase = context.Purchases.Include(purchase => purchase.Items)
            .Single(purchase => purchase.Id == PurchaseSeed.Completed.Id);
        completedPurchase.Items.Single(item => item.ProductId == original.Id).Price.Should().Be(original.Price);
    }

    [Fact]
    public async Task Another_user_cannot_replace_the_saved_price()
    {
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        var original = ProductSeed.ExistingOffer;

        var change = () => api.UpdateProductPriceAsync(original.EntityId, WellKnownUsers.Explorer, 40);

        await change.Should().ThrowAsync<DomainException>();
        using var context = Factory.CreateContext<PaymentDbContext>();
        context.Products.Single(product => product.EntityId == original.EntityId).Price.Should().Be(original.Price);
    }

    [Fact]
    public async Task A_negative_price_leaves_the_saved_price_unchanged()
    {
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        var original = ProductSeed.ExistingOffer;

        var change = () => api.UpdateProductPriceAsync(original.EntityId, original.OwnerId, -1);

        await change.Should().ThrowAsync<DomainException>();
        using var context = Factory.CreateContext<PaymentDbContext>();
        context.Products.Single(product => product.EntityId == original.EntityId).Price.Should().Be(original.Price);
    }

    [Fact]
    public async Task A_price_change_for_missing_content_does_not_create_a_product()
    {
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        var missing = Guid.NewGuid();

        var change = () => api.UpdateProductPriceAsync(missing, WellKnownUsers.Explorer, 40);

        await change.Should().ThrowAsync<NotFoundException>();
        using var context = Factory.CreateContext<PaymentDbContext>();
        context.Products.Any(product => product.EntityId == missing).Should().BeFalse();
    }

    [Fact]
    public async Task Content_is_offered_as_a_product_with_its_owner_and_price()
    {
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        var request = new CreateProductRequest(Guid.NewGuid(), "New digital resource", WellKnownUsers.Explorer, 12);
        int previousCount;
        using (var arrangeContext = Factory.CreateContext<PaymentDbContext>())
        {
            previousCount = arrangeContext.Products.Count();
        }

        await api.CreateProductAsync(request);

        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Products.Count().Should().Be(previousCount + 1);
        var product = assertContext.Products.Single(product => product.EntityId == request.EntityId);
        product.Id.Should().NotBeEmpty();
        product.Name.Should().Be(request.Name);
        product.OwnerId.Should().Be(request.OwnerId);
        product.Price.Should().Be(request.Price);
    }

    [Fact]
    public async Task Repeating_the_same_offer_keeps_the_original_product()
    {
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        var original = ProductSeed.ExistingOffer;
        var request = new CreateProductRequest(original.EntityId, original.Name, original.OwnerId, original.Price);
        int previousCount;
        using (var arrangeContext = Factory.CreateContext<PaymentDbContext>())
        {
            previousCount = arrangeContext.Products.Count();
        }

        await api.CreateProductAsync(request);

        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Products.Count().Should().Be(previousCount);
        var product = assertContext.Products.Single(product => product.EntityId == request.EntityId);
        product.Id.Should().Be(original.Id);
        product.Name.Should().Be(original.Name);
        product.OwnerId.Should().Be(original.OwnerId);
        product.Price.Should().Be(original.Price);
    }

    [Fact]
    public async Task Conflicting_offer_is_rejected_without_replacing_the_product()
    {
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        var original = ProductSeed.ExistingOffer;
        var request = new CreateProductRequest(original.EntityId, original.Name, original.OwnerId, original.Price + 1);
        int previousCount;
        using (var arrangeContext = Factory.CreateContext<PaymentDbContext>())
        {
            previousCount = arrangeContext.Products.Count();
        }

        var creation = () => api.CreateProductAsync(request);

        await creation.Should().ThrowAsync<DomainException>();
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Products.Count().Should().Be(previousCount);
        var product = assertContext.Products.Single(product => product.EntityId == request.EntityId);
        product.Id.Should().Be(original.Id);
        product.Price.Should().Be(original.Price);
    }

    [Fact]
    public async Task An_invalid_offer_does_not_leave_a_product_in_storage()
    {
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        var request = new CreateProductRequest(Guid.NewGuid(), "New resource", WellKnownUsers.Explorer, -1);
        int previousCount;
        using (var arrangeContext = Factory.CreateContext<PaymentDbContext>())
        {
            previousCount = arrangeContext.Products.Count();
        }

        var creation = () => api.CreateProductAsync(request);

        await creation.Should().ThrowAsync<DomainException>();
        using var assertContext = Factory.CreateContext<PaymentDbContext>();
        assertContext.Products.Count().Should().Be(previousCount);
        assertContext.Products.Any(product => product.EntityId == request.EntityId).Should().BeFalse();
    }
}
