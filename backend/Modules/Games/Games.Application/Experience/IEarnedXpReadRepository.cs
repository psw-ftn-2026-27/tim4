using Games.Application.PlayerExperience;

namespace Games.Application.Experience;

public interface IEarnedXpReadRepository
{
    Task<TotalXpDto> GetTotalXpAsync(Guid userId);
}
