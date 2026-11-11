using Exploration.Domain.TourPreferences;
using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.TourPreferences;

public class PreferenceCriteriaTests
{
    [Fact]
    public void Criteria_without_any_difficulty_transport_or_tag_is_rejected()
    {
        var creation = () => new PreferenceCriteria([], null, []);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Criteria_with_only_tags_is_valid()
    {
        var criteria = new PreferenceCriteria([], null, ["istorija"]);

        criteria.Tags.Should().Equal("istorija");
    }

    [Fact]
    public void Criteria_with_a_repeated_difficulty_is_rejected()
    {
        var creation = () => new PreferenceCriteria([TourDifficulty.Easy, TourDifficulty.Easy], null, []);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("istorija")]
    public void Criteria_with_an_empty_or_repeated_tag_is_rejected(string secondTag)
    {
        var creation = () => new PreferenceCriteria([], null, ["istorija", secondTag]);

        creation.Should().Throw<DomainException>();
    }
}
