using Payment.Domain.ShoppingCarts;
using Shared.Domain;

namespace Payment.Domain.Purchases;

public sealed class PurchaseItem : Entity
{
    public Guid BuyerId { get; private set; }
    public Guid ProductId { get; private set; }
    public Guid EntityId { get; private set; }
    public string Name { get; private set; }
    public long Price { get; private set; }

    private PurchaseItem() { Name = null!; }

    internal PurchaseItem(Guid buyerId, OrderItem item) : base(Guid.NewGuid())
    {
        BuyerId = buyerId;
        ProductId = item.ProductId;
        EntityId = item.EntityId;
        Name = item.Name;
        Price = item.Price;
    }
}
