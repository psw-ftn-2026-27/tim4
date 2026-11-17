using Exploration.Application.TourBrowsing;
using Shared.Domain;

namespace Exploration.Application.Tours;

public interface ITourReadRepository
{
    Task<List<TourDto>> GetByAuthorAsync(Guid authorId);
    Task<PageResult<TourDto>> GetPublishedAsync(PublishedTourSearchDto search, int page, int pageSize);

    // Every published tour meeting the search criteria other than price, without paging.
    Task<List<TourDto>> GetAllPublishedAsync(PublishedTourSearchDto search);
}
