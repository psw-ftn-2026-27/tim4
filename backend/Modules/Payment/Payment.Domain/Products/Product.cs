using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Payment.Domain.Products;

public sealed class Product : AggregateRoot
{
    public Guid EntityId { get; private set; }
    public string Name { get; private set; }
    public Guid OwnerId { get; private set; }
    public long Price { get; private set; }

    private Product()
    {
        Name = null!;
    }

    public Product(Guid entityId, string name, Guid ownerId, long price) : base(Guid.NewGuid())
    {
        if (entityId == Guid.Empty || ownerId == Guid.Empty)
        {
            throw new DomainException("Proizvod mora da ima identifikatore sadržaja i vlasnika.");
        }
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("Proizvod mora da ima naziv.");
        }
        if (price < 0)
        {
            throw new DomainException("Cena proizvoda ne može biti negativna.");
        }

        EntityId = entityId;
        Name = name;
        OwnerId = ownerId;
        Price = price;
    }

    public void ChangePrice(Guid ownerId, long price)
    {
        if (ownerId != OwnerId)
            throw new DomainException("Samo vlasnik proizvoda može da promeni njegovu cenu.");
        if (price < 0)
            throw new DomainException("Cena proizvoda ne može biti negativna.");
        Price = price;
    }

    public void EnsureSameOffer(Product other)
    {
        if (EntityId != other.EntityId || Name != other.Name || OwnerId != other.OwnerId || Price != other.Price)
        {
            throw new DomainException("Za ovaj sadržaj već postoji proizvod sa drugačijim podacima.");
        }
    }
}
