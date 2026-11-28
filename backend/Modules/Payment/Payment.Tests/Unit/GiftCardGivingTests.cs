using FluentAssertions;
using Payment.Domain.GiftCards;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit;

public sealed class GiftCardGivingTests
{
    private static readonly DateTime Now = new(2026, 11, 18, 12, 0, 0, DateTimeKind.Utc);

    [Theory]
    [InlineData(10)]
    [InlineData(1000)]
    public void A_gift_card_takes_exactly_its_amount_from_the_givers_wallet(int amount)
    {
        var wallet = FundedWallet(1000);

        var giftCard = new GiftCardGiving().Give(wallet, Guid.NewGuid(), amount, "Poklon", Now);

        giftCard.Amount.Should().Be(amount);
        wallet.Balance.Should().Be(1000 - amount);
        var payment = wallet.Transactions.Single(transaction => transaction.Kind == TransactionKind.GiftCard);
        payment.Amount.Should().Be(amount);
        payment.GiftCardId.Should().Be(giftCard.Id);
    }

    [Theory]
    [InlineData(9)]
    [InlineData(1001)]
    [InlineData(-10)]
    public void An_amount_outside_the_limits_is_rejected_and_the_wallet_is_unchanged(int amount)
    {
        var wallet = FundedWallet(2000);

        var give = () => new GiftCardGiving().Give(wallet, Guid.NewGuid(), amount, "Poklon", Now);

        give.Should().Throw<DomainException>();
        wallet.Balance.Should().Be(2000);
        wallet.Transactions.Should().ContainSingle();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void An_empty_message_is_rejected(string? message)
    {
        var wallet = FundedWallet(100);

        var give = () => new GiftCardGiving().Give(wallet, Guid.NewGuid(), 10, message!, Now);

        give.Should().Throw<DomainException>();
        wallet.Balance.Should().Be(100);
    }

    [Fact]
    public void A_message_of_two_hundred_characters_is_accepted()
    {
        var wallet = FundedWallet(100);

        var giftCard = new GiftCardGiving().Give(wallet, Guid.NewGuid(), 10, new string('a', 200), Now);

        giftCard.Message.Should().HaveLength(200);
    }

    [Fact]
    public void A_message_longer_than_two_hundred_characters_is_rejected()
    {
        var wallet = FundedWallet(100);

        var give = () => new GiftCardGiving().Give(wallet, Guid.NewGuid(), 10, new string('a', 201), Now);

        give.Should().Throw<DomainException>();
        wallet.Balance.Should().Be(100);
    }

    [Fact]
    public void A_giver_cannot_give_a_gift_card_to_themselves()
    {
        var wallet = FundedWallet(100);

        var give = () => new GiftCardGiving().Give(wallet, wallet.UserId, 10, "Poklon", Now);

        give.Should().Throw<DomainException>();
        wallet.Balance.Should().Be(100);
    }

    [Fact]
    public void A_giver_without_enough_credit_gets_no_gift_card_and_keeps_the_balance()
    {
        var wallet = FundedWallet(49);

        var give = () => new GiftCardGiving().Give(wallet, Guid.NewGuid(), 50, "Poklon", Now);

        give.Should().Throw<DomainException>();
        wallet.Balance.Should().Be(49);
        wallet.Transactions.Should().ContainSingle();
    }

    private static Wallet FundedWallet(int amount)
    {
        var wallet = new Wallet(Guid.NewGuid());
        wallet.Credit(amount, Guid.NewGuid(), Now);
        return wallet;
    }
}
