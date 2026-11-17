using FluentAssertions;
using Payment.Domain.Wallets;
using Shared.Domain.Exceptions;
using Xunit;

namespace Payment.Tests.Unit;

public sealed class WalletPurchaseTests
{
    [Fact]
    public void Insufficient_credit_rejects_the_whole_purchase_without_payment_records()
    {
        var wallet = new Wallet(Guid.NewGuid());
        wallet.Credit(20, Guid.NewGuid(), DateTime.UtcNow);

        var pay = () => wallet.Pay(Guid.NewGuid(), [new PurchasePayment(Guid.NewGuid(), 30)], DateTime.UtcNow);

        pay.Should().Throw<DomainException>();
        wallet.Balance.Should().Be(20);
        wallet.Transactions.Should().ContainSingle();
    }

    [Fact]
    public void A_purchase_records_its_items_without_an_administrator()
    {
        var wallet = new Wallet(Guid.NewGuid());
        wallet.Credit(100, Guid.NewGuid(), DateTime.UtcNow);
        var purchaseId = Guid.NewGuid();
        var content = Guid.NewGuid();

        wallet.Pay(purchaseId, [new PurchasePayment(content, 30)], DateTime.UtcNow);

        wallet.Balance.Should().Be(70);
        var payment = wallet.Transactions.Single(transaction => transaction.Kind == TransactionKind.Purchase);
        payment.AdministratorId.Should().BeNull();
        payment.PurchaseId.Should().Be(purchaseId);
        payment.EntityId.Should().Be(content);
        payment.Amount.Should().Be(30);
    }

    [Fact]
    public void Purchase_payments_cannot_be_reversed_as_administrator_credit()
    {
        var wallet = new Wallet(Guid.NewGuid());
        var now = DateTime.UtcNow;
        wallet.Credit(100, Guid.NewGuid(), now);
        wallet.Pay(Guid.NewGuid(), [new PurchasePayment(Guid.NewGuid(), 30)], now);
        var payment = wallet.Transactions.Single(transaction => transaction.Kind == TransactionKind.Purchase);

        var reverse = () => wallet.ReversePayment(payment.Id, Guid.NewGuid(), now.AddMinutes(1));

        reverse.Should().Throw<DomainException>();
        wallet.Balance.Should().Be(70);
        payment.CancelledAt.Should().BeNull();
    }
}
