using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Payment.Application.PersonalGiftCards;
using Payment.Domain.GiftCards;
using Payment.Tests.Integration.Seeds;
using Shared.Domain;
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

        var bought = await client.GetFromJsonAsync<PageResult<GiftCardDto>>(
            "/api/payment/gift-cards/bought", JsonOptions);

        bought!.TotalCount.Should().Be(expected.Count);
        bought.Items.Select(card => card.Id).Should().Equal(expected.Select(card => card.Id));
        bought.Items.Select(card => card.Status).Should().Equal(expected.Select(card => card.GetStatus(today)));
        bought.Items.Single(card => card.Id == GiftCardSeed.ForExplorer.Id).RecipientEmail
            .Should().Be("explorer1@explorer.dev");
        bought.Items.Single(card => card.Id == GiftCardSeed.ExpiredYesterday.Id).Status
            .Should().Be(GiftCardStatus.Expired);
    }

    [Fact]
    public async Task Bought_gift_cards_are_paged_and_the_count_covers_all_of_them()
    {
        var client = Factory.CreateClientFor(GiftCardSeed.Giver, "explorer");
        var expected = GiftCardSeed.All.Where(card => card.GiverId == GiftCardSeed.Giver)
            .OrderByDescending(card => card.CreatedAt).ToList();

        var secondPage = await client.GetFromJsonAsync<PageResult<GiftCardDto>>(
            "/api/payment/gift-cards/bought?page=2&pageSize=2", JsonOptions);

        secondPage!.TotalCount.Should().Be(expected.Count);
        secondPage.Items.Select(card => card.Id).Should().Equal(expected.Skip(2).Take(2).Select(card => card.Id));
    }

    [Fact]
    public async Task A_recipient_sees_the_received_gift_cards_with_message_validity_and_status()
    {
        var client = Factory.CreateClientFor(PurchaseSeed.GiftCardBuyer, "explorer");
        var seeded = PurchaseSeed.RedeemedGiftCard;

        var received = await client.GetFromJsonAsync<PageResult<GiftCardDto>>(
            "/api/payment/gift-cards/received", JsonOptions);

        received!.TotalCount.Should().Be(GiftCardSeed.All.Count(card => card.RecipientId == PurchaseSeed.GiftCardBuyer));
        var card = received.Items.Should().ContainSingle().Subject;
        card.Id.Should().Be(seeded.Id);
        card.GiverId.Should().Be(PurchaseSeed.GiftCardGiver);
        card.Amount.Should().Be(seeded.Amount);
        card.Message.Should().Be(seeded.Message);
        card.ValidUntil.Should().Be(seeded.ValidUntil);
        card.Status.Should().Be(GiftCardStatus.Redeemed);
    }

    [Theory]
    [InlineData("bought")]
    [InlineData("received")]
    public async Task Gift_cards_are_visible_only_to_signed_in_explorers(string list)
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator");

        var response = await client.GetAsync($"/api/payment/gift-cards/{list}");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
