using Exploration.Application.Tours;
using Exploration.Domain.TourExecutions;
using Exploration.Domain.Tours;
using Payment.Contracts;
using Shared.Domain.Exceptions;

namespace Exploration.Application.TourExecutions;

public sealed class TourExecutionsService
{
    private readonly ITourExecutionRepository _executionRepository;
    private readonly ITourRepository _tourRepository;
    private readonly IPaymentApi _paymentApi;
    private readonly TourStarting _tourStarting;
    private readonly IUnitOfWork _unitOfWork;

    public TourExecutionsService(ITourExecutionRepository executionRepository, ITourRepository tourRepository,
        IPaymentApi paymentApi, TourStarting tourStarting, IUnitOfWork unitOfWork)
    {
        _executionRepository = executionRepository;
        _tourRepository = tourRepository;
        _paymentApi = paymentApi;
        _tourStarting = tourStarting;
        _unitOfWork = unitOfWork;
    }

    public async Task StartAsync(Guid touristId, StartTourDto dto)
    {
        var tour = await _tourRepository.GetByIdAsync(dto.TourId)
            ?? throw new NotFoundException($"Tour {dto.TourId} does not exist.");
        var isPurchased = await _paymentApi.HasPurchasedAsync(touristId, dto.TourId);
        var inProgress = await _executionRepository.GetInProgressByTouristAsync(touristId);
        // The tour of the session in progress is loaded only so a rejection can name it.
        var inProgressTour = inProgress is null ? null : await _tourRepository.GetByIdAsync(inProgress.TourId);
        var location = new Location(dto.Latitude, dto.Longitude);

        var execution = _tourStarting.Start(tour, touristId, isPurchased, inProgress, inProgressTour, location, DateTime.UtcNow);
        // A continued session is already stored; only a new one is added.
        if (execution != inProgress)
        {
            _executionRepository.Add(execution);
        }
        await _unitOfWork.SaveChangesAsync();
    }
}
