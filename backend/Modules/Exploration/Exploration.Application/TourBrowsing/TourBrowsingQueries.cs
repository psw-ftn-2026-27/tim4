using Exploration.Application.Tours;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourBrowsing;

public sealed class TourBrowsingQueries
{
    private readonly ITourReadRepository _tourReadRepository;

    public TourBrowsingQueries(ITourReadRepository tourReadRepository)
    {
        _tourReadRepository = tourReadRepository;
    }

    public Task<PageResult<TourDto>> GetPublishedAsync(PublishedTourSearchDto search, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        return _tourReadRepository.GetPublishedAsync(search, page, pageSize);
    }

    public async Task<TourDetailsDto> GetPublishedDetailsAsync(Guid tourId, Guid viewerId)
    {
        var tour = await _tourReadRepository.GetPublishedDetailsAsync(tourId)
            ?? throw new NotFoundException($"Tour {tourId} does not exist.");

        // The author sees the whole tour. Once Payment exposes purchases, a buyer will too.
        if (tour.AuthorId == viewerId)
        {
            return tour;
        }

        // Everyone else sees only the key points open without purchase, and no secret.
        var openKeyPoints = tour.KeyPoints
            .Where(keyPoint => keyPoint.IsVisibleWithoutPurchase)
            .Select(keyPoint => keyPoint with { Secret = null })
            .ToList();
        return tour with { KeyPoints = openKeyPoints };
    }
}
