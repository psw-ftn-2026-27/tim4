using Games.Application.SocialChallenges;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengePreparation;

public sealed class SocialChallengePreparationQueries
{
    private readonly ISocialChallengeReadRepository _repository;

    public SocialChallengePreparationQueries(ISocialChallengeReadRepository repository)
    {
        _repository = repository;
    }

    public Task<PageResult<SocialChallengeDto>> GetAllAsync(int page, int pageSize)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Clamp(page, 1, int.MaxValue / pageSize);
        return _repository.GetAllAsync(page, pageSize);
    }

    public async Task<SocialChallengeDto> GetByIdAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Društveni izazov ne postoji.");
    }
}
