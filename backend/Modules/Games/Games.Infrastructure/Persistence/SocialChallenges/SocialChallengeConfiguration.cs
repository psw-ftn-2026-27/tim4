using Games.Domain.SocialChallenges;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.SocialChallenges;

internal static class SocialChallengeConfiguration
{
    public static void ConfigureSocialChallenges(this ModelBuilder builder)
    {
        ConfigureSocialChallenge(builder.Entity<SocialChallenge>());
    }

    private static void ConfigureSocialChallenge(EntityTypeBuilder<SocialChallenge> builder)
    {
        builder.Property(challenge => challenge.Id).ValueGeneratedNever();
        builder.Property(challenge => challenge.Status).HasConversion<string>().HasMaxLength(20);
        builder.OwnsOne(challenge => challenge.ActivityTime, activityTime =>
        {
            activityTime.Property(time => time.Start).HasColumnName("Start");
            activityTime.Property(time => time.End).HasColumnName("End");
        });
        builder.Navigation(challenge => challenge.ActivityTime).IsRequired();
    }
}
