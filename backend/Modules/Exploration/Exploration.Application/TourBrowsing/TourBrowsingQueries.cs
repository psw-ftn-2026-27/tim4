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
            var pageItems = await WithPricesAndPinsAsync(tourPage.Items, pageLocations);
            return new PageResult<PublishedTourDto>(pageItems, tourPage.TotalCount);
        }

        // Otherwise every tour meeting the other criteria is loaded, and the circle, the price range,
        // the price sort, and the paging are applied here.
        var allTours = await _tourReadRepository.GetAllPublishedAsync(search);
        var locations = await _tourReadRepository.GetVisibleKeyPointLocationsAsync(allTours.Select(tour => tour.Id).ToList());
        var toursInCircle = usesCircle
            ? WithinCircle(allTours, locations, new LocationDto(search.Latitude!.Value, search.Longitude!.Value), search.RadiusInKm!.Value)
            : allTours;
        var tours = await WithPricesAndPinsAsync(toursInCircle, locations);
        var matching = SortByPrice(WithinPriceRange(tours, search.PriceFrom, search.PriceTo), search.Sort);
        var items = matching
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        return new PageResult<PublishedTourDto>(items, matching.Count);
    }

    // The pin is the first key point open without purchase, so the map never reveals a location
    // the visitor has yet to buy.
    private async Task<List<PublishedTourDto>> WithPricesAndPinsAsync(List<TourDto> tours, Dictionary<Guid, List<LocationDto>> locations)
    {
        var prices = await _paymentApi.GetPricesByEntityIdsAsync(tours.Select(tour => tour.Id).ToList());
        return tours
            .Select(tour => new PublishedTourDto(tour, prices[tour.Id], locations[tour.Id].FirstOrDefault()))
            .ToList();
    }

    // A tour is in the circle when at least one of its key points open without purchase is in it;
    // a tour without such key points never is.
    private static List<TourDto> WithinCircle(List<TourDto> tours, Dictionary<Guid, List<LocationDto>> locations,
        LocationDto center, double radiusInKm)
    {
        return tours
            .Where(tour => locations[tour.Id].Any(location => DistanceInKm(center, location) <= radiusInKm))
            .ToList();
    }

    // The straight-line distance over the Earth's surface (the haversine formula), the same one the
    // platform's shared map uses for a tour's length.
    private static double DistanceInKm(LocationDto from, LocationDto to)
    {
        const double earthRadiusInKm = 6371;
        var latitudeDelta = ToRadians(to.Latitude - from.Latitude);
        var longitudeDelta = ToRadians(to.Longitude - from.Longitude);
        var a = Math.Sin(latitudeDelta / 2) * Math.Sin(latitudeDelta / 2)
            + Math.Cos(ToRadians(from.Latitude)) * Math.Cos(ToRadians(to.Latitude))
            * Math.Sin(longitudeDelta / 2) * Math.Sin(longitudeDelta / 2);
        return 2 * earthRadiusInKm * Math.Asin(Math.Sqrt(a));
    }

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

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
