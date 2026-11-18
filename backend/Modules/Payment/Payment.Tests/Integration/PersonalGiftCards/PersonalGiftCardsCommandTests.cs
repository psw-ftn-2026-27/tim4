using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Payment.Application.PersonalGiftCards;
using Payment.Domain.Wallets;
using Payment.Infrastructure.Persistence;
using Payment.Tests.Integration.Seeds;
using Shared.Tests;
using Xunit;

namespace Payment.Tests.Integration.PersonalGiftCards;

public sealed class PersonalGiftCardsCommandTests : BaseIntegrationTest
{
    public PersonalGiftCardsCommandTests(PaymentApiFactory factory) : base(factory) { }

    [Fact]
    public async Task An_explorer_buys_a_gift_card_for_another_explorer_and_pays_exactly_its_amount()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync("/api/payment/gift-cards",
            new BuyGiftCardDto("Explorer2@explorer.dev", 50, "Srećno na turi!"));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var giftCardId = await response.Content.ReadFromJsonAsync<Guid>(JsonOptions);
        using var context = Factory.CreateContext<PaymentDbContext>();
        var giftCard = await context.GiftCards.SingleAsync(card => card.Id == giftCardId);
        giftCard.GiverId.Should().Be(WellKnownUsers.Explorer);
        giftCard.RecipientId.Should().Be(WellKnownUsers.SecondExplorer);
        giftCard.Amount.Should().Be(50);
        giftCard.Message.Should().Be("Srećno na turi!");
        giftCard.ValidUntil.Should().Be(DateOnly.FromDateTime(giftCard.CreatedAt).AddYears(3));
        var wallet = await context.Wallets.Include(wallet => wallet.Transactions)
            .SingleAsync(wallet => wallet.UserId == WellKnownUsers.Explorer);
        wallet.Balance.Should().Be(WalletSeed.ExplorerWithPayments.Balance - 50);
        var payment = wallet.Transactions.Single(transaction => transaction.Kind == TransactionKind.GiftCard);
        payment.GiftCardId.Should().Be(giftCardId);
        payment.Amount.Should().Be(50);
    }

    [Fact]
    public async Task A_recipient_who_is_not_an_explorer_is_rejected_and_nothing_changes()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync("/api/payment/gift-cards",
            new BuyGiftCardDto("admin1@explorer.dev", 50, "Poklon"));

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        using var context = Factory.CreateContext<PaymentDbContext>();
        (await context.GiftCards.CountAsync()).Should().Be(GiftCardSeed.All.Length);
        var wallet = await context.Wallets.SingleAsync(wallet => wallet.UserId == WellKnownUsers.Explorer);
        wallet.Balance.Should().Be(WalletSeed.ExplorerWithPayments.Balance);
    }

    [Fact]
    public async Task A_gift_card_for_oneself_is_rejected_and_nothing_changes()
    {
        var client = Factory.CreateClientFor(WellKnownUsers.Explorer, "explorer");

        var response = await client.PostAsJsonAsync("/api/payment/gift-cards",
            new BuyGiftCardDto("explorer1@explorer.dev", 50, "Poklon"));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        using var context = Factory.CreateContext<PaymentDbContext>();
        (await context.GiftCards.CountAsync()).Should().Be(GiftCardSeed.All.Length);
        var wallet = await context.Wallets.Include(wallet => wallet.Transactions)
            .SingleAsync(wallet => wallet.UserId == WellKnownUsers.Explorer);
        wallet.Balance.Should().Be(WalletSeed.ExplorerWithPayments.Balance);
        wallet.Transactions.Should().HaveCount(WalletSeed.ExplorerWithPayments.Transactions.Count);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Only_signed_in_explorers_can_buy_gift_cards(bool administrator)
    {
        var client = administrator
            ? Factory.CreateClientFor(WellKnownUsers.Administrator, "administrator")
            : Factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/payment/gift-cards",
            new BuyGiftCardDto("explorer2@explorer.dev", 50, "Poklon"));

        response.StatusCode.Should().Be(administrator ? HttpStatusCode.Forbidden : HttpStatusCode.Unauthorized);
    }
}
