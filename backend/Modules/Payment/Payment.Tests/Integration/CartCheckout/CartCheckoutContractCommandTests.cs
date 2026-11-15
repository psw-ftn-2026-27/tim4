using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Payment.Contracts;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.CartCheckout;

public sealed class CartCheckoutContractCommandTests : BaseIntegrationTest
{
    public CartCheckoutContractCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Another_module_can_add_content_to_the_buyers_cart_through_the_payment_contract()
    {
        using var scope = Factory.Services.CreateScope();
        var api = scope.ServiceProvider.GetRequiredService<IPaymentApi>();
        var product = ProductSeed.ExistingOffer;

        await api.AddToCartAsync(WellKnownUsers.Explorer, product.EntityId);

        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.Include(cart => cart.Items)
            .SingleAsync(cart => cart.UserId == WellKnownUsers.Explorer);
        cart.Items.Select(item => item.ProductId).Should().BeEquivalentTo(
            [ProductSeed.CartOffer.Id, product.Id]);
        cart.Items.Should().ContainSingle(item => item.EntityId == product.EntityId
            && item.Name == product.Name && item.Price == (long)product.Price);
        cart.Total.Should().Be(ShoppingCartSeed.Ready.Total + (long)product.Price);
        cart.CheckoutId.Should().BeNull();
    }
}
