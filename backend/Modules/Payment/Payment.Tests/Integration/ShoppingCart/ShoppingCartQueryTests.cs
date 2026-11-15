using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.ShoppingCart;
using Payment.Application.Wallets;
using Payment.Domain.Wallets;
using Payment.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.ShoppingCart;

public sealed class ShoppingCartQueryTests : BaseIntegrationTest
{
    public ShoppingCartQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Cart_display_contains_only_the_buyers_saved_items_and_total()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var cart = await client.GetFromJsonAsync<CartDto>("/api/payment/cart", JsonOptions);

        cart!.Items.Select(item => item.ProductId).Should().Equal(ShoppingCartSeed.Ready.Items.Select(item => item.ProductId));
        cart.Total.Should().Be(ShoppingCartSeed.Ready.Total);
        cart.Id.Should().Be(ShoppingCartSeed.Ready.Id);
    }

    [Fact]
    public async Task Prepared_checkout_can_be_reloaded_with_its_saved_prices_and_current_balance()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var checkout = await client.GetFromJsonAsync<CartCheckoutDto>(
            $"/api/payment/cart/checkouts/{ShoppingCartSeed.Ready.CheckoutId}", JsonOptions);

        checkout!.Id.Should().Be(ShoppingCartSeed.Ready.CheckoutId!.Value);
        checkout.Total.Should().Be(ShoppingCartSeed.Ready.Total);
        checkout.Items.Select(item => item.Price).Should().Equal(ShoppingCartSeed.Ready.Items.Select(item => item.Price));
        checkout.AvailableBalance.Should().Be(WalletSeed.ExplorerWithPayments.Balance);
    }

    [Fact]
    public async Task A_receipt_can_be_reloaded_with_its_purchase_proofs_prices_and_time()
    {
        var client = Factory.CreateClientFor(PurchaseSeed.Buyer, "explorer");

        var purchase = await client.GetFromJsonAsync<CompletedPurchaseDto>(
            $"/api/payment/cart/purchases/{PurchaseSeed.Completed.Id}", JsonOptions);

        purchase!.Id.Should().Be(PurchaseSeed.Completed.Id);
        purchase.PurchasedAt.Should().Be(PurchaseSeed.Completed.PurchasedAt);
        purchase.Total.Should().Be(PurchaseSeed.Completed.Total);
        purchase.Items.Select(item => item.Id).Should().Equal(PurchaseSeed.Completed.Items.Select(item => item.Id));
        purchase.Items.Select(item => item.EntityId).Should().Equal(PurchaseSeed.Completed.Items.Select(item => item.EntityId));
    }

    [Fact]
    public async Task A_receipt_is_not_visible_to_another_buyer()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync($"/api/payment/cart/purchases/{PurchaseSeed.Completed.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Wallet_history_distinguishes_purchase_payments_from_administrator_credit()
    {
        var client = Factory.CreateClientFor(PurchaseSeed.Buyer, "explorer");

        var history = await client.GetFromJsonAsync<PageResult<WalletTransactionDto>>("/api/payment/wallet/transactions", JsonOptions);

        history!.Items.Should().ContainSingle(item => item.Kind == TransactionKind.Purchase
            && item.PurchaseId == PurchaseSeed.Completed.Id && item.EntityId == ProductSeed.ExistingOffer.EntityId
            && item.AdministratorId == null && item.Amount == (long)ProductSeed.ExistingOffer.Price);
        history.Items.Should().ContainSingle(item => item.Kind == TransactionKind.Credit
            && item.AdministratorId == WellKnownUsers.Administrator && item.PurchaseId == null);
    }
}
