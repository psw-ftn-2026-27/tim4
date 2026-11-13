using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class TourPricingTests
{
    [Fact]
    public void Paid_pricing_with_the_smallest_positive_amount_is_valid()
    {
        var pricing = new TourPricing(TourPricingType.Paid, 0.01m);

        pricing.Type.Should().Be(TourPricingType.Paid);
        pricing.Amount.Should().Be(0.01m);
    }

    [Fact]
    public void Pricing_without_a_chosen_type_is_rejected()
    {
        var creation = () => new TourPricing(null, null);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData(0)]
    [InlineData(-5)]
    public void Paid_pricing_without_an_amount_greater_than_zero_is_rejected(int? amount)
    {
        var creation = () => new TourPricing(TourPricingType.Paid, amount);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(10)]
    public void Free_pricing_with_an_amount_is_rejected(int amount)
    {
        var creation = () => new TourPricing(TourPricingType.Free, amount);

        creation.Should().Throw<DomainException>();
    }
}
