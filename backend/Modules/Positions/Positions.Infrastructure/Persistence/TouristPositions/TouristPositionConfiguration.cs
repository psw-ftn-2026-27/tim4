using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Positions.Domain.TouristPositions;

namespace Positions.Infrastructure.Persistence.TouristPositions;

internal static class TouristPositionConfiguration
{
    public static void ConfigureTouristPositions(this ModelBuilder modelBuilder)
    {
        ConfigureTouristPosition(modelBuilder.Entity<TouristPosition>());
    }

    private static void ConfigureTouristPosition(EntityTypeBuilder<TouristPosition> builder)
    {
        builder.Property(position => position.Id).ValueGeneratedNever();
        builder.HasIndex(position => position.TouristId).IsUnique();

        builder.OwnsOne(position => position.Coordinates, coordinates =>
        {
            coordinates.Property(point => point.Latitude).HasColumnName("Latitude");
            coordinates.Property(point => point.Longitude).HasColumnName("Longitude");
        });
        builder.Navigation(position => position.Coordinates).IsRequired();
    }
}
