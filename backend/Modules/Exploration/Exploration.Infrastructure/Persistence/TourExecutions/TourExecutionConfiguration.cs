using Exploration.Domain.TourExecutions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exploration.Infrastructure.Persistence.TourExecutions;

internal static class TourExecutionConfiguration
{
    public static void ConfigureTourExecutions(this ModelBuilder modelBuilder)
    {
        ConfigureTourExecution(modelBuilder.Entity<TourExecution>());
    }

    private static void ConfigureTourExecution(EntityTypeBuilder<TourExecution> builder)
    {
        builder.Property(execution => execution.Status).HasConversion<string>().HasMaxLength(20);
        // A tourist has at most one session in progress; the database also guards it against two
        // starts that arrive at the same moment (the second save becomes a 409).
        builder.HasIndex(execution => execution.TouristId)
            .IsUnique()
            .HasFilter("\"Status\" = 'InProgress'");

        builder.OwnsOne(execution => execution.StartLocation);
        builder.Navigation(execution => execution.StartLocation).IsRequired();

        builder.OwnsMany(execution => execution.ReachedKeyPoints, reachedKeyPoints =>
        {
            reachedKeyPoints.ToJson();
        });
    }
}
