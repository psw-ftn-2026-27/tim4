using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payment.Application.PersonalWallet;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.PersonalWallet;

public sealed class PersonalWalletQueryTests : BaseIntegrationTest
{
    public PersonalWalletQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task The_user_reads_only_their_available_credit()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var balance = await client.GetFromJsonAsync<WalletBalanceDto>("/api/payment/wallet/balance", JsonOptions);

        balance!.AvailableBalance.Should().Be(WalletSeed.ExplorerWithPayments.GetAvailableBalance());
    }

    [Fact]
    public async Task The_user_reads_only_their_transactions_with_status_and_audit_data()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var result = await client.GetFromJsonAsync<PageResult<WalletTransactionDto>>(
            "/api/payment/wallet/transactions", JsonOptions);

        result!.Items.Select(transaction => transaction.Id).Should()
            .BeEquivalentTo(WalletSeed.ExplorerWithPayments.Transactions.Select(transaction => transaction.Id));
        result.TotalCount.Should().Be(WalletSeed.ExplorerWithPayments.Transactions.Count);
        var cancelled = WalletSeed.ExplorerWithPayments.Transactions.Single(transaction => transaction.CancelledAt != null);
        result.Items.Should().Contain(transaction => transaction.Id == cancelled.Id
            && transaction.Status == TransactionStatus.Cancelled
            && transaction.CancelledAt == cancelled.CancelledAt
            && transaction.CancelledByAdministratorId == cancelled.CancelledByAdministratorId);
        var latestPayment = WalletSeed.ExplorerWithPayments.Transactions
            .OrderByDescending(transaction => transaction.SubmittedAt).First();
        result.Items.Should().Contain(transaction => transaction.Id == latestPayment.Id
            && transaction.Status == TransactionStatus.Available);
    }

    [Fact]
    public async Task Transaction_history_is_returned_in_pages()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var result = await client.GetFromJsonAsync<PageResult<WalletTransactionDto>>(
            "/api/payment/wallet/transactions?page=2&pageSize=1", JsonOptions);

        result!.Items.Select(transaction => transaction.Id).Should().Equal(WalletSeed.ExplorerWithPayments.Transactions
            .OrderByDescending(transaction => transaction.SubmittedAt).ThenBy(transaction => transaction.Id)
            .Skip(1).Take(1).Select(transaction => transaction.Id));
        result.TotalCount.Should().Be(WalletSeed.ExplorerWithPayments.Transactions.Count);
    }

    [Theory]
    [InlineData(TransactionStatus.Available)]
    [InlineData(TransactionStatus.Cancelled)]
    public async Task The_user_reads_only_their_transactions_with_the_selected_status(TransactionStatus status)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var expected = WalletSeed.ExplorerWithPayments.Transactions
            .Where(transaction => transaction.GetStatus() == status).ToList();

        var result = await client.GetFromJsonAsync<PageResult<WalletTransactionDto>>(
            $"/api/payment/wallet/transactions?status={status}", JsonOptions);

        result!.Items.Select(transaction => transaction.Id).Should()
            .BeEquivalentTo(expected.Select(transaction => transaction.Id));
        result.Items.Should().OnlyContain(transaction => transaction.Status == status);
        result.TotalCount.Should().Be(expected.Count);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    public async Task Pages_and_totals_contain_only_the_selected_transactions(int page)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");
        var expected = WalletSeed.ExplorerWithPayments.Transactions
            .Where(transaction => transaction.GetStatus() == TransactionStatus.Available)
            .OrderByDescending(transaction => transaction.SubmittedAt).ThenBy(transaction => transaction.Id)
            .ToList();

        var result = await client.GetFromJsonAsync<PageResult<WalletTransactionDto>>(
            $"/api/payment/wallet/transactions?status=Available&page={page}&pageSize=1", JsonOptions);

        result!.Items.Select(transaction => transaction.Id).Should()
            .Equal(expected.Skip(page - 1).Take(1).Select(transaction => transaction.Id));
        result.TotalCount.Should().Be(expected.Count);
    }

    [Fact]
    public async Task A_missing_wallet_has_no_credit_without_creating_a_row()
    {
        var userId = Guid.NewGuid();
        var client = Factory.CreateClientFor(userId, "explorer");

        var balance = await client.GetFromJsonAsync<WalletBalanceDto>("/api/payment/wallet/balance", JsonOptions);

        balance!.AvailableBalance.Should().Be(0);
        using var context = Factory.CreateContext<PaymentDbContext>();
        (await context.Wallets.AnyAsync(wallet => wallet.UserId == userId)).Should().BeFalse();
    }

    [Fact]
    public async Task A_missing_wallet_has_no_transactions_without_creating_a_row()
    {
        var userId = Guid.NewGuid();
        var client = Factory.CreateClientFor(userId, "explorer");

        var result = await client.GetFromJsonAsync<PageResult<WalletTransactionDto>>(
            "/api/payment/wallet/transactions", JsonOptions);

        result!.Items.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
        using var context = Factory.CreateContext<PaymentDbContext>();
        (await context.Wallets.AnyAsync(wallet => wallet.UserId == userId)).Should().BeFalse();
    }

    [Theory]
    [InlineData("balance")]
    [InlineData("transactions")]
    public async Task Wallet_data_is_not_available_without_signing_in(string resource)
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/api/payment/wallet/{resource}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
