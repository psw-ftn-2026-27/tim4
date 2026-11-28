using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.CartCheckout;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.CartCheckout;

public sealed class CartCheckoutQueryTests : BaseIntegrationTest
{
    public CartCheckoutQueryTests(PaymentApiFactory factory) : base(factory) { }

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
    public async Task A_checkout_with_a_valid_coupon_shows_the_discount_and_the_price_to_pay()
    {
        var client = Factory.CreateClientFor(ShoppingCartSeed.CouponBuyer, "explorer");
        var seededCart = ShoppingCartSeed.WithCoupon;

        var checkout = await client.GetFromJsonAsync<CartCheckoutDto>(
            $"/api/payment/cart/checkouts/{seededCart.CheckoutId}", JsonOptions);

        checkout!.AppliedCoupon!.Code.Should().Be(CouponSeed.ExplorerForExpensiveOffer.Code.Value);
        checkout.AppliedCoupon.ProductId.Should().Be(seededCart.AppliedCoupon!.ProductId);
        checkout.AppliedCoupon.DiscountPercent.Should().Be(CouponSeed.ExplorerForExpensiveOffer.DiscountPercent);
        checkout.AppliedCoupon.Discount.Should().Be(seededCart.AppliedCoupon.Discount);
        checkout.AppliedCoupon.IsValid.Should().BeTrue();
        checkout.AppliedCoupon.InvalidReason.Should().BeNull();
        checkout.Total.Should().Be(seededCart.Total);
        checkout.TotalToPay.Should().Be(seededCart.TotalToPay);
    }

    [Fact]
    public async Task A_checkout_with_a_coupon_that_expired_shows_the_reason_and_the_price_without_discount()
    {
        var client = Factory.CreateClientFor(ShoppingCartSeed.ExpiredCouponBuyer, "explorer");
        var seededCart = ShoppingCartSeed.WithCouponExpiredAfterApplying;

        var checkout = await client.GetFromJsonAsync<CartCheckoutDto>(
            $"/api/payment/cart/checkouts/{seededCart.CheckoutId}", JsonOptions);

        checkout!.AppliedCoupon!.Code.Should().Be(CouponSeed.ExplorerExpiredYesterday.Code.Value);
        checkout.AppliedCoupon.IsValid.Should().BeFalse();
        checkout.AppliedCoupon.InvalidReason.Should().NotBeNullOrWhiteSpace();
        checkout.TotalToPay.Should().Be(seededCart.Total);
    }

    [Fact]
    public async Task A_receipt_shows_the_applied_coupon_and_its_discount()
    {
        var client = Factory.CreateClientFor(PurchaseSeed.CouponBuyer, "explorer");
        var seeded = PurchaseSeed.CompletedWithCoupon;

        var purchase = await client.GetFromJsonAsync<CompletedPurchaseDto>(
            $"/api/payment/cart/purchases/{seeded.Id}", JsonOptions);

        purchase!.AppliedCoupon!.Code.Should().Be(CouponSeed.SecondExplorerForAllProducts.Code.Value);
        purchase.AppliedCoupon.ProductId.Should().Be(seeded.AppliedCoupon!.ProductId);
        purchase.AppliedCoupon.DiscountPercent.Should().Be(seeded.AppliedCoupon.DiscountPercent);
        purchase.AppliedCoupon.Discount.Should().Be(seeded.AppliedCoupon.Discount);
        purchase.Items.Select(item => item.Price).Should().Equal(seeded.Items.Select(item => item.Price));
        purchase.Total.Should().Be(seeded.Total);
    }

    [Fact]
    public async Task A_checkout_offers_the_buyers_usable_gift_card_with_its_amount()
    {
        var client = Factory.CreateClientFor(GiftCardSeed.FreeGiftCardBuyer, "explorer");
        var seededCart = ShoppingCartSeed.WithUsableGiftCard;

        var checkout = await client.GetFromJsonAsync<CartCheckoutDto>(
            $"/api/payment/cart/checkouts/{seededCart.CheckoutId}", JsonOptions);

        checkout!.AvailableGiftCard!.GiftCardId.Should().Be(GiftCardSeed.LargerThanCartPrice.Id);
        checkout.AvailableGiftCard.Amount.Should().Be(GiftCardSeed.LargerThanCartPrice.Amount);
        checkout.AvailableGiftCard.ValidUntil.Should().Be(GiftCardSeed.LargerThanCartPrice.ValidUntil);
        checkout.AppliedGiftCard.Should().BeNull();
        checkout.TotalToPay.Should().Be(seededCart.Total);
    }

    [Fact]
    public async Task A_checkout_with_a_gift_card_shows_its_discount_and_the_price_to_pay()
    {
        var client = Factory.CreateClientFor(GiftCardSeed.AppliedGiftCardBuyer, "explorer");
        var seededCart = ShoppingCartSeed.WithGiftCard;

        var checkout = await client.GetFromJsonAsync<CartCheckoutDto>(
            $"/api/payment/cart/checkouts/{seededCart.CheckoutId}", JsonOptions);

        checkout!.AppliedGiftCard!.GiftCardId.Should().Be(GiftCardSeed.SmallerThanCartPrice.Id);
        checkout.AppliedGiftCard.Amount.Should().Be(GiftCardSeed.SmallerThanCartPrice.Amount);
        checkout.AppliedGiftCard.Discount.Should().Be(seededCart.GiftCardDiscount);
        checkout.AppliedGiftCard.IsValid.Should().BeTrue();
        checkout.TotalToPay.Should().Be(seededCart.TotalToPay);
    }

    [Fact]
    public async Task A_checkout_with_a_gift_card_that_expired_shows_the_reason_and_the_price_without_it()
    {
        var client = Factory.CreateClientFor(GiftCardSeed.ExpiredGiftCardBuyer, "explorer");
        var seededCart = ShoppingCartSeed.WithGiftCardExpiredAfterApplying;

        var checkout = await client.GetFromJsonAsync<CartCheckoutDto>(
            $"/api/payment/cart/checkouts/{seededCart.CheckoutId}", JsonOptions);

        checkout!.AppliedGiftCard!.IsValid.Should().BeFalse();
        checkout.AppliedGiftCard.InvalidReason.Should().NotBeNullOrWhiteSpace();
        checkout.AppliedGiftCard.Discount.Should().Be(0);
        checkout.TotalToPay.Should().Be(seededCart.Total);
    }

    [Fact]
    public async Task A_receipt_shows_the_redeemed_gift_card_and_its_discount()
    {
        var client = Factory.CreateClientFor(PurchaseSeed.GiftCardBuyer, "explorer");
        var seeded = PurchaseSeed.CompletedWithGiftCard;

        var purchase = await client.GetFromJsonAsync<CompletedPurchaseDto>(
            $"/api/payment/cart/purchases/{seeded.Id}", JsonOptions);

        purchase!.AppliedGiftCard!.GiftCardId.Should().Be(PurchaseSeed.RedeemedGiftCard.Id);
        purchase.AppliedGiftCard.Amount.Should().Be(PurchaseSeed.RedeemedGiftCard.Amount);
        purchase.AppliedGiftCard.Discount.Should().Be(seeded.GiftCardDiscount);
        purchase.Items.Select(item => new { item.Id, item.Price }).Should()
            .BeEquivalentTo(seeded.Items.Select(item => new { item.Id, item.Price }));
        purchase.Total.Should().Be(seeded.Total);
    }

    [Fact]
    public async Task A_receipt_is_not_visible_to_another_buyer()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync($"/api/payment/cart/purchases/{PurchaseSeed.Completed.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

}
