using Exploration.Domain.Tours;
using Shared.Domain;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourExecutions;

// One session of a tourist going through a tour. It is started by TourStarting, which knows the
// rules that need the tour and the tourist's other sessions.
public sealed class TourExecution : AggregateRoot
{
    public Guid TourId { get; private set; }
    public Guid TouristId { get; private set; }
    public TourExecutionStatus Status { get; private set; }
    public DateTime StartedAt { get; private set; }
    public Location StartLocation { get; private set; }
    public DateTime LastActivityAt { get; private set; }

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

    // The first key point of the tour, in tour order, that this session has not reached yet.
    // Reaching key points comes with a later feature, so for now it is always the first one.
    public Guid NextKeyPointId(IReadOnlyList<Guid> keyPointIds)
    {
        if (keyPointIds.Count == 0)
        {
            throw new DomainException("Tura nema ključnih tačaka.");
        }

        return keyPointIds[0];
    }
}
