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
    private readonly TourWalking _tourWalking;
    private readonly IUnitOfWork _unitOfWork;

    public TourExecutionsService(ITourExecutionRepository executionRepository, ITourRepository tourRepository,
        IPaymentApi paymentApi, TourStarting tourStarting, TourWalking tourWalking, IUnitOfWork unitOfWork)
    {
        _executionRepository = executionRepository;
        _tourRepository = tourRepository;
        _paymentApi = paymentApi;
        _tourStarting = tourStarting;
        _tourWalking = tourWalking;
        _unitOfWork = unitOfWork;
    }

    public async Task StartAsync(Guid touristId, StartTourDto dto)
    {
        var tour = await _tourRepository.GetByIdAsync(dto.TourId)
            ?? throw new NotFoundException($"Tour {dto.TourId} does not exist.");
        var isPurchased = await _paymentApi.HasPurchasedAsync(touristId, dto.TourId);
        var inProgress = await _executionRepository.GetInProgressByTouristAsync(touristId);
        var location = new Location(dto.Latitude, dto.Longitude);

        var execution = _tourStarting.Start(tour, touristId, isPurchased, inProgress, location, DateTime.UtcNow);
        // A continued session is already stored; only a new one is added.
        if (execution != inProgress)
        {
            _executionRepository.Add(execution);
        }
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<PositionCheckResultDto> CheckPositionAsync(Guid touristId, CheckPositionDto dto)
    {
        var execution = await _executionRepository.GetInProgressByTouristAsync(touristId)
            ?? throw new NotFoundException("Nemate turu u toku.");
        var tour = await _tourRepository.GetByIdAsync(execution.TourId)
            ?? throw new NotFoundException($"Tour {execution.TourId} does not exist.");
        var position = new Location(dto.Latitude, dto.Longitude);

        var outcome = _tourWalking.CheckPosition(tour, execution, position, DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
        return new PositionCheckResultDto(outcome);
    }

    public async Task AbandonAsync(Guid touristId)
    {
        var execution = await _executionRepository.GetInProgressByTouristAsync(touristId)
            ?? throw new NotFoundException("Nemate turu u toku.");

        execution.Abandon(DateTime.UtcNow);
        await _unitOfWork.SaveChangesAsync();
    }
}
