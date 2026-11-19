using Exploration.Domain.Tours;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourExecutions;

// One session of a tourist going through a tour. It is started by TourStarting, which knows the
// rules that need the tour and the tourist's other sessions, and walked by TourWalking, which
// knows how near the tourist is to the tour's key points.
public sealed class TourExecution : AggregateRoot
{
    private readonly List<ReachedKeyPoint> _reachedKeyPoints = [];

    public Guid TourId { get; private set; }
    public Guid TouristId { get; private set; }
    public TourExecutionStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public Location StartLocation { get; private set; }
    public DateTime LastActivityAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    // In the order they were reached.
    public IReadOnlyList<ReachedKeyPoint> ReachedKeyPoints => _reachedKeyPoints;
    public int ReachedKeyPointCount => _reachedKeyPoints.Count;

    private TourExecution()
    {
        StartLocation = null!;
    }

    public TourExecution(Guid tourId, Guid touristId, Location startLocation, DateTime now)
        : base(Guid.NewGuid())
    {
        if (startLocation is null)
        {
            throw new DomainException("Turu možete da pokrenete tek kada postavite svoju poziciju.");
        }

        TourId = tourId;
        TouristId = touristId;
        Status = TourExecutionStatus.InProgress;
        StartedAt = now;
        StartLocation = startLocation;
        LastActivityAt = now;
    }

    public void Continue(DateTime now)
    {
        if (Status != TourExecutionStatus.InProgress)
        {
            throw new DomainException("Nastaviti se može samo tura koja je u toku.");
        }

        LastActivityAt = now;
    }

    public void RecordActivity(DateTime now)
    {
        EnsureInProgress();

        LastActivityAt = now;
    }

    // Reaching the last key point of the tour completes the session.
    public void ReachKeyPoint(Guid keyPointId, bool isLast, DateTime now)
    {
        EnsureInProgress();
        if (HasReached(keyPointId))
        {
            throw new DomainException("Ova ključna tačka je već dostignuta.");
        }

        _reachedKeyPoints.Add(new ReachedKeyPoint(keyPointId, now));
        if (isLast)
        {
            Status = TourExecutionStatus.Completed;
            CompletedAt = now;
        }
    }

    public bool HasReached(Guid keyPointId) =>
        _reachedKeyPoints.Any(reached => reached.KeyPointId == keyPointId);

    // The first key point of the tour, in tour order, that this session has not reached yet.
    public Guid NextKeyPointId(IReadOnlyList<Guid> keyPointIds)
    {
        if (keyPointIds.Count == 0)
        {
            throw new DomainException("Tura nema ključnih tačaka.");
        }

        foreach (var keyPointId in keyPointIds)
        {
            if (!HasReached(keyPointId))
            {
                return keyPointId;
            }
        }

        throw new DomainException("Dostigli ste sve ključne tačke ture.");
    }

    private void EnsureInProgress()
    {
        if (Status != TourExecutionStatus.InProgress)
        {
            throw new DomainException("Ova tura više nije u toku.");
        }
    }
}
