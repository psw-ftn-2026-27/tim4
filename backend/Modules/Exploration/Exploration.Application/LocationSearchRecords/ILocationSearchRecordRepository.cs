using Exploration.Domain.LocationSearchRecords;

namespace Exploration.Application.LocationSearchRecords;

public interface ILocationSearchRecordRepository
{
    void Add(LocationSearchRecord record);
}
