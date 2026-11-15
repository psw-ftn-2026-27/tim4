using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payment.Application.CartCheckout;
using Payment.Application.WalletAdministration;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.CartCheckout;

public sealed class CartCheckoutCommandTests : BaseIntegrationTest
{
    public CartCheckoutCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task An_author_cannot_add_their_own_content_and_keeps_the_existing_cart()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsJsonAsync("/api/payment/cart/items", new AddToCartDto(ProductSeed.ExistingOffer.EntityId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.Include(cart => cart.Items)
            .SingleAsync(cart => cart.UserId == WellKnownUsers.SecondExplorer);
        cart.Items.Select(item => item.Id).Should().Equal(ShoppingCartSeed.TooExpensive.Items.Select(item => item.Id));
        cart.CheckoutId.Should().Be(ShoppingCartSeed.TooExpensive.CheckoutId);
        cart.Total.Should().Be(ShoppingCartSeed.TooExpensive.Total);
    }

    [Fact]
    public async Task An_explorer_adds_a_product_and_invalidates_the_previous_checkout()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync("/api/payment/cart/items", new AddToCartDto(ProductSeed.ExistingOffer.EntityId));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.Include(cart => cart.Items)
            .SingleAsync(cart => cart.UserId == WellKnownUsers.Explorer);
        cart.Items.Select(item => item.ProductId).Should().BeEquivalentTo(
            [ProductSeed.CartOffer.Id, ProductSeed.ExistingOffer.Id]);
        cart.Total.Should().Be((long)(ProductSeed.CartOffer.Price + ProductSeed.ExistingOffer.Price));
        cart.CheckoutId.Should().BeNull();
    }

    [Fact]
    public async Task Removing_a_cart_item_persists_the_empty_cart_without_a_checkout()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var itemId = ShoppingCartSeed.Ready.Items.Single().Id;

        var response = await client.DeleteAsync($"/api/payment/cart/items/{itemId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.Include(cart => cart.Items)
            .SingleAsync(cart => cart.UserId == WellKnownUsers.Explorer);
        cart.Items.Should().BeEmpty();
        cart.Total.Should().Be(0);
        cart.CheckoutId.Should().BeNull();
    }

    [Fact]
    public async Task Preparing_checkout_saves_the_prices_that_will_be_confirmed()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsync("/api/payment/cart/checkouts", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var checkoutId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.Include(cart => cart.Items)
            .SingleAsync(cart => cart.UserId == WellKnownUsers.Explorer);
        cart.CheckoutId.Should().Be(checkoutId);
        checkoutId.Should().NotBe(ShoppingCartSeed.Ready.CheckoutId!.Value);
        cart.Items.Single().Price.Should().Be((long)ProductSeed.CartOffer.Price);
    }

