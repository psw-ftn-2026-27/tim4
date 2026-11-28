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
    public async Task A_missing_cart_cannot_be_prepared_and_is_not_created()
    {
        var userId = Guid.NewGuid();
        var client = Factory.CreateClientFor(userId, "explorer");

        var response = await client.PostAsync("/api/payment/cart/checkouts", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var context = Factory.CreateContext<PaymentDbContext>();
        (await context.ShoppingCarts.AnyAsync(cart => cart.UserId == userId)).Should().BeFalse();
        (await context.Purchases.AnyAsync(purchase => purchase.UserId == userId)).Should().BeFalse();
    }

    [Fact]
    public async Task Missing_content_cannot_be_added_and_leaves_the_prepared_cart_unchanged()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync("/api/payment/cart/items", new AddToCartDto(Guid.NewGuid()));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.Include(cart => cart.Items)
            .SingleAsync(cart => cart.UserId == WellKnownUsers.Explorer);
        cart.Items.Select(item => new { item.Id, item.ProductId, item.EntityId, item.Name, item.Price }).Should()
            .BeEquivalentTo(ShoppingCartSeed.Ready.Items.Select(item => new { item.Id, item.ProductId, item.EntityId, item.Name, item.Price }));
        cart.Total.Should().Be(ShoppingCartSeed.Ready.Total);
        cart.CheckoutId.Should().Be(ShoppingCartSeed.Ready.CheckoutId);
        cart.Version.Should().Be(ShoppingCartSeed.Ready.Version);
    }

    [Fact]
    public async Task Purchased_content_cannot_be_added_and_leaves_the_buyers_cart_unchanged()
    {
        var client = Factory.CreateClientFor(PurchaseSeed.Buyer, "explorer");

        var response = await client.PostAsJsonAsync("/api/payment/cart/items", new AddToCartDto(ProductSeed.ExistingOffer.EntityId));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.Include(cart => cart.Items)
            .SingleAsync(cart => cart.UserId == PurchaseSeed.Buyer);
        cart.Items.Should().BeEmpty();
        cart.Total.Should().Be(PurchaseSeed.CompletedCart.Total);
        cart.CheckoutId.Should().Be(PurchaseSeed.CompletedCart.CheckoutId);
        cart.Version.Should().Be(PurchaseSeed.CompletedCart.Version);
        (await context.Purchases.CountAsync()).Should().Be(PurchaseSeed.All.Length);
    }

    [Fact]
    public async Task An_empty_cart_cannot_be_prepared_and_remains_without_a_checkout()
    {
        var client = Factory.CreateClientFor(PurchaseSeed.Buyer, "explorer");

        var response = await client.PostAsync("/api/payment/cart/checkouts", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.Include(cart => cart.Items)
            .SingleAsync(cart => cart.UserId == PurchaseSeed.Buyer);
        cart.Items.Should().BeEmpty();
        cart.Total.Should().Be(PurchaseSeed.CompletedCart.Total);
        cart.CheckoutId.Should().BeNull();
        cart.Version.Should().Be(PurchaseSeed.CompletedCart.Version);
        (await context.Purchases.CountAsync()).Should().Be(PurchaseSeed.All.Length);
    }

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

        var response = await client.PostAsJsonAsync($"/api/payment/cart/checkouts/{ShoppingCartSeed.MultipleItems.CheckoutId}/confirm",
            new ConfirmCheckoutDto(ShoppingCartSeed.MultipleItems.TotalToPay));

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

        var response = await client.PostAsJsonAsync($"/api/payment/cart/checkouts/{ShoppingCartSeed.TooExpensive.CheckoutId}/confirm",
            new ConfirmCheckoutDto(ShoppingCartSeed.TooExpensive.TotalToPay));

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

        var response = await client.PostAsJsonAsync($"/api/payment/cart/checkouts/{ShoppingCartSeed.Free.CheckoutId}/confirm",
            new ConfirmCheckoutDto(ShoppingCartSeed.Free.TotalToPay));

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

        var response = await client.PostAsJsonAsync($"/api/payment/cart/checkouts/{PurchaseSeed.Completed.CheckoutId}/confirm",
            new ConfirmCheckoutDto(PurchaseSeed.Completed.Total));

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

        var responses = await Task.WhenAll(first.PostAsJsonAsync(path, new ConfirmCheckoutDto(seededCart.TotalToPay)),
            second.PostAsJsonAsync(path, new ConfirmCheckoutDto(seededCart.TotalToPay)));

        responses.Count(response => response.StatusCode == HttpStatusCode.OK).Should().Be(1);
        responses.Should().ContainSingle(response => response.StatusCode == HttpStatusCode.BadRequest
            || response.StatusCode == HttpStatusCode.Conflict);
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
            buyer.PostAsJsonAsync($"/api/payment/cart/checkouts/{ShoppingCartSeed.Ready.CheckoutId}/confirm",
                new ConfirmCheckoutDto(ShoppingCartSeed.Ready.TotalToPay)),
            administrator.PostAsJsonAsync($"/api/payment/wallet/users/{WellKnownUsers.Explorer}/payments", new CreditWalletDto(50)));

        responses.Should().OnlyContain(response => response.IsSuccessStatusCode
            || response.StatusCode == HttpStatusCode.BadRequest || response.StatusCode == HttpStatusCode.Conflict);
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

    [Fact]
    public async Task Applying_a_valid_code_typed_in_lowercase_saves_the_coupon_and_discount_on_the_checkout()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var coupon = CouponSeed.SecondExplorerForAllProducts;

        var response = await client.PutAsJsonAsync($"/api/payment/cart/checkouts/{ShoppingCartSeed.Ready.CheckoutId}/coupon",
            new ApplyCouponDto(coupon.Code.Value.ToLowerInvariant()));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.Include(cart => cart.Items)
            .SingleAsync(cart => cart.UserId == WellKnownUsers.Explorer);
        cart.CheckoutId.Should().Be(ShoppingCartSeed.Ready.CheckoutId);
        cart.AppliedCoupon!.CouponId.Should().Be(coupon.Id);
        cart.AppliedCoupon.Code.Should().Be(coupon.Code.Value);
        cart.AppliedCoupon.ProductId.Should().Be(ProductSeed.CartOffer.Id);
        cart.AppliedCoupon.DiscountPercent.Should().Be(coupon.DiscountPercent);
        cart.AppliedCoupon.Discount.Should().Be(ProductSeed.CartOffer.Price * coupon.DiscountPercent / 100);
    }

    [Fact]
    public async Task A_missing_code_is_rejected_and_the_checkout_stays_the_same()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PutAsJsonAsync($"/api/payment/cart/checkouts/{ShoppingCartSeed.Ready.CheckoutId}/coupon",
            new ApplyCouponDto("missing"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.SingleAsync(cart => cart.UserId == WellKnownUsers.Explorer);
        cart.AppliedCoupon.Should().BeNull();
        cart.CheckoutId.Should().Be(ShoppingCartSeed.Ready.CheckoutId);
        cart.Version.Should().Be(ShoppingCartSeed.Ready.Version);
    }

    [Fact]
    public async Task A_buyer_cannot_apply_again_a_coupon_from_a_confirmed_purchase_and_keeps_the_previous_coupon()
    {
        var client = Factory.CreateClientFor(PurchaseSeed.CouponBuyer, "explorer");

        var response = await client.PutAsJsonAsync($"/api/payment/cart/checkouts/{PurchaseSeed.CouponCart.CheckoutId}/coupon",
            new ApplyCouponDto(CouponSeed.SecondExplorerForAllProducts.Code.Value));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.SingleAsync(cart => cart.UserId == PurchaseSeed.CouponBuyer);
        cart.AppliedCoupon.Should().Be(PurchaseSeed.CouponCart.AppliedCoupon);
        cart.CheckoutId.Should().Be(PurchaseSeed.CouponCart.CheckoutId);
        cart.Version.Should().Be(PurchaseSeed.CouponCart.Version);
    }

    [Fact]
    public async Task Removing_the_coupon_saves_the_checkout_without_a_coupon()
    {
        var client = Factory.CreateClientFor(ShoppingCartSeed.CouponBuyer, "explorer");

        var response = await client.DeleteAsync($"/api/payment/cart/checkouts/{ShoppingCartSeed.WithCoupon.CheckoutId}/coupon");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.Include(cart => cart.Items)
            .SingleAsync(cart => cart.UserId == ShoppingCartSeed.CouponBuyer);
        cart.AppliedCoupon.Should().BeNull();
        cart.CheckoutId.Should().Be(ShoppingCartSeed.WithCoupon.CheckoutId);
        cart.Items.Select(item => item.Id).Should().Equal(ShoppingCartSeed.WithCoupon.Items.Select(item => item.Id));
    }

    [Fact]
    public async Task Buying_with_a_coupon_charges_the_discounted_price_that_the_wallet_exactly_covers()
    {
        var client = Factory.CreateClientFor(ShoppingCartSeed.CouponBuyer, "explorer");
        var seededCart = ShoppingCartSeed.WithCoupon;
        var applied = seededCart.AppliedCoupon!;
        var beforeCount = PurchaseSeed.All.Length;

        var response = await client.PostAsJsonAsync($"/api/payment/cart/checkouts/{seededCart.CheckoutId}/confirm",
            new ConfirmCheckoutDto(seededCart.TotalToPay));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var purchaseId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var purchase = await context.Purchases.Include(purchase => purchase.Items).SingleAsync(purchase => purchase.Id == purchaseId);
        purchase.AppliedCoupon.Should().Be(applied);
        purchase.Items.Single(item => item.ProductId == applied.ProductId).Price
            .Should().Be(ProductSeed.ExpensiveOffer.Price - applied.Discount);
        purchase.Total.Should().Be(seededCart.TotalToPay);
        var wallet = await context.Wallets.Include(wallet => wallet.Transactions)
            .SingleAsync(wallet => wallet.UserId == ShoppingCartSeed.CouponBuyer);
        wallet.Balance.Should().Be(WalletSeed.CouponBuyerWalletWithDiscountedPrice.Balance - seededCart.TotalToPay);
        wallet.Transactions.Where(transaction => transaction.Kind == TransactionKind.Purchase)
            .Sum(transaction => transaction.Amount).Should().Be(seededCart.TotalToPay);
        var cart = await context.ShoppingCarts.Include(cart => cart.Items)
            .SingleAsync(cart => cart.UserId == ShoppingCartSeed.CouponBuyer);
        cart.Items.Should().BeEmpty();
        cart.CheckoutId.Should().BeNull();
        cart.AppliedCoupon.Should().BeNull();
        (await context.Purchases.CountAsync()).Should().Be(beforeCount + 1);
    }

    [Fact]
    public async Task A_coupon_that_expired_after_it_was_applied_blocks_the_purchase_and_changes_nothing()
    {
        var client = Factory.CreateClientFor(ShoppingCartSeed.ExpiredCouponBuyer, "explorer");
        var seededCart = ShoppingCartSeed.WithCouponExpiredAfterApplying;

        var response = await client.PostAsJsonAsync($"/api/payment/cart/checkouts/{seededCart.CheckoutId}/confirm",
            new ConfirmCheckoutDto(seededCart.TotalToPay));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var wallet = await context.Wallets.Include(wallet => wallet.Transactions)
            .SingleAsync(wallet => wallet.UserId == ShoppingCartSeed.ExpiredCouponBuyer);
        wallet.Balance.Should().Be(WalletSeed.ExpiredCouponBuyerWallet.Balance);
        wallet.Transactions.Select(transaction => transaction.Id).Should().BeEquivalentTo(
            WalletSeed.ExpiredCouponBuyerWallet.Transactions.Select(transaction => transaction.Id));
        var cart = await context.ShoppingCarts.Include(cart => cart.Items)
            .SingleAsync(cart => cart.UserId == ShoppingCartSeed.ExpiredCouponBuyer);
        cart.Items.Select(item => item.Id).Should().Equal(seededCart.Items.Select(item => item.Id));
        cart.CheckoutId.Should().Be(seededCart.CheckoutId);
        cart.AppliedCoupon.Should().Be(seededCart.AppliedCoupon);
        (await context.Purchases.CountAsync()).Should().Be(PurchaseSeed.All.Length);
    }

    [Fact]
    public async Task Using_a_gift_card_saves_the_buyers_card_on_the_checkout()
    {
        var client = Factory.CreateClientFor(GiftCardSeed.FreeGiftCardBuyer, "explorer");
        var seededCart = ShoppingCartSeed.WithUsableGiftCard;

        var response = await client.PutAsync($"/api/payment/cart/checkouts/{seededCart.CheckoutId}/gift-card", null);

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.Include(cart => cart.Items)
            .SingleAsync(cart => cart.UserId == GiftCardSeed.FreeGiftCardBuyer);
        cart.AppliedGiftCard!.GiftCardId.Should().Be(GiftCardSeed.LargerThanCartPrice.Id);
        cart.CheckoutId.Should().Be(seededCart.CheckoutId);
        cart.TotalToPay.Should().Be(0);
    }

    [Fact]
    public async Task A_buyer_without_a_usable_gift_card_cannot_use_one_and_the_checkout_stays_the_same()
    {
        var client = Factory.CreateClientFor(GiftCardSeed.ExpiredGiftCardBuyer, "explorer");
        var seededCart = ShoppingCartSeed.WithGiftCardExpiredAfterApplying;

        var response = await client.PutAsync($"/api/payment/cart/checkouts/{seededCart.CheckoutId}/gift-card", null);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.SingleAsync(cart => cart.UserId == GiftCardSeed.ExpiredGiftCardBuyer);
        cart.AppliedGiftCard.Should().Be(seededCart.AppliedGiftCard);
        cart.Version.Should().Be(seededCart.Version);
    }

    [Fact]
    public async Task Removing_the_gift_card_saves_the_checkout_without_it()
    {
        var client = Factory.CreateClientFor(GiftCardSeed.AppliedGiftCardBuyer, "explorer");
        var seededCart = ShoppingCartSeed.WithGiftCard;

        var response = await client.DeleteAsync($"/api/payment/cart/checkouts/{seededCart.CheckoutId}/gift-card");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var cart = await context.ShoppingCarts.SingleAsync(cart => cart.UserId == GiftCardSeed.AppliedGiftCardBuyer);
        cart.AppliedGiftCard.Should().BeNull();
        cart.CheckoutId.Should().Be(seededCart.CheckoutId);
    }

    [Fact]
    public async Task Buying_with_a_gift_card_redeems_it_records_its_discount_and_charges_only_the_rest()
    {
        var client = Factory.CreateClientFor(GiftCardSeed.AppliedGiftCardBuyer, "explorer");
        var seededCart = ShoppingCartSeed.WithGiftCard;
        var giftCard = GiftCardSeed.SmallerThanCartPrice;

        var response = await client.PostAsJsonAsync($"/api/payment/cart/checkouts/{seededCart.CheckoutId}/confirm",
            new ConfirmCheckoutDto(seededCart.TotalToPay));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var purchaseId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var purchase = await context.Purchases.Include(purchase => purchase.Items)
            .SingleAsync(purchase => purchase.Id == purchaseId);
        purchase.AppliedGiftCard!.GiftCardId.Should().Be(giftCard.Id);
        purchase.GiftCardDiscount.Should().Be(giftCard.Amount);
        purchase.Total.Should().Be(seededCart.Total - giftCard.Amount);
        var stored = await context.GiftCards.SingleAsync(card => card.Id == giftCard.Id);
        stored.RedeemedInPurchaseId.Should().Be(purchaseId);
        var wallet = await context.Wallets.SingleAsync(wallet => wallet.UserId == GiftCardSeed.AppliedGiftCardBuyer);
        wallet.Balance.Should().Be(WalletSeed.AppliedGiftCardBuyerWithPriceToPay.Balance - seededCart.TotalToPay);
        var cart = await context.ShoppingCarts.Include(cart => cart.Items)
            .SingleAsync(cart => cart.UserId == GiftCardSeed.AppliedGiftCardBuyer);
        cart.Items.Should().BeEmpty();
        cart.AppliedGiftCard.Should().BeNull();
    }

    [Fact]
    public async Task A_gift_card_that_expired_after_it_was_applied_blocks_the_purchase_and_changes_nothing()
    {
        var client = Factory.CreateClientFor(GiftCardSeed.ExpiredGiftCardBuyer, "explorer");
        var seededCart = ShoppingCartSeed.WithGiftCardExpiredAfterApplying;

        var response = await client.PostAsJsonAsync($"/api/payment/cart/checkouts/{seededCart.CheckoutId}/confirm",
            new ConfirmCheckoutDto(seededCart.TotalToPay));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var wallet = await context.Wallets.Include(wallet => wallet.Transactions)
            .SingleAsync(wallet => wallet.UserId == GiftCardSeed.ExpiredGiftCardBuyer);
        wallet.Balance.Should().Be(WalletSeed.ExpiredGiftCardBuyerWithFullPrice.Balance);
        wallet.Transactions.Should().HaveCount(WalletSeed.ExpiredGiftCardBuyerWithFullPrice.Transactions.Count);
        var cart = await context.ShoppingCarts.Include(cart => cart.Items)
            .SingleAsync(cart => cart.UserId == GiftCardSeed.ExpiredGiftCardBuyer);
        cart.Items.Select(item => item.Id).Should().Equal(seededCart.Items.Select(item => item.Id));
        cart.AppliedGiftCard.Should().Be(seededCart.AppliedGiftCard);
        var giftCard = await context.GiftCards.SingleAsync(card => card.Id == GiftCardSeed.ExpiredYesterday.Id);
        giftCard.RedeemedInPurchaseId.Should().BeNull();
        (await context.Purchases.CountAsync()).Should().Be(PurchaseSeed.All.Length);
    }

    [Theory]
    [InlineData("POST", "items", false)]
    [InlineData("DELETE", "item", false)]
    [InlineData("POST", "checkouts", false)]
    [InlineData("POST", "confirm", false)]
    [InlineData("PUT", "coupon", false)]
    [InlineData("DELETE", "coupon", false)]
    [InlineData("PUT", "gift-card", false)]
    [InlineData("DELETE", "gift-card", false)]
    [InlineData("POST", "items", true)]
    [InlineData("DELETE", "item", true)]
    [InlineData("POST", "checkouts", true)]
    [InlineData("POST", "confirm", true)]
    [InlineData("PUT", "coupon", true)]
    [InlineData("DELETE", "coupon", true)]
    [InlineData("PUT", "gift-card", true)]
    [InlineData("DELETE", "gift-card", true)]
    public async Task Only_signed_in_explorers_can_change_their_cart(string method, string operation, bool administrator)
    {
        var client = administrator ? Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator") : Factory.CreateClient();
        var suffix = operation switch
        {
            "item" => $"items/{ShoppingCartSeed.Ready.Items.Single().Id}",
            "confirm" => $"checkouts/{ShoppingCartSeed.Ready.CheckoutId}/confirm",
            "coupon" => $"checkouts/{ShoppingCartSeed.Ready.CheckoutId}/coupon",
            "gift-card" => $"checkouts/{ShoppingCartSeed.Ready.CheckoutId}/gift-card",
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
