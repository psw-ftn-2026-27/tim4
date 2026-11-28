using Exploration.Domain.Tours;

namespace Exploration.Application.TourAuthoring;

public sealed record PublishTourDto(
    TourPricingType? PricingType,
    decimal? PricingAmount);
