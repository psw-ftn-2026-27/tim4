using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.PersonalGiftCards;
using Payment.Domain.GiftCards;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.PersonalGiftCards;

public sealed class PersonalGiftCardsQueryTests : BaseIntegrationTest
{
    public PersonalGiftCardsQueryTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task A_giver_sees_the_bought_gift_cards_newest_first_with_recipient_and_status()
    {
        var client = Factory.CreateClientFor(GiftCardSeed.Giver, "explorer");
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var expected = GiftCardSeed.All.Where(card => card.GiverId == GiftCardSeed.Giver)
            .OrderByDescending(card => card.CreatedAt).ToList();

        var giftCards = await client.GetFromJsonAsync<MyGiftCardsDto>("/api/payment/gift-cards", JsonOptions);

        giftCards!.Bought.Select(card => card.Id).Should().Equal(expected.Select(card => card.Id));
        giftCards.Bought.Select(card => card.Status).Should().Equal(expected.Select(card => card.GetStatus(today)));
        giftCards.Bought.Single(card => card.Id == GiftCardSeed.ForExplorer.Id).RecipientEmail
            .Should().Be("explorer1@explorer.dev");
        giftCards.Bought.Single(card => card.Id == GiftCardSeed.ExpiredYesterday.Id).Status
            .Should().Be(GiftCardStatus.Expired);
        giftCards.Received.Should().BeEmpty();
    }

    [Fact]
    public async Task A_recipient_sees_the_received_gift_cards_with_message_validity_and_status()
    {
        var client = Factory.CreateClientFor(PurchaseSeed.GiftCardBuyer, "explorer");
        var seeded = PurchaseSeed.RedeemedGiftCard;

        var giftCards = await client.GetFromJsonAsync<MyGiftCardsDto>("/api/payment/gift-cards", JsonOptions);

        var received = giftCards!.Received.Should().ContainSingle().Subject;
        received.Id.Should().Be(seeded.Id);
        received.GiverId.Should().Be(PurchaseSeed.GiftCardGiver);
        received.Amount.Should().Be(seeded.Amount);
        received.Message.Should().Be(seeded.Message);
        received.ValidUntil.Should().Be(seeded.ValidUntil);
        received.Status.Should().Be(GiftCardStatus.Redeemed);
        giftCards.Bought.Should().BeEmpty();
    }

    [Fact]
    public async Task Gift_cards_are_visible_only_to_signed_in_explorers()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync("/api/payment/gift-cards");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
