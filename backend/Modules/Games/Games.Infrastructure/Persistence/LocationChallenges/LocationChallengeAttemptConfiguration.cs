using Games.Domain.LocationChallenges;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.LocationChallenges;

internal static class LocationChallengeAttemptConfiguration
{
    public static void ConfigureLocationChallengeAttempts(this ModelBuilder builder)
    {
        ConfigureAttempt(builder.Entity<LocationChallengeAttempt>());
    }

    private static void ConfigureAttempt(EntityTypeBuilder<LocationChallengeAttempt> attempt)
    {
        attempt.Property(item => item.Id).ValueGeneratedNever();
        attempt.Property(item => item.Version).IsConcurrencyToken();
        attempt.Ignore(item => item.DiscoveredPointIds);
        attempt.Property<Guid[]>("_discoveredPointIds").HasColumnName("DiscoveredPointIds");
        attempt.HasIndex(item => new { item.ChallengeId, item.UserId }).IsUnique();
        attempt.HasOne<LocationChallenge>().WithMany().HasForeignKey(item => item.ChallengeId);
    }
}
