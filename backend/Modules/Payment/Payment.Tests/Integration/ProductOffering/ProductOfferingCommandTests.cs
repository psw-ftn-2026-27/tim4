using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
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

    [Fact]
    public async Task Content_is_offered_as_a_product_with_its_owner_and_price()
    {
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        var request = new CreateProductRequest(Guid.NewGuid(), "New digital resource", WellKnownUsers.Explorer, 12.75m);
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
