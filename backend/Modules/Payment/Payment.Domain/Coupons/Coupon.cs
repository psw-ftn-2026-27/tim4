using Payment.Domain.ShoppingCarts;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Coupons;

public sealed class Coupon : AggregateRoot
{
    public Guid OwnerId { get; private set; }
    public Guid? ProductId { get; private set; }
    public CouponCode Code { get; private set; }
    public int DiscountPercent { get; private set; }
    public DateOnly ValidUntil { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private Coupon()
    {
        Code = null!;
    }

    // Coupon dates follow UTC, like the rest of the backend: "today" is the UTC date of now.
    internal Coupon(Guid ownerId, Guid? productId, int discountPercent, DateOnly validUntil, DateTime now)
        : base(Guid.NewGuid())
    {
        if (discountPercent < 1 || discountPercent > 100)
            throw new DomainException("Procenat popusta mora biti ceo broj od 1 do 100.");
        if (validUntil < DateOnly.FromDateTime(now))
            throw new DomainException("Datum važenja kupona ne može biti u prošlosti.");

        OwnerId = ownerId;
        ProductId = productId;
        Code = CouponCode.Generate();
        DiscountPercent = discountPercent;
        ValidUntil = validUntil;
        CreatedAt = now;
    }

    public bool IsValidOn(DateOnly today) => today <= ValidUntil;

    public OrderItem AppliesTo(IReadOnlyCollection<OrderItem> items)
    {
        var item = ProductId is not null
            ? items.FirstOrDefault(item => item.ProductId == ProductId)
            : items.Where(item => item.OwnerId == OwnerId).MaxBy(item => item.Price);
        return item ?? throw new DomainException("U korpi nema proizvoda za koji ovaj kupon važi.");
    }

    public long DiscountFor(long price) => (long)((decimal)price * DiscountPercent / 100);
}
