using FluentAssertions;
using Payment.Domain.Products;
using Payment.Domain.Purchases;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit;

public sealed class PurchasingTests
{
    [Fact]
    public void A_price_change_after_checkout_does_not_change_the_agreed_charge()
    {
        var product = new Product(Guid.NewGuid(), "Resource", Guid.NewGuid(), 10);
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.Add(product);
        var checkout = cart.PrepareCheckout([product]);
        product.ChangePrice(product.OwnerId, 20);
        var wallet = new Wallet(cart.UserId);
        wallet.Credit(10, Guid.NewGuid(), DateTime.UtcNow);
        var purchasing = new Purchasing();

        var purchase = purchasing.Complete(cart, wallet, checkout, [], DateTime.UtcNow);

        purchase.Total.Should().Be(10);
        wallet.Balance.Should().Be(0);
        cart.Items.Should().BeEmpty();
    }

    [Fact]
    public void Owned_content_cannot_be_added_again()
    {
        var product = new Product(Guid.NewGuid(), "Resource", Guid.NewGuid(), 0);
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.Add(product);
        var checkout = cart.PrepareCheckout([product]);
        var purchasing = new Purchasing();
        var purchased = purchasing.Complete(cart, new Wallet(cart.UserId), checkout, [], DateTime.UtcNow);

        var add = () => purchasing.AddToCart(cart, product, [purchased]);

        add.Should().Throw<DomainException>();
        cart.Items.Should().BeEmpty();
    }
}
