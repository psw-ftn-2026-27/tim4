using Exploration.Application.KeyPointAuthoring;
using Exploration.Application.TourBrowsing;
using Shared.Domain;

namespace Exploration.Application.Tours;

public interface ITourReadRepository
{
    Task<List<TourDto>> GetByAuthorAsync(Guid authorId);
    Task<PageResult<TourDto>> GetPublishedAsync(PublishedTourSearchDto search, int page, int pageSize);

    // Every published tour meeting the search criteria other than price (the length range included),
    // without paging.
    Task<List<TourDto>> GetAllPublishedAsync(PublishedTourSearchDto search);

    // Null when the tour does not exist or is not published. Holds every key point with its secret.
    Task<TourDetailsDto?> GetPublishedDetailsAsync(Guid tourId);

    // Null when the tour does not exist or belongs to another author.
    Task<List<KeyPointDto>?> GetKeyPointsOfAuthoredTourAsync(Guid tourId, Guid authorId);
}
