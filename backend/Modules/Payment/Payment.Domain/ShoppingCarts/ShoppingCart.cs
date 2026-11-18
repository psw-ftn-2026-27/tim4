using Payment.Domain.Coupons;
using Payment.Domain.Products;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.ShoppingCarts;

public sealed class ShoppingCart : AggregateRoot
{
    private readonly List<OrderItem> _items = [];
    public Guid UserId { get; private set; }
    public Guid Version { get; private set; }
    public Guid? CheckoutId { get; private set; }
    public AppliedCoupon? AppliedCoupon { get; private set; }
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    public long Total => _items.Sum(item => item.Price);
    public long TotalToPay => Total - (AppliedCoupon?.Discount ?? 0);

    private ShoppingCart() { }

    public ShoppingCart(Guid userId) : base(Guid.NewGuid())
    {
        if (userId == Guid.Empty) throw new DomainException("Korpa mora da ima vlasnika.");
        UserId = userId;
        Version = Guid.NewGuid();
    }

    public void Add(Product product)
    {
        if (product.OwnerId == UserId)
            throw new DomainException("Ne možeš da kupiš sopstveni proizvod.");
        if (_items.Any(item => item.ProductId == product.Id))
            throw new DomainException("Proizvod je već u korpi.");
        var item = new OrderItem(product.Id, product.EntityId, product.OwnerId, product.Name, product.Price);
        EnsureTotalWithinRange(_items.Append(item));
        _items.Add(item);
        InvalidateCheckout();
    }

    public void Remove(Guid itemId)
    {
        var item = _items.SingleOrDefault(item => item.Id == itemId)
            ?? throw new NotFoundException("Stavka korpe ne postoji.");
        _items.Remove(item);
        InvalidateCheckout();
    }

    public Guid PrepareCheckout(IReadOnlyCollection<Product> products)
    {
        if (_items.Count == 0) throw new DomainException("Nije moguće kupiti praznu korpu.");
        var refreshed = _items.Select(item => products.SingleOrDefault(product => product.Id == item.ProductId)
            ?? throw new NotFoundException("Proizvod u korpi više ne postoji."))
            .Select(product => new OrderItem(product.Id, product.EntityId, product.OwnerId, product.Name, product.Price)).ToList();
        EnsureTotalWithinRange(refreshed);
        _items.Clear();
        _items.AddRange(refreshed);
        CheckoutId = Guid.NewGuid();
        AppliedCoupon = null;
        Version = Guid.NewGuid();
        return CheckoutId.Value;
    }

    internal void ApplyCoupon(Guid checkoutId, AppliedCoupon appliedCoupon)
    {
        EnsureCheckout(checkoutId);
        AppliedCoupon = appliedCoupon;
        Version = Guid.NewGuid();
    }

    public void RemoveCoupon(Guid checkoutId)
    {
        EnsureCheckout(checkoutId);
        AppliedCoupon = null;
        Version = Guid.NewGuid();
    }

    public void EnsureTotalToPay(long seenTotal)
    {
        if (seenTotal != TotalToPay)
            throw new DomainException("Cena za plaćanje se promenila. Osvežite prikaz i proverite novu cenu.");
    }

    public void EnsureCheckout(Guid checkoutId)
    {
        if (CheckoutId is null || CheckoutId != checkoutId || _items.Count == 0)
            throw new DomainException("Korpa je izmenjena. Ponovo pređi na plaćanje.");
    }

    public void CompleteCheckout(Guid checkoutId)
    {
        EnsureCheckout(checkoutId);
        _items.Clear();
        InvalidateCheckout();
    }

    private void InvalidateCheckout()
    {
        CheckoutId = null;
        AppliedCoupon = null;
        Version = Guid.NewGuid();
    }

    private static void EnsureTotalWithinRange(IEnumerable<OrderItem> items)
    {
        if (items.Sum(item => (decimal)item.Price) > long.MaxValue)
            throw new DomainException("Ukupan iznos korpe premašuje dozvoljenu vrednost.");
    }
}
