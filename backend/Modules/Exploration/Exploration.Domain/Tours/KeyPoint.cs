using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed class KeyPoint : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string ImageAddress { get; private set; }
    public string? Secret { get; private set; }
    public Location Location { get; private set; }
    public bool IsVisibleWithoutPurchase { get; private set; }

    private KeyPoint()
    {
        Name = null!;
        Description = null!;
        ImageAddress = null!;
        Location = null!;
    }

    public KeyPoint(string name, string description, string imageAddress, string? secret,
        Location location, bool isVisibleWithoutPurchase)
        : base(Guid.NewGuid())
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new DomainException("A key point requires a name.");
        }
        if (string.IsNullOrWhiteSpace(description))
        {
            throw new DomainException("A key point requires a description.");
        }
        if (string.IsNullOrWhiteSpace(imageAddress))
        {
            throw new DomainException("A key point requires an image.");
        }
        if (location is null)
        {
            throw new DomainException("A key point requires a location.");
        }

        Name = name;
        Description = description;
        ImageAddress = imageAddress;
        Secret = string.IsNullOrWhiteSpace(secret) ? null : secret;
        Location = location;
        IsVisibleWithoutPurchase = isVisibleWithoutPurchase;
    }

    internal void ChangeTo(KeyPoint changes)
    {
        Name = changes.Name;
        Description = changes.Description;
        ImageAddress = changes.ImageAddress;
        Secret = changes.Secret;
        Location = changes.Location;
        IsVisibleWithoutPurchase = changes.IsVisibleWithoutPurchase;
    }
}
