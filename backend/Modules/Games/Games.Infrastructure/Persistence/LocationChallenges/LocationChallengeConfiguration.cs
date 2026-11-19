using Games.Domain.LocationChallenges;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.LocationChallenges;

internal static class LocationChallengeConfiguration
{
    public static void ConfigureLocationChallenges(this ModelBuilder builder)
    {
        ConfigureChallenge(builder.Entity<LocationChallenge>());
    }

    private static void ConfigureChallenge(EntityTypeBuilder<LocationChallenge> challenge)
    {
        challenge.Property(item => item.Id).ValueGeneratedNever();
        challenge.Property(item => item.Name).HasMaxLength(200);
        challenge.OwnsMany(item => item.HiddenLocations, ConfigureHiddenLocations);
        challenge.Navigation(item => item.HiddenLocations).HasField("_hiddenLocations");
    }

    private static void ConfigureHiddenLocations(OwnedNavigationBuilder<LocationChallenge, HiddenLocation> points)
    {
        points.ToTable("HiddenLocations");
        points.WithOwner().HasForeignKey("LocationChallengeId");
        points.HasKey("LocationChallengeId", nameof(HiddenLocation.PointId));
    }
}
