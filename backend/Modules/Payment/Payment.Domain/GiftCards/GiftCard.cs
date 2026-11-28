using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.GiftCards;

public sealed class GiftCard : AggregateRoot
{
    public const int MinAmount = 10;
    public const int MaxAmount = 1000;
    public const int MaxMessageLength = 200;

    public Guid GiverId { get; private set; }
    public Guid RecipientId { get; private set; }
    public int Amount { get; private set; }
    public string Message { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateOnly ValidUntil { get; private set; }
    public Guid? RedeemedInPurchaseId { get; private set; }
    public Guid Version { get; private set; }

    private GiftCard()
    {
        Message = null!;
    }

    // Gift card dates follow UTC, like the rest of the backend: "today" is the UTC date of now.
    internal GiftCard(Guid giverId, Guid recipientId, int amount, string message, DateTime now)
        : base(Guid.NewGuid())
    {
        if (giverId == Guid.Empty || recipientId == Guid.Empty)
            throw new DomainException("Poklon kartica mora da ima darivaoca i primaoca.");
        if (recipientId == giverId)
            throw new DomainException("Ne možeš da pokloniš karticu sebi.");
        if (amount < MinAmount || amount > MaxAmount)
            throw new DomainException($"Iznos kartice mora biti ceo broj EC od {MinAmount} do {MaxAmount}.");
        var trimmed = message?.Trim() ?? "";
        if (trimmed.Length == 0 || trimmed.Length > MaxMessageLength)
            throw new DomainException($"Poruka ne sme biti prazna i može imati najviše {MaxMessageLength} karaktera.");

        GiverId = giverId;
        RecipientId = recipientId;
        Amount = amount;
        Message = trimmed;
        CreatedAt = now;
        ValidUntil = DateOnly.FromDateTime(now).AddYears(3);
        Version = Guid.NewGuid();
    }

    public GiftCardStatus GetStatus(DateOnly today) => GetStatus(RedeemedInPurchaseId, ValidUntil, today);

    public static GiftCardStatus GetStatus(Guid? redeemedInPurchaseId, DateOnly validUntil, DateOnly today) =>
        redeemedInPurchaseId is not null ? GiftCardStatus.Redeemed
        : today > validUntil ? GiftCardStatus.Expired
        : GiftCardStatus.Valid;

    public bool IsUsableBy(Guid buyerId, DateOnly today) =>
        RecipientId == buyerId && GetStatus(today) == GiftCardStatus.Valid;

    // When the buyer has several usable cards, the one that expires first is used.
    public static GiftCard? FirstToExpire(IEnumerable<GiftCard> giftCards, Guid buyerId, DateOnly today) =>
        giftCards.Where(card => card.IsUsableBy(buyerId, today))
            .OrderBy(card => card.ValidUntil).ThenBy(card => card.CreatedAt).ThenBy(card => card.Id)
            .FirstOrDefault();

    internal void Redeem(Guid buyerId, Guid purchaseId, DateOnly today)
    {
        EnsureUsableBy(buyerId, today);
        RedeemedInPurchaseId = purchaseId;
        Version = Guid.NewGuid();
    }

    internal void EnsureUsableBy(Guid buyerId, DateOnly today)
    {
        if (RecipientId != buyerId)
            throw new DomainException("Možeš da iskoristiš samo poklon karticu koju si dobio.");
        switch (GetStatus(today))
        {
            case GiftCardStatus.Redeemed: throw new DomainException("Poklon kartica je već iskorišćena.");
            case GiftCardStatus.Expired: throw new DomainException("Poklon kartica je istekla.");
        }
    }
}
