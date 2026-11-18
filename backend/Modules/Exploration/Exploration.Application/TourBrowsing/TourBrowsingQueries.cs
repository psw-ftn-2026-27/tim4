using Exploration.Application.Tours;
using Payment.Contracts;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourBrowsing;

public sealed class TourBrowsingQueries
{
    private readonly ITourReadRepository _tourReadRepository;
    private readonly IPaymentApi _paymentApi;

    public TourBrowsingQueries(ITourReadRepository tourReadRepository, IPaymentApi paymentApi)
    {
        _tourReadRepository = tourReadRepository;
        _paymentApi = paymentApi;
    }

    public async Task<PageResult<PublishedTourDto>> GetPublishedAsync(PublishedTourSearchDto search, int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);

        // Prices live in Payment, so the database cannot filter or sort by them. Without a price
        // criterion the database pages the tours and only the shown tours get their prices.
        var usesPrice = search.PriceFrom is not null || search.PriceTo is not null
            || search.Sort is TourSort.PriceAscending or TourSort.PriceDescending;
        if (!usesPrice)
        {
            var tourPage = await _tourReadRepository.GetPublishedAsync(search, page, pageSize);
            var pageItems = await WithPricesAsync(tourPage.Items);
            return new PageResult<PublishedTourDto>(pageItems, tourPage.TotalCount);
        }

        // With a price criterion every tour meeting the other criteria is loaded, and the price
        // range, the price sort, and the paging are applied here, after the prices arrive.
        var tours = await WithPricesAsync(await _tourReadRepository.GetAllPublishedAsync(search));
        var matching = SortByPrice(WithinPriceRange(tours, search.PriceFrom, search.PriceTo), search.Sort);
        var items = matching
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return new PageResult<PublishedTourDto>(items, matching.Count);
    }

    private async Task<List<PublishedTourDto>> WithPricesAsync(List<TourDto> tours)
    {
        var prices = await _paymentApi.GetPricesByEntityIdsAsync(tours.Select(tour => tour.Id).ToList());
        return tours.Select(tour => new PublishedTourDto(tour, prices[tour.Id])).ToList();
    }

    private static List<PublishedTourDto> WithinPriceRange(List<PublishedTourDto> tours, long? from, long? to)
    {
        if (from is null && to is null)
        {
            return tours;
        }
        // Bounds entered in the wrong order are swapped, so 50 to 20 means 20 to 50.
        if (from > to)
        {
            (from, to) = (to, from);
        }
        // A tour without a price never falls within a range.
        return tours
            .Where(tour => tour.Price is not null)
            .Where(tour => from is null || tour.Price >= from)
            .Where(tour => to is null || tour.Price <= to)
            .ToList();
    }

    private static List<PublishedTourDto> SortByPrice(List<PublishedTourDto> tours, TourSort? sort)
    {
        // Tours without a price go last in both directions. The sort is stable, so tours with the
        // same price keep the newest-first order they came in.
        return sort switch
        {
            TourSort.PriceAscending => tours
                .OrderBy(tour => tour.Price is null)
                .ThenBy(tour => tour.Price)
                .ToList(),
            TourSort.PriceDescending => tours
                .OrderBy(tour => tour.Price is null)
                .ThenByDescending(tour => tour.Price)
                .ToList(),
            _ => tours
        };
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
