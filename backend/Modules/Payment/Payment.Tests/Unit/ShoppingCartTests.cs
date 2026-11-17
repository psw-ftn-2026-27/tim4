using FluentAssertions;
using Payment.Domain.Products;
using Payment.Domain.ShoppingCarts;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit;

public sealed class ShoppingCartTests
{
    [Fact]
    public void An_author_cannot_add_their_own_product()
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        var product = new Product(Guid.NewGuid(), "Resource", cart.UserId, 10);

        var add = () => cart.Add(product);

        add.Should().Throw<DomainException>();
        cart.Items.Should().BeEmpty();
        cart.Total.Should().Be(0);
    }

    [Fact]
    public void Preparing_checkout_refreshes_a_changed_product_price()
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        var product = Product(10);
        cart.Add(product);
        product.ChangePrice(product.OwnerId, 20);

        cart.PrepareCheckout([product]);

        cart.Items.Should().ContainSingle().Which.Price.Should().Be(20);
        cart.Total.Should().Be(20);
    }

    [Fact]
    public void The_cart_total_tracks_added_and_removed_items()
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        var first = Product(30);
        cart.Add(first);
        cart.Add(Product(20));

        cart.Remove(cart.Items.Single(item => item.ProductId == first.Id).Id);

        cart.Total.Should().Be(20);
        cart.Items.Should().ContainSingle();
    }

    [Fact]
    public void Adding_a_duplicate_keeps_the_cart_and_total_unchanged()
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        var product = Product(30);
        cart.Add(product);

        var add = () => cart.Add(product);

        add.Should().Throw<DomainException>();
        cart.Items.Should().ContainSingle();
        cart.Total.Should().Be(30);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Changing_items_invalidates_the_prepared_checkout(bool addItem)
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        var product = Product(30);
        cart.Add(product);
        var checkout = cart.PrepareCheckout([product]);

        if (addItem) cart.Add(Product(20));
        else cart.Remove(cart.Items.Single().Id);

        cart.CheckoutId.Should().BeNull();
        var confirm = () => cart.EnsureCheckout(checkout);
        confirm.Should().Throw<DomainException>();
    }

    [Fact]
    public void An_empty_cart_cannot_be_prepared_for_payment()
    {
        var cart = new ShoppingCart(Guid.NewGuid());

        var prepare = () => cart.PrepareCheckout([]);

        prepare.Should().Throw<DomainException>();
        cart.CheckoutId.Should().BeNull();
    }

    [Fact]
    public void A_total_beyond_the_supported_balance_is_rejected_without_adding_the_item()
    {
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.Add(Product(long.MaxValue));

        var add = () => cart.Add(Product(1));

        add.Should().Throw<DomainException>();
        cart.Total.Should().Be(long.MaxValue);
        cart.Items.Should().ContainSingle();
    }

    private static Product Product(long price) => new(Guid.NewGuid(), "Resource", Guid.NewGuid(), price);
}
