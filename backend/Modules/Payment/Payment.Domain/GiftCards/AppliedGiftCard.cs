namespace Payment.Domain.GiftCards;

// The gift card chosen for a checkout. The discount is not stored: it is the card amount, but never more
// than the price after the coupon, because the card is used in full and its remainder is lost.
public sealed record AppliedGiftCard
{
    public Guid GiftCardId { get; private init; }
    public int Amount { get; private init; }

    private AppliedGiftCard() { }

    internal AppliedGiftCard(GiftCard giftCard)
    {
        GiftCardId = giftCard.Id;
        Amount = giftCard.Amount;
    }

    public long DiscountOn(long price) => DiscountOn(Amount, price);

    public static long DiscountOn(long amount, long price) => Math.Min(amount, Math.Max(price, 0));
}
