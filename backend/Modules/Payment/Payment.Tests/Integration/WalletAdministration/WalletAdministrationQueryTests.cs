using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Identity.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.WalletAdministration;
using Payment.Application.Wallets;
using Payment.Tests.Integration.Seeds;
using Shared.Domain;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.WalletAdministration;

public sealed class WalletAdministrationQueryTests : BaseIntegrationTest
{
    public WalletAdministrationQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task Every_explorer_is_listed_with_email_and_balance_even_without_a_stored_wallet()
    {
        using var scope = Factory.Services.CreateScope();
        var explorers = await scope.ServiceProvider.GetRequiredService<IIdentityApi>().GetExplorersAsync(1, 100);
        var expected = explorers.Items.Select(user => new ExplorerWalletDto(user.Id, user.Email,
            WalletSeed.All.SingleOrDefault(wallet => wallet.UserId == user.Id)?.Balance ?? 0)).ToList();
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var result = await client.GetFromJsonAsync<PageResult<ExplorerWalletDto>>(
            "/api/payment/wallet/users?pageSize=100", JsonOptions);

        result!.Items.Should().BeEquivalentTo(expected, options => options.WithStrictOrdering());
        result.TotalCount.Should().Be(explorers.TotalCount);
        var unfunded = expected.First(user => WalletSeed.All.All(wallet => wallet.UserId != user.UserId));
        result.Items.Should().Contain(user => user.UserId == unfunded.UserId && user.Balance == 0);
        result.Items.Should().NotContain(user => user.UserId == WellKnownUsers.Administrator);
    }

    [Fact]
    public async Task Explorer_pages_keep_the_total_and_email_order()
    {
        using var scope = Factory.Services.CreateScope();
        var explorers = await scope.ServiceProvider.GetRequiredService<IIdentityApi>().GetExplorersAsync(2, 1);
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var result = await client.GetFromJsonAsync<PageResult<ExplorerWalletDto>>(
            "/api/payment/wallet/users?page=2&pageSize=1", JsonOptions);

        result!.Items.Select(user => user.UserId).Should().Equal(explorers.Items.Select(user => user.Id));
        result.TotalCount.Should().Be(explorers.TotalCount);
    }

    [Fact]
    public async Task The_administrator_reads_only_the_selected_explorers_payments()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var result = await client.GetFromJsonAsync<PageResult<WalletTransactionDto>>(
            $"/api/payment/wallet/users/{WellKnownUsers.Explorer}/transactions", JsonOptions);

        result!.Items.Select(payment => payment.Id).Should().BeEquivalentTo(
            WalletSeed.ExplorerWithPayments.Transactions.Select(payment => payment.Id));
        result.TotalCount.Should().Be(WalletSeed.ExplorerWithPayments.Transactions.Count);
    }

    [Theory]
    [InlineData("users")]
    [InlineData("users/20000000-0000-0000-0000-000000000001/transactions")]
    public async Task Administrative_wallet_data_is_hidden_from_explorers(string resource)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.GetAsync($"/api/payment/wallet/{resource}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Theory]
    [InlineData("users")]
    [InlineData("users/20000000-0000-0000-0000-000000000001/transactions")]
    public async Task Administrative_wallet_data_requires_signing_in(string resource)
    {
        var client = Factory.CreateClient();

        var response = await client.GetAsync($"/api/payment/wallet/{resource}");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
