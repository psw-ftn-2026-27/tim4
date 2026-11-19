using Exploration.Application.KeyPointAuthoring;
using Exploration.Application.TourBrowsing;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Microsoft.EntityFrameworkCore;
using Shared.Domain;

namespace Exploration.Infrastructure.Persistence.Tours;

internal sealed class TourReadRepository : ITourReadRepository
{
    private readonly ExplorationDbContext _dbContext;

    public TourReadRepository(ExplorationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<List<TourDto>> GetByAuthorAsync(Guid authorId)
    {
        return ProjectToDtos(_dbContext.Tours.Where(tour => tour.AuthorId == authorId)).ToListAsync();
    }

    public async Task<PageResult<TourDto>> GetPublishedAsync(PublishedTourSearchDto search, int page, int pageSize)
    {
        var published = Filter(_dbContext.Tours.Where(tour => tour.Status == TourStatus.Published), search);
        var ordered = Sort(published, search.Sort);

        var totalCount = await published.CountAsync();
        var items = await ProjectToDtos(ordered)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<TourDto>(items, totalCount);
    }

    public Task<List<TourDto>> GetAllPublishedAsync(PublishedTourSearchDto search)
    {
        var published = Filter(_dbContext.Tours.Where(tour => tour.Status == TourStatus.Published), search);
        return ProjectToDtos(Sort(published, search.Sort)).ToListAsync();
    }

    public async Task<Dictionary<Guid, List<LocationDto>>> GetVisibleKeyPointLocationsAsync(List<Guid> tourIds)
    {
        var tours = await _dbContext.Tours
            .Where(tour => tourIds.Contains(tour.Id))
            .Select(tour => new
            {
                tour.Id,
                KeyPoints = tour.KeyPoints
                    .Select(keyPoint => new
                    {
                        keyPoint.IsVisibleWithoutPurchase,
                        keyPoint.Location.Latitude,
                        keyPoint.Location.Longitude
                    })
                    .ToList()
            })
            .ToListAsync();

        // The key points are filtered here, after loading, so they keep the order of the tour.
        return tours.ToDictionary(
            tour => tour.Id,
            tour => tour.KeyPoints
                .Where(keyPoint => keyPoint.IsVisibleWithoutPurchase)
                .Select(keyPoint => new LocationDto(keyPoint.Latitude, keyPoint.Longitude))
                .ToList());
    }

    public Task<TourDetailsDto?> GetPublishedDetailsAsync(Guid tourId)
    {
        return _dbContext.Tours
            .Where(tour => tour.Id == tourId && tour.Status == TourStatus.Published)
            .Select(tour => new TourDetailsDto(
                tour.Id,
                tour.AuthorId,
                tour.Name,
                tour.Description,
                tour.Difficulty,
                tour.Tags,
                tour.LengthInKm,
                tour.TransportTimes
                    .Select(time => new TransportTimeDto(time.Transport, time.Minutes))
                    .ToList(),
                tour.KeyPoints
                    .Select(keyPoint => new TourKeyPointDto(
                        keyPoint.Id,
                        keyPoint.Name,
                        keyPoint.Description,
                        keyPoint.ImageAddress,
                        keyPoint.Secret,
                        keyPoint.Location.Latitude,
                        keyPoint.Location.Longitude,
                        keyPoint.IsVisibleWithoutPurchase))
                    .ToList(),
                // Purchases live in Payment, so the queries class fills this in.
                false))
            .SingleOrDefaultAsync();
    }

    public async Task<List<KeyPointDto>?> GetKeyPointsOfAuthoredTourAsync(Guid tourId, Guid authorId)
    {
        var tour = await _dbContext.Tours
            .Where(tour => tour.Id == tourId && tour.AuthorId == authorId)
            .Select(tour => new
            {
                KeyPoints = tour.KeyPoints
                    .Select(keyPoint => new KeyPointDto(
                        keyPoint.Id,
                        keyPoint.Name,
                        keyPoint.Description,
                        keyPoint.ImageAddress,
                        keyPoint.Secret,
                        keyPoint.Location.Latitude,
                        keyPoint.Location.Longitude,
                        keyPoint.IsVisibleWithoutPurchase))
                    .ToList()
            })
            .SingleOrDefaultAsync();

        return tour?.KeyPoints;
    }

    private static IQueryable<Tour> Filter(IQueryable<Tour> tours, PublishedTourSearchDto search)
    {
        if (!string.IsNullOrWhiteSpace(search.Name))
        {
            var name = search.Name.ToLower();
            tours = tours.Where(tour => tour.Name.ToLower().Contains(name));
        }
        var difficulties = search.Difficulties ?? [];
        if (difficulties.Count > 0)
        {
            tours = tours.Where(tour => difficulties.Contains(tour.Difficulty));
        }
        var lowerTags = (search.Tags ?? [])
            .Where(tag => !string.IsNullOrWhiteSpace(tag))
            .Select(tag => tag.ToLower())
            .ToList();
        if (lowerTags.Count > 0)
        {
            tours = tours.Where(tour => tour.Tags.Any(tourTag => lowerTags.Contains(tourTag.ToLower())));
        }
        if (search.Transport is not null)
        {
            var transport = search.Transport.Value;
            tours = tours.Where(tour => tour.TransportTimes.Any(time => time.Transport == transport));
        }
        var lengthFrom = search.LengthFrom;
        var lengthTo = search.LengthTo;
        // Bounds entered in the wrong order are swapped, so 5 to 2 km means 2 to 5 km.
        if (lengthFrom > lengthTo)
        {
            (lengthFrom, lengthTo) = (lengthTo, lengthFrom);
        }
        if (lengthFrom is not null)
        {
            tours = tours.Where(tour => tour.LengthInKm >= lengthFrom);
        }
        if (lengthTo is not null)
        {
            tours = tours.Where(tour => tour.LengthInKm <= lengthTo);
        }
        return tours;
    }

    // Price sorting happens in TourBrowsingQueries, because prices live in Payment; here a price
    // sort leaves the tours newest first.
    private static IQueryable<Tour> Sort(IQueryable<Tour> tours, TourSort? sort)
    {
        // Difficulty is stored as text, so ordering by the column would be alphabetical
        // (Easy, Hard, Moderate). The rank below restores the order Easy, Moderate, Hard.
        return sort switch
        {
            TourSort.DifficultyAscending => tours
                .OrderBy(tour => tour.Difficulty == TourDifficulty.Easy ? 0 : tour.Difficulty == TourDifficulty.Moderate ? 1 : 2)
                .ThenByDescending(tour => tour.PublishedAt),
            TourSort.DifficultyDescending => tours
                .OrderByDescending(tour => tour.Difficulty == TourDifficulty.Easy ? 0 : tour.Difficulty == TourDifficulty.Moderate ? 1 : 2)
                .ThenByDescending(tour => tour.PublishedAt),
            _ => tours.OrderByDescending(tour => tour.PublishedAt)
        };
    }

    private static IQueryable<TourDto> ProjectToDtos(IQueryable<Tour> tours)
    {
        return tours
            .Select(tour => new TourDto(
                tour.Id,
                tour.AuthorId,
                tour.Name,
                tour.Description,
                tour.Difficulty,
                tour.Tags,
                tour.Status,
                tour.PublishedAt,
                tour.ArchivedAt,
                tour.RecommendedGroupSize.Min,
                tour.RecommendedGroupSize.Max,
                tour.TransportTimes
                    .Select(time => new TransportTimeDto(time.Transport, time.Minutes))
                    .ToList(),
                tour.LengthInKm));
    }
}
