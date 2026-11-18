using Payment.Domain.GiftCards;
using Payment.Domain.Wallets;
using Shared.Tests;

namespace Payment.Tests.Integration.Seeds;

internal static class GiftCardSeed
{
    public static readonly Guid Giver = Guid.Parse("20000000-0000-0000-0000-000000000011");
    public static readonly Guid FreeGiftCardBuyer = Guid.Parse("20000000-0000-0000-0000-000000000012");
    public static readonly Guid AppliedGiftCardBuyer = Guid.Parse("20000000-0000-0000-0000-000000000013");
    public static readonly Guid ExpiredGiftCardBuyer = Guid.Parse("20000000-0000-0000-0000-000000000014");
    public static readonly Wallet GiverWallet;
    public static readonly GiftCard ForExplorer;
    public static readonly GiftCard LargerThanCartPrice;
    public static readonly GiftCard SmallerThanCartPrice;
    public static readonly GiftCard ExpiredYesterday;
    public static readonly GiftCard[] All;

    static GiftCardSeed()
    {
        var now = DateTimeOffset.FromUnixTimeSeconds(DateTimeOffset.UtcNow.ToUnixTimeSeconds()).UtcDateTime;
        var giving = new GiftCardGiving();
        GiverWallet = new Wallet(Giver);
        GiverWallet.Credit(1000, WellKnownUsers.Administrator, now.AddYears(-4));
        ExpiredYesterday = giving.Give(GiverWallet, ExpiredGiftCardBuyer, 100, "Za neku turu.",
            now.AddYears(-3).AddDays(-1));
        ForExplorer = giving.Give(GiverWallet, WellKnownUsers.Explorer, 50, "Srećan rođendan!", now.AddMinutes(-3));
        LargerThanCartPrice = giving.Give(GiverWallet, FreeGiftCardBuyer, 50, "Uživaj u turi.", now.AddMinutes(-2));
        SmallerThanCartPrice = giving.Give(GiverWallet, AppliedGiftCardBuyer, 20, "Mali poklon.", now.AddMinutes(-1));
        All = [ExpiredYesterday, ForExplorer, LargerThanCartPrice, SmallerThanCartPrice, PurchaseSeed.RedeemedGiftCard];
    }
}
