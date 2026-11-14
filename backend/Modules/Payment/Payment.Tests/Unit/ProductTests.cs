using FluentAssertions;
using Payment.Domain.Products;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit;

public sealed class ProductTests
{
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

    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(1, true)]
    public void A_product_can_be_free_but_cannot_have_a_negative_price(int price, bool allowed)
    {
        var creation = () => new Product(Guid.NewGuid(), "A digital resource", Guid.NewGuid(), price);

        if (allowed)
        {
            creation.Should().NotThrow();
        }
        else
        {
            creation.Should().Throw<DomainException>();
        }
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
