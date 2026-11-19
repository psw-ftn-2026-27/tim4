using System.Globalization;
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
        var pricing = new TourPricing(TourPricingType.Paid, 1m);

        pricing.Type.Should().Be(TourPricingType.Paid);
        pricing.Amount.Should().Be(1m);
    }

    [Theory]
    [InlineData("0.5")]
    [InlineData("1500.50")]
    [InlineData("30.01")]
    public void Paid_pricing_with_an_amount_that_is_not_a_whole_number_is_rejected(string amount)
    {
        var creation = () => new TourPricing(TourPricingType.Paid, decimal.Parse(amount, CultureInfo.InvariantCulture));

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Free_tour_costs_zero_ec()
    {
        var pricing = new TourPricing(TourPricingType.Free, null);

        var price = pricing.PriceInEc();

        price.Should().Be(0);
    }

    [Fact]
    public void Paid_tour_costs_its_amount_in_ec()
    {
        var pricing = new TourPricing(TourPricingType.Paid, 1500m);

        var price = pricing.PriceInEc();

        price.Should().Be(1500);
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
