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
    public IReadOnlyList<OrderItem> Items => _items.AsReadOnly();
    public long Total => _items.Sum(item => item.Price);

    private ShoppingCart() { }

    public ShoppingCart(Guid userId) : base(Guid.NewGuid())
    {
        if (userId == Guid.Empty) throw new DomainException("A cart requires an owner.");
        UserId = userId;
        Version = Guid.NewGuid();
    }

    public void Add(Product product)
    {
        if (product.OwnerId == UserId)
            throw new DomainException("Ne mozes da kupis sopstveni proizvod.");
        if (_items.Any(item => item.ProductId == product.Id))
            throw new DomainException("Proizvod je vec u korpi.");
        var item = new OrderItem(product.Id, product.EntityId, product.Name, product.Price);
        EnsureTotalWithinRange(_items.Append(item));
        _items.Add(item);
        InvalidateCheckout();
    }

    public void Remove(Guid itemId)
    {
        var item = _items.SingleOrDefault(item => item.Id == itemId)
            ?? throw new NotFoundException("The cart item does not exist.");
        _items.Remove(item);
        InvalidateCheckout();
    }

    public Guid PrepareCheckout(IReadOnlyCollection<Product> products)
    {
        if (_items.Count == 0) throw new DomainException("An empty cart cannot be purchased.");
        var refreshed = _items.Select(item => products.SingleOrDefault(product => product.Id == item.ProductId)
            ?? throw new NotFoundException("A product in the cart no longer exists."))
            .Select(product => new OrderItem(product.Id, product.EntityId, product.Name, product.Price)).ToList();
        EnsureTotalWithinRange(refreshed);
        _items.Clear();
        _items.AddRange(refreshed);
        CheckoutId = Guid.NewGuid();
        Version = Guid.NewGuid();
        return CheckoutId.Value;
    }

    public void EnsureCheckout(Guid checkoutId)
    {
        if (CheckoutId is null || CheckoutId != checkoutId || _items.Count == 0)
            throw new DomainException("Korpa je izmenjena. Ponovo pripremi checkout.");
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
        Version = Guid.NewGuid();
    }

    private static void EnsureTotalWithinRange(IEnumerable<OrderItem> items)
    {
        if (items.Sum(item => (decimal)item.Price) > long.MaxValue)
            throw new DomainException("The cart total is too large.");
    }
}
