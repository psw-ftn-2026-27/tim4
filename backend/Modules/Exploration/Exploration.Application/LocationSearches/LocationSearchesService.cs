using Exploration.Application.LocationSearchRecords;
using Exploration.Application.TourBrowsing;
using Exploration.Domain.LocationSearchRecords;
using Exploration.Domain.Tours;

namespace Exploration.Application.LocationSearches;

public sealed class LocationSearchesService
{
    private readonly ILocationSearchRecordRepository _recordRepository;
    private readonly TourBrowsingQueries _tourBrowsingQueries;
    private readonly IUnitOfWork _unitOfWork;

    public LocationSearchesService(ILocationSearchRecordRepository recordRepository,
        TourBrowsingQueries tourBrowsingQueries, IUnitOfWork unitOfWork)
    {
        _recordRepository = recordRepository;
        _tourBrowsingQueries = tourBrowsingQueries;
        _unitOfWork = unitOfWork;
    }

    public async Task RecordAsync(Guid touristId, RecordLocationSearchDto dto)
    {
        // The number of tours found comes from the same search the tourist sees, so the client
        // cannot send a wrong one. Only the total count is needed, so one tour per page is enough.
        var search = new PublishedTourSearchDto(dto.Name, dto.Difficulties, dto.Tags, dto.Transport,
            dto.PriceFrom, dto.PriceTo, dto.LengthFrom, dto.LengthTo, null,
            dto.Latitude, dto.Longitude, dto.RadiusInKm);
        var found = await _tourBrowsingQueries.GetPublishedAsync(search, 1, 1);

        var center = new Location(dto.Latitude, dto.Longitude);
        var criteria = new LocationSearchCriteria(dto.Name, dto.Difficulties, dto.Tags, dto.Transport,
            dto.PriceFrom, dto.PriceTo, dto.LengthFrom, dto.LengthTo);
        var record = new LocationSearchRecord(touristId, center, dto.RadiusInKm, criteria, found.TotalCount, DateTime.UtcNow);

        _recordRepository.Add(record);
        await _unitOfWork.SaveChangesAsync();
    }
}
