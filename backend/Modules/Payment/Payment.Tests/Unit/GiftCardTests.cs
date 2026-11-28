using FluentAssertions;
using Payment.Domain.GiftCards;
using Payment.Domain.Wallets;
using Xunit;

namespace Payment.Tests.Unit;

public sealed class GiftCardTests
{
    [Fact]
    public void A_gift_card_is_valid_through_the_same_date_three_years_later_and_expires_the_next_day()
    {
        var giftCard = GiftCardCreatedOn(new DateTime(2026, 11, 18, 23, 30, 0, DateTimeKind.Utc));

        var lastDay = giftCard.GetStatus(new DateOnly(2029, 11, 18));
        var nextDay = giftCard.GetStatus(new DateOnly(2029, 11, 19));

        lastDay.Should().Be(GiftCardStatus.Valid);
        nextDay.Should().Be(GiftCardStatus.Expired);
    }

    [Fact]
    public void A_gift_card_created_on_a_leap_day_is_valid_through_the_last_day_of_february()
    {
        var giftCard = GiftCardCreatedOn(new DateTime(2028, 2, 29, 10, 0, 0, DateTimeKind.Utc));

        var validUntil = giftCard.ValidUntil;

        validUntil.Should().Be(new DateOnly(2031, 2, 28));
    }

    [Fact]
    public void Among_usable_gift_cards_the_one_that_expires_first_is_chosen()
    {
        var recipientId = Guid.NewGuid();
        var later = GiftCardCreatedOn(new DateTime(2026, 5, 1, 0, 0, 0, DateTimeKind.Utc), recipientId);
        var earlier = GiftCardCreatedOn(new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc), recipientId);
        var expired = GiftCardCreatedOn(new DateTime(2022, 1, 1, 0, 0, 0, DateTimeKind.Utc), recipientId);
        var someoneElses = GiftCardCreatedOn(new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        var chosen = GiftCard.FirstToExpire([later, expired, someoneElses, earlier], recipientId,
            new DateOnly(2026, 11, 18));

        chosen.Should().BeSameAs(earlier);
    }

    private static GiftCard GiftCardCreatedOn(DateTime now, Guid? recipientId = null)
    {
        var wallet = new Wallet(Guid.NewGuid());
        wallet.Credit(100, Guid.NewGuid(), now);
        return new GiftCardGiving().Give(wallet, recipientId ?? Guid.NewGuid(), 50, "Poklon", now);
    }
}
