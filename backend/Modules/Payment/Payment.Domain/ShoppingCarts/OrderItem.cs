using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.ShoppingCarts;

public sealed class OrderItem : Entity
{
    public Guid ProductId { get; private set; }
    public Guid EntityId { get; private set; }
    public Guid OwnerId { get; private set; }
    public string Name { get; private set; }
    public long Price { get; private set; }

    private OrderItem() { Name = null!; }

    internal OrderItem(Guid productId, Guid entityId, Guid ownerId, string name, long price) : base(Guid.NewGuid())
    {
        if (price < 0)
            throw new DomainException("Cena proizvoda ne može biti negativna.");
        ProductId = productId;
        EntityId = entityId;
        OwnerId = ownerId;
        Name = name;
        Price = price;
    }
}
