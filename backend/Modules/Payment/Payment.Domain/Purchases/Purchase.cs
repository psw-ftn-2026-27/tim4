using Payment.Domain.Coupons;
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
    public IReadOnlyList<PurchaseItem> Items => _items.AsReadOnly();
    public long Total => _items.Sum(item => item.Price);

    private Purchase() { }

    internal Purchase(Guid userId, Guid checkoutId, IReadOnlyCollection<OrderItem> items,
        AppliedCoupon? appliedCoupon, DateTime purchasedAt)
        : base(Guid.NewGuid())
    {
        if (userId == Guid.Empty || checkoutId == Guid.Empty || items.Count == 0)
            throw new DomainException("Kupovina mora da ima kupca i najmanje jednu stavku za plaćanje.");
        UserId = userId;
        CheckoutId = checkoutId;
        PurchasedAt = purchasedAt;
        AppliedCoupon = appliedCoupon is null ? null : appliedCoupon with { };
        _items.AddRange(items.Select(item => new PurchaseItem(userId, item,
            item.ProductId == appliedCoupon?.ProductId ? item.Price - appliedCoupon.Discount : item.Price)));
    }
}
