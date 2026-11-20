using Games.Domain.SocialChallengeRuns;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Games.Infrastructure.Persistence.SocialChallengeRuns;

internal static class SocialChallengeRunConfiguration
{
    public static void ConfigureSocialChallengeRuns(this ModelBuilder builder)
    {
        ConfigureSocialChallengeRun(builder.Entity<SocialChallengeRun>());
    }

    private static void ConfigureSocialChallengeRun(EntityTypeBuilder<SocialChallengeRun> builder)
    {
        builder.Property(run => run.Id).ValueGeneratedNever();
        builder.HasIndex(run => run.ChallengeId);
        builder.OwnsMany(run => run.Participants, ConfigureRunParticipant);
    }

    private static void ConfigureRunParticipant(OwnedNavigationBuilder<SocialChallengeRun, RunParticipant> builder)
    {
        builder.ToJson();
    }
}
