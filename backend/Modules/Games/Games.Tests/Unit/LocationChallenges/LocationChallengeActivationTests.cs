using FluentAssertions;
using Games.Application.LocationChallengePlaying;
using Xunit;

namespace Games.Tests.Unit.LocationChallenges;

public sealed class LocationChallengeActivationTests
{
    [Fact]
    public void Activation_defaults_to_fifty_meters()
    {
        var activation = new LocationChallengeActivation(null);

        activation.DistanceInMeters.Should().Be(50);
    }
}