    [Fact]
    public async Task Buying_saves_the_purchase_per_item_payments_and_access_proofs_and_clears_the_cart()
    {
        var client = Factory.CreateClientFor(ShoppingCartSeed.MultiBuyer, "explorer");
        var beforeBalance = WalletSeed.MultiItemWallet.Balance;
        var beforeCount = PurchaseSeed.All.Length;
        var beforeTime = DateTime.UtcNow;

        var response = await client.PostAsync($"/api/payment/cart/checkouts/{ShoppingCartSeed.MultipleItems.CheckoutId}/confirm", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var purchaseId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var wallet = await context.Wallets.Include(wallet => wallet.Transactions)
            .SingleAsync(wallet => wallet.UserId == ShoppingCartSeed.MultiBuyer);
        wallet.Balance.Should().Be(beforeBalance - ShoppingCartSeed.MultipleItems.Total);
        var purchase = await context.Purchases.Include(purchase => purchase.Items).SingleAsync(purchase => purchase.Id == purchaseId);
        purchase.UserId.Should().Be(ShoppingCartSeed.MultiBuyer);
        purchase.CheckoutId.Should().Be(ShoppingCartSeed.MultipleItems.CheckoutId!.Value);
        purchase.PurchasedAt.Should().BeOnOrAfter(beforeTime).And.BeOnOrBefore(DateTime.UtcNow);
        purchase.Total.Should().Be(ShoppingCartSeed.MultipleItems.Total);
        purchase.Items.Select(item => new { item.ProductId, item.EntityId, item.Name, item.Price }).Should().BeEquivalentTo(
            ShoppingCartSeed.MultipleItems.Items.Select(item => new { item.ProductId, item.EntityId, item.Name, item.Price }));
        purchase.Items.Should().OnlyContain(item => item.Id != Guid.Empty && item.BuyerId == ShoppingCartSeed.MultiBuyer);
        var payments = wallet.Transactions.Where(transaction => transaction.Kind == TransactionKind.Purchase).ToList();
        payments.Select(transaction => new { transaction.EntityId, transaction.Amount }).Should().BeEquivalentTo(
            ShoppingCartSeed.MultipleItems.Items.Select(item => new { EntityId = (Guid?)item.EntityId, Amount = item.Price }));
        payments.Should().OnlyContain(transaction => transaction.PurchaseId == purchaseId && transaction.AdministratorId == null
            && transaction.SubmittedAt == purchase.PurchasedAt);
        var cart = await context.ShoppingCarts.Include(cart => cart.Items).SingleAsync(cart => cart.UserId == ShoppingCartSeed.MultiBuyer);
        cart.Items.Should().BeEmpty();
        cart.CheckoutId.Should().BeNull();
        (await context.Purchases.CountAsync()).Should().Be(beforeCount + 1);
    }

    [Fact]
    public async Task An_unaffordable_purchase_leaves_the_wallet_cart_and_purchase_evidence_unchanged()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.PostAsync($"/api/payment/cart/checkouts/{ShoppingCartSeed.TooExpensive.CheckoutId}/confirm", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var wallet = await context.Wallets.Include(wallet => wallet.Transactions)
            .SingleAsync(wallet => wallet.UserId == WellKnownUsers.SecondExplorer);
        wallet.Balance.Should().Be(WalletSeed.SecondExplorerWithPayment.Balance);
        wallet.Transactions.Select(transaction => transaction.Id).Should().BeEquivalentTo(
            WalletSeed.SecondExplorerWithPayment.Transactions.Select(transaction => transaction.Id));
        var cart = await context.ShoppingCarts.Include(cart => cart.Items)
            .SingleAsync(cart => cart.UserId == WellKnownUsers.SecondExplorer);
        cart.Items.Select(item => item.Id).Should().Equal(ShoppingCartSeed.TooExpensive.Items.Select(item => item.Id));
        cart.CheckoutId.Should().Be(ShoppingCartSeed.TooExpensive.CheckoutId);
        (await context.Purchases.CountAsync()).Should().Be(PurchaseSeed.All.Length);
    }

    [Fact]
    public async Task A_free_checkout_creates_a_missing_zero_wallet_and_its_payment_evidence()
    {
        var client = Factory.CreateClientFor(ShoppingCartSeed.FreeBuyer, "explorer");

        var response = await client.PostAsync($"/api/payment/cart/checkouts/{ShoppingCartSeed.Free.CheckoutId}/confirm", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var purchaseId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var wallet = await context.Wallets.Include(wallet => wallet.Transactions)
            .SingleAsync(wallet => wallet.UserId == ShoppingCartSeed.FreeBuyer);
        wallet.Balance.Should().Be(0);
        wallet.Transactions.Should().ContainSingle().Which.PurchaseId.Should().Be(purchaseId);
        wallet.Transactions.Single().Amount.Should().Be(0);
        var purchase = await context.Purchases.Include(purchase => purchase.Items).SingleAsync(purchase => purchase.Id == purchaseId);
        purchase.Items.Should().ContainSingle().Which.BuyerId.Should().Be(ShoppingCartSeed.FreeBuyer);
    }

    [Fact]
    public async Task A_completed_checkout_cannot_be_charged_again()
    {
        var client = Factory.CreateClientFor(PurchaseSeed.Buyer, "explorer");

        var response = await client.PostAsync($"/api/payment/cart/checkouts/{PurchaseSeed.Completed.CheckoutId}/confirm", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<PaymentDbContext>();
        (await context.Wallets.Where(wallet => wallet.UserId == PurchaseSeed.Buyer).Select(wallet => wallet.Balance)
            .SingleAsync()).Should().Be(PurchaseSeed.CompletedWallet.Balance);
        (await context.Purchases.CountAsync()).Should().Be(PurchaseSeed.All.Length);
    }

    [Fact]
    public async Task A_buyer_cannot_remove_another_buyers_item()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.SecondExplorer, "explorer");

        var response = await client.DeleteAsync($"/api/payment/cart/items/{ShoppingCartSeed.Ready.Items.Single().Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.Include(cart => cart.Items)
            .SingleAsync(cart => cart.UserId == WellKnownUsers.Explorer);
        cart.Items.Select(item => item.Id).Should().Equal(ShoppingCartSeed.Ready.Items.Select(item => item.Id));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Parallel_confirmations_charge_once_and_leave_no_partial_purchase(bool free)
    {
        var seededCart = free ? ShoppingCartSeed.Free : ShoppingCartSeed.Ready;
        var beforeBalance = free ? 0 : WalletSeed.ExplorerWithPayments.Balance;
        var first = Factory.CreateClientFor(seededCart.UserId, "explorer");
        var second = Factory.CreateClientFor(seededCart.UserId, "explorer");
        var path = $"/api/payment/cart/checkouts/{seededCart.CheckoutId}/confirm";

        var responses = await Task.WhenAll(first.PostAsync(path, null), second.PostAsync(path, null));

        responses.Count(response => response.StatusCode == HttpStatusCode.OK).Should().Be(1);
        responses.Count(response => response.StatusCode == HttpStatusCode.BadRequest).Should().Be(1);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var wallet = await context.Wallets.Include(wallet => wallet.Transactions)
            .SingleAsync(wallet => wallet.UserId == seededCart.UserId);
        wallet.Balance.Should().Be(beforeBalance - seededCart.Total);
        wallet.Transactions.Count(transaction => transaction.Kind == TransactionKind.Purchase).Should().Be(1);
        (await context.Purchases.CountAsync()).Should().Be(PurchaseSeed.All.Length + 1);
        var purchase = await context.Purchases.Include(purchase => purchase.Items)
            .SingleAsync(purchase => purchase.CheckoutId == seededCart.CheckoutId);
        purchase.Items.Should().ContainSingle();
        (await context.ShoppingCarts.Include(cart => cart.Items).SingleAsync(cart => cart.UserId == seededCart.UserId))
            .Items.Should().BeEmpty();
    }

    [Fact]
    public async Task Parallel_credit_and_purchase_do_not_overwrite_each_others_balance_changes()
    {
        var buyer = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var administrator = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var responses = await Task.WhenAll(
            buyer.PostAsync($"/api/payment/cart/checkouts/{ShoppingCartSeed.Ready.CheckoutId}/confirm", null),
            administrator.PostAsJsonAsync($"/api/payment/wallet/users/{WellKnownUsers.Explorer}/payments", new CreditWalletDto(50)));

        responses.Should().OnlyContain(response => response.IsSuccessStatusCode || response.StatusCode == HttpStatusCode.BadRequest);
        responses.Should().Contain(response => response.IsSuccessStatusCode);
        var charged = responses[0].IsSuccessStatusCode;
        var credited = responses[1].IsSuccessStatusCode;
        using var context = Factory.CreateContext<PaymentDbContext>();
        var wallet = await context.Wallets.Include(wallet => wallet.Transactions)
            .SingleAsync(wallet => wallet.UserId == WellKnownUsers.Explorer);
        wallet.Balance.Should().Be(WalletSeed.ExplorerWithPayments.Balance
            - (charged ? ShoppingCartSeed.Ready.Total : 0) + (credited ? 50 : 0));
        wallet.Transactions.Count.Should().Be(WalletSeed.ExplorerWithPayments.Transactions.Count
            + (charged ? 1 : 0) + (credited ? 1 : 0));
        (await context.Purchases.CountAsync()).Should().Be(PurchaseSeed.All.Length + (charged ? 1 : 0));
        var cart = await context.ShoppingCarts.Include(cart => cart.Items).SingleAsync(cart => cart.UserId == WellKnownUsers.Explorer);
        cart.Items.Count.Should().Be(charged ? 0 : ShoppingCartSeed.Ready.Items.Count);
    }

    [Theory]
    [InlineData("POST", "items", false)]
    [InlineData("DELETE", "item", false)]
    [InlineData("POST", "checkouts", false)]
    [InlineData("POST", "confirm", false)]
    [InlineData("POST", "items", true)]
    [InlineData("DELETE", "item", true)]
    [InlineData("POST", "checkouts", true)]
    [InlineData("POST", "confirm", true)]
    public async Task Only_signed_in_explorers_can_change_their_cart(string method, string operation, bool administrator)
    {
        var client = administrator ? Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator") : Factory.CreateClient();
        var suffix = operation switch
        {
            "item" => $"items/{ShoppingCartSeed.Ready.Items.Single().Id}",
            "confirm" => $"checkouts/{ShoppingCartSeed.Ready.CheckoutId}/confirm",
            _ => operation
        };
        var request = new HttpRequestMessage(new HttpMethod(method), $"/api/payment/cart/{suffix}")
        {
            Content = JsonContent.Create(new AddToCartDto(ProductSeed.ExistingOffer.EntityId))
        };

        var response = await client.SendAsync(request);

        response.StatusCode.Should().Be(administrator ? HttpStatusCode.Forbidden : HttpStatusCode.Unauthorized);
    }
}
