using Exploration.Domain.Tours;
using FluentAssertions;
using Shared.Domain.Exceptions;
using Xunit;

namespace Exploration.Tests.Unit.Tours;

public class KeyPointTests
{
    [Theory]
    [InlineData("", "Opis tačke.", "/images/tacka.jpg")]
    [InlineData("   ", "Opis tačke.", "/images/tacka.jpg")]
    [InlineData("Trg slobode", "", "/images/tacka.jpg")]
    [InlineData("Trg slobode", "   ", "/images/tacka.jpg")]
    [InlineData("Trg slobode", "Opis tačke.", "")]
    [InlineData("Trg slobode", "Opis tačke.", "   ")]
    public void Key_point_requires_a_name_a_description_and_an_image(string name, string description, string imageAddress)
    {
        var creation = () => new KeyPoint(name, description, imageAddress, null, new Location(45.2551, 19.8452), true);

        creation.Should().Throw<DomainException>();
    }

    [Fact]
    public void Key_point_requires_a_location()
    {
        var creation = () => new KeyPoint("Trg slobode", "Opis tačke.", "/images/tacka.jpg", null, null!, true);

        creation.Should().Throw<DomainException>();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Key_point_without_a_secret_text_has_no_secret(string? secret)
    {
        var keyPoint = new KeyPoint("Trg slobode", "Opis tačke.", "/images/tacka.jpg", secret, new Location(45.2551, 19.8452), true);

        keyPoint.Secret.Should().BeNull();
    }
}
