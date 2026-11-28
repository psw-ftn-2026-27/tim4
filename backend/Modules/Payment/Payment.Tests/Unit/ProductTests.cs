using FluentAssertions;
using Payment.Domain.Products;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit;

public sealed class ProductTests
{
    [Theory]
    [InlineData(0L)]
    [InlineData(30L)]
    [InlineData(long.MaxValue)]
    public void The_owner_can_change_the_price_within_the_supported_balance_range(long price)
    {
        var product = new Product(Guid.NewGuid(), "Resource", Guid.NewGuid(), 10);

        product.ChangePrice(product.OwnerId, price);

        product.Price.Should().Be(price);
    }

    [Fact]
    public void Another_user_cannot_change_the_price()
    {
        var product = new Product(Guid.NewGuid(), "Resource", Guid.NewGuid(), 10);

        var change = () => product.ChangePrice(Guid.NewGuid(), 20);

        change.Should().Throw<DomainException>();
        product.Price.Should().Be(10);
    }

    [Fact]
    public void A_price_change_cannot_make_the_price_negative()
    {
        var product = new Product(Guid.NewGuid(), "Resource", Guid.NewGuid(), 10);

        var change = () => product.ChangePrice(product.OwnerId, -1);

        change.Should().Throw<DomainException>();
        product.Price.Should().Be(10);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void A_product_requires_a_name(string? name)
    {
        var creation = () => new Product(Guid.NewGuid(), name!, Guid.NewGuid(), 10);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void A_product_requires_content_and_owner_identifiers(bool emptyContent)
    {
        var entityId = emptyContent ? Guid.Empty : Guid.NewGuid();
        var ownerId = emptyContent ? Guid.NewGuid() : Guid.Empty;

        var creation = () => new Product(entityId, "A digital resource", ownerId, 10);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void A_product_can_be_free()
    {
        var product = new Product(Guid.NewGuid(), "A digital resource", Guid.NewGuid(), 0);

        product.Price.Should().Be(0);
    }

    [Fact]
    public void A_product_cannot_have_a_negative_price()
    {
        var creation = () => new Product(Guid.NewGuid(), "A digital resource", Guid.NewGuid(), -1);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("entity")]
    [InlineData("name")]
    [InlineData("owner")]
    [InlineData("price")]
    public void Existing_product_details_cannot_be_silently_replaced(string difference)
    {
        var original = new Product(Guid.NewGuid(), "A digital resource", Guid.NewGuid(), 10);
        var other = new Product(
            difference == "entity" ? Guid.NewGuid() : original.EntityId,
            difference == "name" ? "A different resource" : original.Name,
            difference == "owner" ? Guid.NewGuid() : original.OwnerId,
            difference == "price" ? 20 : original.Price);

        var comparison = () => original.EnsureSameOffer(other);

        comparison.Should().Throw<DomainException>();
    }
}
