using Payment.Domain.Coupons;
using Payment.Domain.GiftCards;
using Payment.Domain.ShoppingCarts;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Purchases;

public sealed class Purchase : AggregateRoot
{
    private readonly List<PurchaseItem> _items = [];
    public Guid UserId { get; private set; }
    public Guid CheckoutId { get; private set; }
    public DateTime PurchasedAt { get; private set; }
    public AppliedCoupon? AppliedCoupon { get; private set; }
    public AppliedGiftCard? AppliedGiftCard { get; private set; }
    public long GiftCardDiscount { get; private set; }
    public IReadOnlyList<PurchaseItem> Items => _items.AsReadOnly();
    public long Total => _items.Sum(item => item.Price);

    private Purchase() { }

    internal Purchase(Guid userId, Guid checkoutId, IReadOnlyCollection<OrderItem> items,
        AppliedCoupon? appliedCoupon, AppliedGiftCard? appliedGiftCard, long giftCardDiscount, DateTime purchasedAt)
        : base(Guid.NewGuid())
    {
        if (userId == Guid.Empty || checkoutId == Guid.Empty || items.Count == 0)
            throw new DomainException("Kupovina mora da ima kupca i najmanje jednu stavku za plaćanje.");
        if (giftCardDiscount < 0 || (appliedGiftCard is null && giftCardDiscount != 0))
            throw new DomainException("Umanjenje poklon karticom mora da pripada primenjenoj kartici.");
        UserId = userId;
        CheckoutId = checkoutId;
        PurchasedAt = purchasedAt;
        AppliedCoupon = appliedCoupon is null ? null : appliedCoupon with { };
        AppliedGiftCard = appliedGiftCard is null ? null : appliedGiftCard with { };
        GiftCardDiscount = giftCardDiscount;
        // Every item keeps the price paid from the wallet: the coupon discount goes to its product, and the gift
        // card discount covers the items in order until it is spent.
        var giftCardLeft = giftCardDiscount;
        foreach (var item in items)
        {
            var price = item.ProductId == appliedCoupon?.ProductId ? item.Price - appliedCoupon.Discount : item.Price;
            var coveredByGiftCard = Math.Min(giftCardLeft, price);
            giftCardLeft -= coveredByGiftCard;
            _items.Add(new PurchaseItem(userId, item, price - coveredByGiftCard));
        }
    }
}
