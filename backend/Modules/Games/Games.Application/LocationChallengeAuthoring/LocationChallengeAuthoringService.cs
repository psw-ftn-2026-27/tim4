using Games.Application.LocationChallenges;
using Games.Domain.LocationChallenges;
using Shared.Application;
using Shared.Domain.Images;

namespace Games.Application.LocationChallengeAuthoring;

public sealed class LocationChallengeAuthoringService
{
    private readonly ILocationChallengeRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IImageStorage _imageStorage;

    public LocationChallengeAuthoringService(
        ILocationChallengeRepository repository, IUnitOfWork unitOfWork, IImageStorage imageStorage)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _imageStorage = imageStorage;
    }

    public async Task<Guid> CreateAsync(LocationChallengeContentDto content)
    {
        var start = new ChallengeLocation(content.StartLatitude, content.StartLongitude);
        var uploads = (content.Points ?? []).Select(point => (
            Location: new ChallengeLocation(point.Latitude, point.Longitude),
            Image: new ImageFile(point.Image)
        )).ToList();

        var points = new List<HiddenLocation>();
        foreach (var upload in uploads)
        {
            var address = await _imageStorage.SaveAsync(upload.Image);
            points.Add(new HiddenLocation(upload.Location, address));
        }

        var challenge = new LocationChallenge(content.Name, start, content.XpReward, points);
        _repository.Add(challenge);
        await _unitOfWork.SaveChangesAsync();
        return challenge.Id;
    }
}
