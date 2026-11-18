using Exploration.Application.Tours;
using Exploration.Domain.Tours;
using Shared.Application;
using Shared.Domain.Exceptions;
using Shared.Domain.Images;

namespace Exploration.Application.KeyPointAuthoring;

public sealed class KeyPointAuthoringService
{
    private readonly ITourRepository _tourRepository;
    private readonly IImageStorage _imageStorage;
    private readonly IUnitOfWork _unitOfWork;

    public KeyPointAuthoringService(ITourRepository tourRepository, IImageStorage imageStorage, IUnitOfWork unitOfWork)
    {
        _tourRepository = tourRepository;
        _imageStorage = imageStorage;
        _unitOfWork = unitOfWork;
    }

    public async Task AddAsync(Guid tourId, Guid authorId, KeyPointDetailsDto dto, byte[]? image)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);
        var imageFile = new ImageFile(image ?? []);
        var imageAddress = await _imageStorage.SaveAsync(imageFile);

        tour.AddKeyPoint(CreateKeyPoint(dto, imageAddress), dto.TourLengthInKm);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateAsync(Guid tourId, Guid keyPointId, Guid authorId, KeyPointDetailsDto dto, byte[]? image)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);
        var oldImageAddress = tour.GetKeyPoint(keyPointId).ImageAddress;
        var imageAddress = oldImageAddress;
        if (image is not null)
        {
            imageAddress = await _imageStorage.SaveAsync(new ImageFile(image));
        }

        tour.UpdateKeyPoint(keyPointId, CreateKeyPoint(dto, imageAddress), dto.TourLengthInKm);
        await _unitOfWork.SaveChangesAsync();

        if (imageAddress != oldImageAddress)
        {
            await _imageStorage.DeleteAsync(oldImageAddress);
        }
    }

    public async Task RemoveAsync(Guid tourId, Guid keyPointId, Guid authorId, double tourLengthInKm)
    {
        var tour = await GetOwnedTourAsync(tourId, authorId);
        var imageAddress = tour.GetKeyPoint(keyPointId).ImageAddress;

        tour.RemoveKeyPoint(keyPointId, tourLengthInKm);
        await _unitOfWork.SaveChangesAsync();

        await _imageStorage.DeleteAsync(imageAddress);
    }

    private static KeyPoint CreateKeyPoint(KeyPointDetailsDto dto, string imageAddress) =>
        new(dto.Name, dto.Description, imageAddress, dto.Secret, new Location(dto.Latitude, dto.Longitude), dto.IsVisibleWithoutPurchase);

    private async Task<Tour> GetOwnedTourAsync(Guid tourId, Guid authorId)
    {
        var tour = await _tourRepository.GetByIdAsync(tourId);
        if (tour is null || tour.AuthorId != authorId)
        {
            throw new NotFoundException($"Tour {tourId} does not exist.");
        }
        return tour;
    }
}
