using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class RecommendedGroupSizeTests
{
    [Fact]
    public void Recommended_group_size_may_have_the_same_minimum_and_maximum()
    {
        var groupSize = new RecommendedGroupSize(4, 4);

        groupSize.Min.Should().Be(4);
        groupSize.Max.Should().Be(4);
    }

    [Theory]
    [InlineData(0, 5)]
    [InlineData(-2, 5)]
    [InlineData(0, 0)]
    [InlineData(-3, -1)]
    public void Recommended_group_size_below_one_person_is_rejected(int min, int max)
    {
        var creation = () => new RecommendedGroupSize(min, max);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Recommended_group_size_with_a_minimum_greater_than_the_maximum_is_rejected()
    {
        var creation = () => new RecommendedGroupSize(6, 5);

        creation.Should().Throw<DomainException>();
    }
}
