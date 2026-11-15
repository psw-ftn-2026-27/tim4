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
