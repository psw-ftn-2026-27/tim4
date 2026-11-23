using Games.Domain.Experience;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.Experience;

internal static class EarnedXpConfiguration
{
    public static void ConfigureXpAwards(this ModelBuilder builder)
    {
        ConfigureEarnedXp(builder.Entity<EarnedXp>());
    }

    private static void ConfigureEarnedXp(EntityTypeBuilder<EarnedXp> award)
    {
        award.Property(item => item.Id).ValueGeneratedNever();
        award.HasIndex(item => new { item.UserId, item.ChallengeType, item.ChallengeId }).IsUnique();
    }
}
