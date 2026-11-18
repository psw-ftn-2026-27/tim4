using Exploration.Domain.LocationSearchRecords;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Exploration.Infrastructure.Persistence.LocationSearchRecords;

internal static class LocationSearchRecordConfiguration
{
    public static void ConfigureLocationSearchRecords(this ModelBuilder modelBuilder)
    {
        ConfigureLocationSearchRecord(modelBuilder.Entity<LocationSearchRecord>());
    }

    private static void ConfigureLocationSearchRecord(EntityTypeBuilder<LocationSearchRecord> builder)
    {
        builder.OwnsOne(record => record.Center);
        builder.Navigation(record => record.Center).IsRequired();

        builder.OwnsOne(record => record.Criteria, criteria =>
        {
            criteria.ToJson();
            criteria.PrimitiveCollection(c => c.Difficulties).ElementType().HasConversion<string>();
            criteria.Property(c => c.Transport).HasConversion<string>();
        });
    }
}
