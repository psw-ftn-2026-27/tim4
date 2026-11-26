using Exploration.Application.KeyPointAuthoring;
using Exploration.Application.TourBrowsing;
using Shared.Domain;

namespace Exploration.Application.Tours;

public interface ITourReadRepository
{
    Task<List<TourDto>> GetByAuthorAsync(Guid authorId);
    Task<PageResult<TourDto>> GetPublishedAsync(PublishedTourSearchDto search, int page, int pageSize);

    // Every published tour meeting the search criteria other than price and the circle (the length
    // range included), without paging.
    Task<List<TourDto>> GetAllPublishedAsync(PublishedTourSearchDto search);

    // For each of the given tours, the locations of its key points open without purchase, in tour order.
    Task<Dictionary<Guid, List<LocationDto>>> GetVisibleKeyPointLocationsAsync(List<Guid> tourIds);

    // The published and archived tours among the given IDs, each once, ordered by name; other IDs are skipped.
    Task<List<TourDto>> GetPurchasedAsync(List<Guid> tourIds);

    // For each of the given tours, the location of its first key point, whether or not it is open without purchase.
    Task<Dictionary<Guid, LocationDto>> GetFirstKeyPointLocationsAsync(List<Guid> tourIds);

    // Null when the tour does not exist or is neither published nor, when includeArchived is true, archived.
    // Holds every key point with its secret.
    Task<TourDetailsDto?> GetPublishedDetailsAsync(Guid tourId, bool includeArchived);

    // Null when the tour does not exist or belongs to another author.
    Task<List<KeyPointDto>?> GetKeyPointsOfAuthoredTourAsync(Guid tourId, Guid authorId);
}
