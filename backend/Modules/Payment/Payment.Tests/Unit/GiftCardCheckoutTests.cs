using FluentAssertions;
using Payment.Domain.Coupons;
using Payment.Domain.GiftCards;
using Payment.Domain.Products;
using Payment.Domain.Purchases;
using Payment.Domain.ShoppingCarts;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit;

public sealed class GiftCardCheckoutTests
{
    private static readonly DateOnly Today = new(2026, 11, 18);
    private static readonly DateTime Now = Today.ToDateTime(new TimeOnly(12, 0), DateTimeKind.Utc);

    [Fact]
    public void A_gift_card_lowers_the_price_to_pay_by_its_amount()
    {
        var cart = PreparedCart(Guid.NewGuid(), 100, out var checkout);
        var giftCard = GiftCardFor(cart.UserId, 30);

        new Purchasing().UseGiftCard(cart, checkout, [giftCard], Today);

        cart.GiftCardDiscount.Should().Be(30);
        cart.TotalToPay.Should().Be(70);
    }

    [Fact]
    public void A_gift_card_larger_than_the_price_brings_it_to_zero_and_the_rest_is_lost()
    {
        var cart = PreparedCart(Guid.NewGuid(), 30, out var checkout);
        var giftCard = GiftCardFor(cart.UserId, 50);

        new Purchasing().UseGiftCard(cart, checkout, [giftCard], Today);

        cart.GiftCardDiscount.Should().Be(30);
        cart.TotalToPay.Should().Be(0);
    }

    [Fact]
    public void A_gift_card_applies_to_the_price_after_the_coupon_discount()
    {
        var ownerId = Guid.NewGuid();
        var product = new Product(Guid.NewGuid(), "Resource", ownerId, 100);
        var coupon = new CouponIssuing().Issue(ownerId, [product], null, 50, Today, Now);
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.Add(product);
        var checkout = cart.PrepareCheckout([product]);
        var purchasing = new Purchasing();
        purchasing.ApplyCoupon(cart, checkout, coupon, [], Today);
        var giftCard = GiftCardFor(cart.UserId, 80);

        purchasing.UseGiftCard(cart, checkout, [giftCard], Today);

        cart.GiftCardDiscount.Should().Be(50);
        cart.TotalToPay.Should().Be(0);
    }

    [Fact]
    public void A_buyer_cannot_use_a_gift_card_given_to_someone_else()
    {
        var cart = PreparedCart(Guid.NewGuid(), 100, out var checkout);
        var giftCard = GiftCardFor(Guid.NewGuid(), 30);

        var use = () => new Purchasing().UseGiftCard(cart, checkout, [giftCard], Today);

        use.Should().Throw<DomainException>();
        cart.AppliedGiftCard.Should().BeNull();
    }

    [Fact]
    public void An_expired_gift_card_cannot_be_used()
    {
        var cart = PreparedCart(Guid.NewGuid(), 100, out var checkout);
        var giftCard = GiftCardFor(cart.UserId, 30);

        var use = () => new Purchasing().UseGiftCard(cart, checkout, [giftCard], giftCard.ValidUntil.AddDays(1));

        use.Should().Throw<DomainException>();
        cart.AppliedGiftCard.Should().BeNull();
    }

    [Fact]
    public void Using_a_gift_card_again_keeps_only_one_on_the_checkout()
    {
        var cart = PreparedCart(Guid.NewGuid(), 100, out var checkout);
        var giftCard = GiftCardFor(cart.UserId, 30);
        var purchasing = new Purchasing();
        purchasing.UseGiftCard(cart, checkout, [giftCard], Today);

        purchasing.UseGiftCard(cart, checkout, [giftCard], Today);

        cart.GiftCardDiscount.Should().Be(30);
        cart.TotalToPay.Should().Be(70);
    }

    [Fact]
    public void Changing_the_cart_removes_the_gift_card()
    {
        var cart = PreparedCart(Guid.NewGuid(), 100, out var checkout);
        new Purchasing().UseGiftCard(cart, checkout, [GiftCardFor(cart.UserId, 30)], Today);

        cart.Add(new Product(Guid.NewGuid(), "Another", Guid.NewGuid(), 10));

        cart.AppliedGiftCard.Should().BeNull();
        cart.TotalToPay.Should().Be(cart.Total);
    }

