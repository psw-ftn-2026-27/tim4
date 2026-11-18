using Exploration.Application.LocationSearchRecords;
using Exploration.Domain.LocationSearchRecords;
using Exploration.Domain.Tours;

namespace Exploration.Application.LocationSearches;

public sealed class LocationSearchesService
{
    private readonly ILocationSearchRecordRepository _recordRepository;
    private readonly IUnitOfWork _unitOfWork;

    public LocationSearchesService(ILocationSearchRecordRepository recordRepository, IUnitOfWork unitOfWork)
    {
        _recordRepository = recordRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task RecordAsync(Guid touristId, RecordLocationSearchDto dto)
    {
        var center = new Location(dto.Latitude, dto.Longitude);
        var criteria = new LocationSearchCriteria(dto.Name, dto.Difficulties, dto.Tags, dto.Transport,
            dto.PriceFrom, dto.PriceTo, dto.LengthFrom, dto.LengthTo);
        var record = new LocationSearchRecord(touristId, center, dto.RadiusInKm, criteria, dto.ResultCount, DateTime.UtcNow);

        _recordRepository.Add(record);
        await _unitOfWork.SaveChangesAsync();
    }
}
