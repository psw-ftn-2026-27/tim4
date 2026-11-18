using Payment.Domain.Wallets;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class WalletSeed
{
    public static readonly Wallet ExplorerWithPayments;
    public static readonly Wallet SecondExplorerWithPayment;
    public static readonly Wallet EmptyAdministratorWallet;
    public static readonly Wallet MultiItemWallet;
    public static readonly Wallet CouponBuyerWalletWithDiscountedPrice;
    public static readonly Wallet ExpiredCouponBuyerWallet;
    public static readonly Wallet AppliedGiftCardBuyerWithPriceToPay;
    public static readonly Wallet ExpiredGiftCardBuyerWithFullPrice;
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
        MultiItemWallet = new Wallet(ShoppingCartSeed.MultiBuyer);
        MultiItemWallet.Credit(100, WellKnownUsers.Administrator, now);
        CouponBuyerWalletWithDiscountedPrice = new Wallet(ShoppingCartSeed.CouponBuyer);
        CouponBuyerWalletWithDiscountedPrice.Credit((int)ShoppingCartSeed.WithCoupon.TotalToPay,
            WellKnownUsers.Administrator, now);
        ExpiredCouponBuyerWallet = new Wallet(ShoppingCartSeed.ExpiredCouponBuyer);
        ExpiredCouponBuyerWallet.Credit((int)ShoppingCartSeed.WithCouponExpiredAfterApplying.Total,
            WellKnownUsers.Administrator, now);
        AppliedGiftCardBuyerWithPriceToPay = new Wallet(GiftCardSeed.AppliedGiftCardBuyer);
        AppliedGiftCardBuyerWithPriceToPay.Credit((int)ShoppingCartSeed.WithGiftCard.TotalToPay,
            WellKnownUsers.Administrator, now);
        ExpiredGiftCardBuyerWithFullPrice = new Wallet(GiftCardSeed.ExpiredGiftCardBuyer);
        ExpiredGiftCardBuyerWithFullPrice.Credit((int)ShoppingCartSeed.WithGiftCardExpiredAfterApplying.Total,
            WellKnownUsers.Administrator, now);
        All = [ExplorerWithPayments, SecondExplorerWithPayment, EmptyAdministratorWallet, PurchaseSeed.CompletedWallet,
            MultiItemWallet, CouponBuyerWalletWithDiscountedPrice, ExpiredCouponBuyerWallet, PurchaseSeed.CouponWallet,
            GiftCardSeed.GiverWallet, AppliedGiftCardBuyerWithPriceToPay, ExpiredGiftCardBuyerWithFullPrice,
            PurchaseSeed.GiftCardWallet, PurchaseSeed.GiftCardGiverWallet];
    }
}
