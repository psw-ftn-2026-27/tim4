using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed class KeyPoint : Entity
{
    public string Name { get; private set; }
    public string Description { get; private set; }
    public string ImageAddress { get; private set; }
    public string? Secret { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public bool IsVisibleWithoutPurchase { get; private set; }

    private KeyPoint()
    {
        Name = null!;
        Description = null!;
        ImageAddress = null!;
    }

    public KeyPoint(string name, string description, string imageAddress, string? secret,
        double latitude, double longitude, bool isVisibleWithoutPurchase)
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
        if (!double.IsFinite(latitude) || latitude < -90 || latitude > 90)
        {
            throw new DomainException("A key point latitude must be between -90 and 90 degrees.");
        }
        if (!double.IsFinite(longitude) || longitude < -180 || longitude > 180)
        {
            throw new DomainException("A key point longitude must be between -180 and 180 degrees.");
        }

        Name = name;
        Description = description;
        ImageAddress = imageAddress;
        Secret = string.IsNullOrWhiteSpace(secret) ? null : secret;
        Latitude = latitude;
        Longitude = longitude;
        IsVisibleWithoutPurchase = isVisibleWithoutPurchase;
    }

    internal void ChangeTo(KeyPoint changes)
    {
        Name = changes.Name;
        Description = changes.Description;
        ImageAddress = changes.ImageAddress;
        Secret = changes.Secret;
        Latitude = changes.Latitude;
        Longitude = changes.Longitude;
        IsVisibleWithoutPurchase = changes.IsVisibleWithoutPurchase;
    }
}
