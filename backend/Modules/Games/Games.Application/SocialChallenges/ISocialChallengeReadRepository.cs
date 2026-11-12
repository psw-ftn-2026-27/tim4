using Games.Application.SocialChallengePreparation;
using Shared.Domain;

namespace Games.Application.SocialChallenges;

public interface ISocialChallengeReadRepository
{
    Task<PageResult<SocialChallengeDto>> GetAllAsync(int page, int pageSize);
    Task<SocialChallengeDto?> GetByIdAsync(Guid id);
}
