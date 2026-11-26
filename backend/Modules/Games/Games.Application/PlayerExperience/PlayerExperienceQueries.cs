using Games.Application.Experience;

namespace Games.Application.PlayerExperience;

public sealed class PlayerExperienceQueries
{
    private readonly IEarnedXpReadRepository _repository;

    public PlayerExperienceQueries(IEarnedXpReadRepository repository)
    {
        _repository = repository;
    }

    public Task<TotalXpDto> GetTotalXpAsync(Guid userId)
    {
        return _repository.GetTotalXpAsync(userId);
    }
}
