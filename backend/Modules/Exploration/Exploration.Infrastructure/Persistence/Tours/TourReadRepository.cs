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
        var ordered = Sort(published, search.SortByDifficulty);

        var totalCount = await published.CountAsync();
        var items = await ProjectToDtos(ordered)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PageResult<TourDto>(items, totalCount);
    }

    private static IQueryable<Tour> Filter(IQueryable<Tour> tours, PublishedTourSearchDto search)
    {
        if (!string.IsNullOrWhiteSpace(search.Name))
        {
            var name = search.Name.ToLower();
            tours = tours.Where(tour => tour.Name.ToLower().Contains(name));
        }
        if (search.Difficulty is not null)
        {
            var difficulty = search.Difficulty.Value;
            tours = tours.Where(tour => tour.Difficulty == difficulty);
        }
        foreach (var tag in search.Tags ?? [])
        {
            if (!string.IsNullOrWhiteSpace(tag))
            {
                tours = tours.Where(tour => tour.Tags.Contains(tag));
            }
        }
        if (search.Transport is not null)
        {
            var transport = search.Transport.Value;
            tours = tours.Where(tour => tour.TransportTimes.Any(time => time.Transport == transport));
        }
        return tours;
    }

    private static IQueryable<Tour> Sort(IQueryable<Tour> tours, DifficultySort? sortByDifficulty)
    {
        // Difficulty is stored as text, so ordering by the column would be alphabetical
        // (Easy, Hard, Moderate). The rank below restores the order Easy, Moderate, Hard.
        return sortByDifficulty switch
        {
            DifficultySort.Ascending => tours
                .OrderBy(tour => tour.Difficulty == TourDifficulty.Easy ? 0 : tour.Difficulty == TourDifficulty.Moderate ? 1 : 2)
                .ThenByDescending(tour => tour.PublishedAt),
            DifficultySort.Descending => tours
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
                tour.TransportTimes
                    .Select(time => new TransportTimeDto(time.Transport, time.Minutes))
                    .ToList()));
    }
}
