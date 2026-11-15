using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.ShoppingCarts;

public sealed class OrderItem : Entity
{
    public Guid ProductId { get; private set; }
    public Guid EntityId { get; private set; }
    public string Name { get; private set; }
    public long Price { get; private set; }

    private OrderItem() { Name = null!; }

    internal OrderItem(Guid productId, Guid entityId, string name, decimal price) : base(Guid.NewGuid())
    {
        if (price < 0 || price > long.MaxValue || price != decimal.Truncate(price))
            throw new DomainException("A product price must be a whole number of EC within the supported range.");
        ProductId = productId;
        EntityId = entityId;
        Name = name;
        Price = (long)price;
    }
}
