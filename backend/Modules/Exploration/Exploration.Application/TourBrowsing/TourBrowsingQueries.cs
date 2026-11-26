using Exploration.Application.TourExecutions;
using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Payment.Contracts;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourBrowsing;

public sealed class TourBrowsingQueries
{
    private readonly ITourReadRepository _tourReadRepository;
    private readonly ITourExecutionReadRepository _executionReadRepository;
    private readonly IPaymentApi _paymentApi;

    public TourBrowsingQueries(ITourReadRepository tourReadRepository,
        ITourExecutionReadRepository executionReadRepository, IPaymentApi paymentApi)
    {
        _tourReadRepository = tourReadRepository;
        _executionReadRepository = executionReadRepository;
        _paymentApi = paymentApi;
    }

    public async Task<PageResult<PublishedTourDto>> GetPublishedAsync(PublishedTourSearchDto search, Guid viewerId,
        int page, int pageSize)
    {
        page = Math.Max(page, 1);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var inProgressTourId = await _executionReadRepository.GetInProgressTourIdAsync(viewerId);

        // Prices live in Payment, and key points are stored as JSON, so the database cannot filter or
        // sort by price or filter by the circle. Without such a criterion the database pages the tours
        // and only the shown tours get their prices and pins.
        var usesCircle = search.Latitude is not null && search.Longitude is not null && search.RadiusInKm is not null;
        var usesPrice = search.PriceFrom is not null || search.PriceTo is not null
            || search.Sort is TourSort.PriceAscending or TourSort.PriceDescending;
        if (!usesPrice && !usesCircle)
        {
            var tourPage = await _tourReadRepository.GetPublishedAsync(search, page, pageSize);
            var pageLocations = await _tourReadRepository.GetVisibleKeyPointLocationsAsync(tourPage.Items.Select(tour => tour.Id).ToList());
            var pageItems = await WithPricesAndPinsAsync(tourPage.Items, pageLocations, inProgressTourId);
            return new PageResult<PublishedTourDto>(pageItems, tourPage.TotalCount);
        }

        // Otherwise every tour meeting the other criteria is loaded, and the circle, the price range,
        // the price sort, and the paging are applied here.
        var allTours = await _tourReadRepository.GetAllPublishedAsync(search);
        var locations = await _tourReadRepository.GetVisibleKeyPointLocationsAsync(allTours.Select(tour => tour.Id).ToList());
        var toursInCircle = usesCircle
            ? WithinCircle(allTours, locations, new Location(search.Latitude!.Value, search.Longitude!.Value), search.RadiusInKm!.Value)
            : allTours;
        var tours = await WithPricesAndPinsAsync(toursInCircle, locations, inProgressTourId);
        var matching = SortByPrice(WithinPriceRange(tours, search.PriceFrom, search.PriceTo), search.Sort);
        var items = matching
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return new PageResult<PublishedTourDto>(items, matching.Count);
    }

    // The pin is the first key point open without purchase, so the map never reveals a location
    // the visitor has yet to buy.
    private async Task<List<PublishedTourDto>> WithPricesAndPinsAsync(List<TourDto> tours,
        Dictionary<Guid, List<LocationDto>> locations, Guid? inProgressTourId)
    {
        var prices = await _paymentApi.GetPricesByEntityIdsAsync(tours.Select(tour => tour.Id).ToList());
        return tours
            .Select(tour => new PublishedTourDto(tour, prices[tour.Id], locations[tour.Id].FirstOrDefault(),
                tour.Id == inProgressTourId))
            .ToList();
    }

    // A tour is in the circle when at least one of its key points open without purchase is in it;
    // a tour without such key points never is.
    private static List<TourDto> WithinCircle(List<TourDto> tours, Dictionary<Guid, List<LocationDto>> locations,
        Location center, double radiusInKm)
    {
        return tours
            .Where(tour => locations[tour.Id]
                .Any(location => center.DistanceInKm(new Location(location.Latitude, location.Longitude)) <= radiusInKm))
            .ToList();
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

    // Payment returns every bought product, so IDs that are not tours of this module, and any repeats,
    // are dropped here. The pin is the first key point, because the tourist has bought the whole tour.
    public async Task<List<PurchasedTourDto>> GetPurchasedAsync(Guid touristId)
    {
        var purchasedIds = await _paymentApi.GetPurchasedEntityIdsAsync(touristId);
        var tours = await _tourReadRepository.GetPurchasedAsync(purchasedIds.Distinct().ToList());
        var locations = await _tourReadRepository.GetFirstKeyPointLocationsAsync(tours.Select(tour => tour.Id).ToList());
        var inProgressTourId = await _executionReadRepository.GetInProgressTourIdAsync(touristId);
        return tours
            .Select(tour => new PurchasedTourDto(tour, locations[tour.Id], tour.Id == inProgressTourId))
            .ToList();
    }

    // An archived tour is out of the offer, so only its buyers can still open it; for everyone else
    // it does not exist.
    public async Task<TourDetailsDto> GetPublishedDetailsAsync(Guid tourId, Guid viewerId)
    {
        var isPurchased = await _paymentApi.HasPurchasedAsync(viewerId, tourId);
        var tour = await _tourReadRepository.GetPublishedDetailsAsync(tourId, isPurchased)
            ?? throw new NotFoundException($"Tour {tourId} does not exist.");
        var inProgressTourId = await _executionReadRepository.GetInProgressTourIdAsync(viewerId);
        tour = tour with { IsPurchased = isPurchased, IsInProgress = tour.Id == inProgressTourId };

        // The author sees the whole tour, secrets included.
        if (tour.AuthorId == viewerId)
        {
            return tour;
        }

        // A buyer sees every key point, and everyone else only the key points open without
        // purchase. Neither sees a secret: a secret is revealed only when the tourist reaches
        // its key point while going through the tour.
        var visibleKeyPoints = tour.KeyPoints
            .Where(keyPoint => isPurchased || keyPoint.IsVisibleWithoutPurchase)
            .Select(keyPoint => keyPoint with { Secret = null })
            .ToList();
        return tour with { KeyPoints = visibleKeyPoints };
    }
}
