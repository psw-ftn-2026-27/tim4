using FluentAssertions;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit;

public sealed class WalletTests
{
    private static readonly DateTime SubmittedAt = new(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
    private static readonly Guid AdministratorId = Guid.NewGuid();

    [Fact]
    public void A_new_wallet_has_no_credit_or_transactions()
    {
        var wallet = new Wallet(Guid.NewGuid());

        wallet.Balance.Should().Be(0);
        wallet.GetAvailableBalance().Should().Be(0);
        wallet.Transactions.Should().BeEmpty();
    }

    [Fact]
    public void A_payment_increases_available_credit_immediately()
    {
        var wallet = new Wallet(Guid.NewGuid());

        wallet.Credit(50, AdministratorId, SubmittedAt);

        wallet.Balance.Should().Be(50);
        wallet.GetAvailableBalance().Should().Be(50);
        wallet.Transactions.Single().GetStatus().Should().Be(TransactionStatus.Available);
    }

    [Fact]
    public void Reading_available_credit_again_does_not_increase_it()
    {
        var wallet = WalletWithPayment();
        var previousBalance = wallet.GetAvailableBalance();

        var balance = wallet.GetAvailableBalance();

        balance.Should().Be(previousBalance);
        wallet.Transactions.Should().ContainSingle();
    }

    [Fact]
    public void Reversing_a_payment_reduces_credit_and_preserves_the_payment_and_audit()
    {
        var wallet = WalletWithPayment();
        var payment = wallet.Transactions.Single();
        var cancellingAdministrator = Guid.NewGuid();
        var cancellationTime = SubmittedAt.AddHours(1);

        wallet.ReversePayment(payment.Id, cancellingAdministrator, cancellationTime);

        wallet.GetAvailableBalance().Should().Be(0);
        wallet.Transactions.Should().ContainSingle().Which.Should().Be(payment);
        payment.GetStatus().Should().Be(TransactionStatus.Cancelled);
        payment.CancelledByAdministratorId.Should().Be(cancellingAdministrator);
        payment.CancelledAt.Should().Be(cancellationTime);
        payment.Amount.Should().Be(50);
        payment.AdministratorId.Should().Be(AdministratorId);
        payment.SubmittedAt.Should().Be(SubmittedAt);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void A_payment_requires_a_positive_amount(int amount)
    {
        var wallet = new Wallet(Guid.NewGuid());

        var act = () => wallet.Credit(amount, AdministratorId, SubmittedAt);

        act.Should().Throw<DomainException>();
        wallet.Balance.Should().Be(0);
        wallet.Transactions.Should().BeEmpty();
    }

    [Theory]
    [InlineData(24)]
    [InlineData(25)]
    [InlineData(-1)]
    public void Payments_cannot_be_reversed_outside_the_allowed_time(int hours)
    {
        var wallet = WalletWithPayment();
        var payment = wallet.Transactions.Single();

        var act = () => wallet.ReversePayment(payment.Id, AdministratorId, SubmittedAt.AddHours(hours));

        act.Should().Throw<DomainException>();
        wallet.Balance.Should().Be(50);
        payment.CancelledAt.Should().BeNull();
        payment.CancelledByAdministratorId.Should().BeNull();
    }

    [Fact]
    public void Credit_can_exceed_the_range_of_a_single_payment_amount()
    {
        var wallet = WalletWithPayment();

        wallet.Credit(int.MaxValue, AdministratorId, SubmittedAt);

        wallet.Balance.Should().Be(50L + int.MaxValue);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(86399)]
    public void A_payment_can_be_reversed_before_the_deadline(int secondsAfterPayment)
    {
        var wallet = WalletWithPayment();
        var payment = wallet.Transactions.Single();

        wallet.ReversePayment(payment.Id, AdministratorId, SubmittedAt.AddSeconds(secondsAfterPayment));

        wallet.Balance.Should().Be(0);
        payment.GetStatus().Should().Be(TransactionStatus.Cancelled);
    }

    [Fact]
    public void A_payment_cannot_be_reversed_twice()
    {
        var wallet = WalletWithPayment();
        var payment = wallet.Transactions.Single();
        wallet.Credit(50, AdministratorId, SubmittedAt);
        var cancelledAt = SubmittedAt.AddHours(1);
        wallet.ReversePayment(payment.Id, AdministratorId, cancelledAt);

        var act = () => wallet.ReversePayment(payment.Id, Guid.NewGuid(), SubmittedAt.AddHours(2));

        act.Should().Throw<DomainException>();
        wallet.Balance.Should().Be(50);
        payment.CancelledAt.Should().Be(cancelledAt);
        payment.CancelledByAdministratorId.Should().Be(AdministratorId);
    }

    [Fact]
    public void A_payment_from_another_wallet_cannot_be_reversed()
    {
        var wallet = WalletWithPayment();
        var otherWallet = WalletWithPayment();
        var otherPayment = otherWallet.Transactions.Single();

        var act = () => wallet.ReversePayment(otherPayment.Id, AdministratorId, SubmittedAt.AddHours(1));

        act.Should().Throw<NotFoundException>();
        wallet.Balance.Should().Be(50);
        otherPayment.CancelledAt.Should().BeNull();
        wallet.Transactions.Single().CancelledAt.Should().BeNull();
    }

    private static Wallet WalletWithPayment()
    {
        var wallet = new Wallet(Guid.NewGuid());
        wallet.Credit(50, AdministratorId, SubmittedAt);
        return wallet;
    }
}
