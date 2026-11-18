using Exploration.Application.LocationSearchRecords;
using Exploration.Domain.LocationSearchRecords;

namespace Exploration.Infrastructure.Persistence.LocationSearchRecords;

internal sealed class LocationSearchRecordRepository : ILocationSearchRecordRepository
{
    private readonly ExplorationDbContext _dbContext;

    public LocationSearchRecordRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public void Add(LocationSearchRecord record)
    {
        _dbContext.LocationSearchRecords.Add(record);
    }
}
