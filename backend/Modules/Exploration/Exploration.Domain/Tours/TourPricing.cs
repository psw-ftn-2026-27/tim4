using Shared.Domain.Exceptions;

namespace Exploration.Domain.Tours;

public sealed record TourPricing
{
    public TourPricingType Type { get; private init; }
    public decimal? Amount { get; private init; }

    public TourPricing(TourPricingType? type, decimal? amount)
    {
        if (type is null || !Enum.IsDefined(type.Value))
        {
            throw new DomainException("A tour pricing requires a choice between free and paid.");
        }
        if (type == TourPricingType.Paid && (amount is null || amount <= 0))
        {
            throw new DomainException("A paid tour requires an amount greater than zero.");
        }
        if (type == TourPricingType.Free && amount is not null)
        {
            throw new DomainException("A free tour cannot have an amount.");
        }

        Type = type.Value;
        Amount = amount;
    }
}
