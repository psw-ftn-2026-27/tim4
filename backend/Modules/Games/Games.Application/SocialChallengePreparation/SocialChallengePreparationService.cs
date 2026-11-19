using Games.Application.SocialChallenges;
using Games.Domain.SocialChallenges;
using Shared.Domain.Exceptions;

namespace Games.Application.SocialChallengePreparation;

public sealed class SocialChallengePreparationService
{
    private readonly ISocialChallengeRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SocialChallengePreparationService(ISocialChallengeRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Guid> CreateAsync(SocialChallengeDetailsDto details)
    {
        var activityTime = new ActivityTime(details.Start, details.End);
        var challenge = new SocialChallenge(details.Name, activityTime, details.RequiredParticipants,
            details.RangeInMeters, details.XpReward, details.CompletionMessage, details.Latitude, details.Longitude);
        _repository.Add(challenge);
        await _unitOfWork.SaveChangesAsync();
        return challenge.Id;
    }

    public async Task UpdateAsync(Guid id, SocialChallengeDetailsDto details)
    {
        var challenge = await GetChallengeAsync(id);
        var activityTime = new ActivityTime(details.Start, details.End);
        challenge.UpdateDetails(details.Name, activityTime, details.RequiredParticipants,
            details.RangeInMeters, details.XpReward, details.CompletionMessage, details.Latitude, details.Longitude);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task DeleteAsync(Guid id)
    {
        var challenge = await GetChallengeAsync(id);
        challenge.EnsureCanBeDeleted();
        _repository.Remove(challenge);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task PublishAsync(Guid id)
    {
        var challenge = await GetChallengeAsync(id);
        challenge.Publish();
        await _unitOfWork.SaveChangesAsync();
    }
    public async Task RetireAsync(Guid id)
    {
        var challenge = await GetChallengeAsync(id);
        challenge.Retire();
        await _unitOfWork.SaveChangesAsync();
    }
    public async Task ArchiveAsync(Guid id)
    {
        var challenge = await GetChallengeAsync(id);
        challenge.Archive();
        await _unitOfWork.SaveChangesAsync();
    }

    private async Task<SocialChallenge> GetChallengeAsync(Guid id)
    {
        return await _repository.GetByIdAsync(id)
            ?? throw new NotFoundException("Društveni izazov ne postoji.");
    }
}
