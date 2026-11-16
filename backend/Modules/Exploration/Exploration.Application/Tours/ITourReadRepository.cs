using Exploration.Application.KeyPointAuthoring;
using Exploration.Application.TourBrowsing;
using Shared.Domain;

namespace Exploration.Application.Tours;

public interface ITourReadRepository
{
    Task<List<TourDto>> GetByAuthorAsync(Guid authorId);
    Task<PageResult<TourDto>> GetPublishedAsync(PublishedTourSearchDto search, int page, int pageSize);

    // Null when the tour does not exist or belongs to another author.
    Task<List<KeyPointDto>?> GetKeyPointsOfAuthoredTourAsync(Guid tourId, Guid authorId);
}
