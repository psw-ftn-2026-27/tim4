using Payment.Domain.Wallets;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class WalletSeed
{
    public static readonly Wallet ExplorerWithPayments;
    public static readonly Wallet SecondExplorerWithPayment;
    public static readonly Wallet EmptyAdministratorWallet;
    public static readonly Wallet[] All;

    static WalletSeed()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds()).UtcDateTime;
        ExplorerWithPayments = new Wallet(WellKnownUsers.Explorer);
        ExplorerWithPayments.Credit(100, WellKnownUsers.Administrator, now.AddDays(-2));
        ExplorerWithPayments.Credit(30, WellKnownUsers.Administrator, now);
        ExplorerWithPayments.Credit(20, WellKnownUsers.Administrator, now.AddDays(-3));
        ExplorerWithPayments.ReversePayment(ExplorerWithPayments.Transactions[2].Id,
            WellKnownUsers.Administrator, ExplorerWithPayments.Transactions[2].SubmittedAt.AddHours(1));
        ExplorerWithPayments.Credit(40, WellKnownUsers.Administrator, now.AddDays(-4));
        SecondExplorerWithPayment = new Wallet(WellKnownUsers.SecondExplorer);
        SecondExplorerWithPayment.Credit(500, WellKnownUsers.Administrator, now.AddDays(-2));
        SecondExplorerWithPayment.Credit(60, WellKnownUsers.Administrator, now);
        SecondExplorerWithPayment.Credit(70, WellKnownUsers.Administrator, now.AddDays(-3));
        SecondExplorerWithPayment.ReversePayment(SecondExplorerWithPayment.Transactions[2].Id,
            WellKnownUsers.Administrator, SecondExplorerWithPayment.Transactions[2].SubmittedAt.AddHours(1));
        EmptyAdministratorWallet = new Wallet(WellKnownUsers.Administrator);
        All = [ExplorerWithPayments, SecondExplorerWithPayment, EmptyAdministratorWallet];
    }
}