    [Fact]
    public void A_confirmed_purchase_redeems_the_gift_card_and_records_its_discount()
    {
        var cart = PreparedCart(Guid.NewGuid(), 100, out var checkout);
        var giftCard = GiftCardFor(cart.UserId, 30);
        var purchasing = new Purchasing();
        purchasing.UseGiftCard(cart, checkout, [giftCard], Today);
        var wallet = FundedWallet(cart.UserId, 70);

        var purchase = purchasing.Complete(cart, wallet, checkout, null, giftCard, 70, [], Now);

        giftCard.GetStatus(Today).Should().Be(GiftCardStatus.Redeemed);
        purchase.AppliedGiftCard!.GiftCardId.Should().Be(giftCard.Id);
        purchase.GiftCardDiscount.Should().Be(30);
        purchase.Total.Should().Be(70);
        wallet.Balance.Should().Be(0);
    }

    [Fact]
    public void The_gift_card_discount_is_spread_over_the_items_so_the_wallet_pays_only_the_rest()
    {
        var first = new Product(Guid.NewGuid(), "First", Guid.NewGuid(), 25);
        var second = new Product(Guid.NewGuid(), "Second", Guid.NewGuid(), 30);
        var cart = new ShoppingCart(Guid.NewGuid());
        cart.Add(first);
        cart.Add(second);
        var checkout = cart.PrepareCheckout([first, second]);
        var giftCard = GiftCardFor(cart.UserId, 40);
        var purchasing = new Purchasing();
        purchasing.UseGiftCard(cart, checkout, [giftCard], Today);
        var wallet = FundedWallet(cart.UserId, 15);

        var purchase = purchasing.Complete(cart, wallet, checkout, null, giftCard, 15, [], Now);

        purchase.Items.Sum(item => item.Price).Should().Be(15);
        purchase.Items.Should().OnlyContain(item => item.Price >= 0);
        wallet.Transactions.Where(transaction => transaction.Kind == TransactionKind.Purchase)
            .Sum(transaction => transaction.Amount).Should().Be(15);
    }

    [Fact]
    public void A_purchase_that_costs_nothing_after_the_gift_card_succeeds_with_an_empty_wallet()
    {
        var cart = PreparedCart(Guid.NewGuid(), 30, out var checkout);
        var giftCard = GiftCardFor(cart.UserId, 50);
        var purchasing = new Purchasing();
        purchasing.UseGiftCard(cart, checkout, [giftCard], Today);
        var wallet = new Wallet(cart.UserId);

        var purchase = purchasing.Complete(cart, wallet, checkout, null, giftCard, 0, [], Now);

        purchase.Total.Should().Be(0);
        purchase.GiftCardDiscount.Should().Be(30);
        wallet.Balance.Should().Be(0);
    }

    [Fact]
    public void A_redeemed_gift_card_cannot_be_used_again()
    {
        var buyerId = Guid.NewGuid();
        var giftCard = GiftCardFor(buyerId, 50);
        var firstCart = PreparedCart(buyerId, 30, out var firstCheckout);
        var purchasing = new Purchasing();
        purchasing.UseGiftCard(firstCart, firstCheckout, [giftCard], Today);
        purchasing.Complete(firstCart, new Wallet(buyerId), firstCheckout, null, giftCard, 0, [], Now);
        var secondCart = PreparedCart(buyerId, 30, out var secondCheckout);

        var use = () => purchasing.UseGiftCard(secondCart, secondCheckout, [giftCard], Today);

        use.Should().Throw<DomainException>();
    }

    [Fact]
    public void Confirming_with_a_gift_card_that_expired_after_it_was_applied_is_rejected()
    {
        var cart = PreparedCart(Guid.NewGuid(), 100, out var checkout);
        var giftCard = GiftCardFor(cart.UserId, 30);
        var purchasing = new Purchasing();
        var lastDay = giftCard.ValidUntil;
        purchasing.UseGiftCard(cart, checkout, [giftCard], lastDay);
        var wallet = FundedWallet(cart.UserId, 100);

        var complete = () => purchasing.Complete(cart, wallet, checkout, null, giftCard, 70, [],
            lastDay.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc));

        complete.Should().Throw<DomainException>();
        giftCard.GetStatus(lastDay).Should().Be(GiftCardStatus.Valid);
    }

    private static ShoppingCart PreparedCart(Guid buyerId, int price, out Guid checkout)
    {
        var product = new Product(Guid.NewGuid(), "Resource", Guid.NewGuid(), price);
        var cart = new ShoppingCart(buyerId);
        cart.Add(product);
        checkout = cart.PrepareCheckout([product]);
        return cart;
    }

    private static GiftCard GiftCardFor(Guid recipientId, int amount)
    {
        var giverWallet = FundedWallet(Guid.NewGuid(), amount);
        return new GiftCardGiving().Give(giverWallet, recipientId, amount, "Poklon", Now);
    }

    private static Wallet FundedWallet(Guid userId, int amount)
    {
        var wallet = new Wallet(userId);
        if (amount > 0) wallet.Credit(amount, Guid.NewGuid(), Now);
        return wallet;
    }
}
