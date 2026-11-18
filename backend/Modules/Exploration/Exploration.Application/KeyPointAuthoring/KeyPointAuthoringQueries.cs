using Exploration.Application.Tours;
using Shared.Domain.Exceptions;

namespace Exploration.Application.KeyPointAuthoring;

public sealed class KeyPointAuthoringQueries
{
    private readonly ITourReadRepository _tourReadRepository;

    public KeyPointAuthoringQueries(ITourReadRepository tourReadRepository)
    {
        _tourReadRepository = tourReadRepository;
    }

    public async Task<List<KeyPointDto>> GetByTourAsync(Guid tourId, Guid authorId)
    {
        return await _tourReadRepository.GetKeyPointsOfAuthoredTourAsync(tourId, authorId)
            ?? throw new NotFoundException($"Tour {tourId} does not exist.");
    }
}
