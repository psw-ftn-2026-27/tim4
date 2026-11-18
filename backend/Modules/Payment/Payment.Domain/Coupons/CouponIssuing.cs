using Payment.Domain.Products;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Coupons;

public sealed class CouponIssuing
{
    public Coupon Issue(Guid ownerId, IReadOnlyCollection<Product> ownerProducts, Guid? productId,
        int discountPercent, DateOnly validUntil, DateOnly today)
    {
        var owned = ownerProducts.Where(product => product.OwnerId == ownerId).ToList();
        if (owned.Count == 0)
            throw new DomainException("Kupon može da napravi samo vlasnik proizvoda.");
        if (productId is not null && owned.All(product => product.Id != productId))
            throw new DomainException("Kupon može da važi samo za tvoj proizvod.");

        return new Coupon(ownerId, productId, discountPercent, validUntil, today);
    }
}
