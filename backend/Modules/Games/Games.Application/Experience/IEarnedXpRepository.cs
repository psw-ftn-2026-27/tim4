using Games.Domain.Experience;

namespace Games.Application.Experience;

public interface IEarnedXpRepository
{
    void Add(EarnedXp earnedXp);
}
