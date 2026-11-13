using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Identity.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Payment.Application.WalletAdministration;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.WalletAdministration;

public sealed class WalletAdministrationCommandTests : BaseIntegrationTest
{
    public WalletAdministrationCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task An_administrator_credits_an_existing_wallet_and_the_payment_is_available_immediately()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        var beforeBalance = WalletSeed.ExplorerWithPayments.Balance;
        var beforeCount = WalletSeed.ExplorerWithPayments.Transactions.Count;
        var beforeTime = DateTime.UtcNow;

        var response = await client.PostAsJsonAsync(
            $"/api/payment/wallet/users/{WellKnownUsers.Explorer}/payments", new CreditWalletDto(50));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var wallet = await context.Wallets.Include(wallet => wallet.Transactions)
            .SingleAsync(wallet => wallet.UserId == WellKnownUsers.Explorer);
        wallet.Balance.Should().Be(beforeBalance + 50);
        wallet.Transactions.Should().HaveCount(beforeCount + 1);
        var payment = wallet.Transactions.Single(transaction =>
            !WalletSeed.ExplorerWithPayments.Transactions.Select(seed => seed.Id).Contains(transaction.Id));
        payment.Amount.Should().Be(50);
        payment.AdministratorId.Should().Be(WellKnownUsers.Administrator);
        payment.SubmittedAt.Should().BeOnOrAfter(beforeTime).And.BeOnOrBefore(DateTime.UtcNow);
        payment.CancelledAt.Should().BeNull();
    }

    [Fact]
    public async Task The_first_payment_creates_a_wallet_for_an_existing_explorer()
    {
        using var scope = Factory.Services.CreateScope();
        var explorers = await scope.ServiceProvider.GetRequiredService<IIdentityApi>().GetExplorersAsync(1, 100);
        var userId = explorers.Items.First(user => WalletSeed.All.All(wallet => wallet.UserId != user.Id)).Id;
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        int beforeCount;
        using (var context = Factory.CreateContext<PaymentDbContext>())
        {
            beforeCount = await context.Wallets.CountAsync();
        }

        var response = await client.PostAsJsonAsync(
            $"/api/payment/wallet/users/{userId}/payments", new CreditWalletDto(75));

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        using var stored = Factory.CreateContext<PaymentDbContext>();
        (await stored.Wallets.CountAsync()).Should().Be(beforeCount + 1);
        var wallet = await stored.Wallets.Include(wallet => wallet.Transactions)
            .SingleAsync(wallet => wallet.UserId == userId);
        wallet.Balance.Should().Be(75);
        wallet.Transactions.Should().ContainSingle().Which.Amount.Should().Be(75);
    }

    [Fact]
    public async Task An_invalid_first_payment_does_not_create_a_wallet()
    {
        using var scope = Factory.Services.CreateScope();
        var explorers = await scope.ServiceProvider.GetRequiredService<IIdentityApi>().GetExplorersAsync(1, 100);
        var userId = explorers.Items.First(user => WalletSeed.All.All(wallet => wallet.UserId != user.Id)).Id;
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        int beforeCount;
        using (var context = Factory.CreateContext<PaymentDbContext>())
        {
            beforeCount = await context.Wallets.CountAsync();
        }

        var response = await client.PostAsJsonAsync(
            $"/api/payment/wallet/users/{userId}/payments", new CreditWalletDto(0));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var stored = Factory.CreateContext<PaymentDbContext>();
        (await stored.Wallets.CountAsync()).Should().Be(beforeCount);
        (await stored.Wallets.AnyAsync(wallet => wallet.UserId == userId)).Should().BeFalse();
    }

    [Theory]
    [InlineData("00000000-0000-0000-0000-000000000099")]
    [InlineData("10000000-0000-0000-0000-000000000001")]
    public async Task Only_an_existing_explorer_can_receive_credit(string recipient)
    {
        var userId = Guid.Parse(recipient);
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");
        long? beforeBalance;
        int beforeCount;
        using (var context = Factory.CreateContext<PaymentDbContext>())
        {
            beforeCount = await context.Wallets.CountAsync();
            beforeBalance = await context.Wallets.Where(wallet => wallet.UserId == userId)
                .Select(wallet => (long?)wallet.Balance).SingleOrDefaultAsync();
        }

        var response = await client.PostAsJsonAsync(
            $"/api/payment/wallet/users/{userId}/payments", new CreditWalletDto(50));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var stored = Factory.CreateContext<PaymentDbContext>();
        (await stored.Wallets.CountAsync()).Should().Be(beforeCount);
        (await stored.Wallets.Where(wallet => wallet.UserId == userId)
            .Select(wallet => (long?)wallet.Balance).SingleOrDefaultAsync()).Should().Be(beforeBalance);
    }

    [Fact]
    public async Task An_explorer_cannot_credit_a_wallet()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync(
            $"/api/payment/wallet/users/{WellKnownUsers.Explorer}/payments", new CreditWalletDto(50));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Crediting_a_wallet_requires_signing_in()
    {
        var client = Factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            $"/api/payment/wallet/users/{WellKnownUsers.Explorer}/payments", new CreditWalletDto(50));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
