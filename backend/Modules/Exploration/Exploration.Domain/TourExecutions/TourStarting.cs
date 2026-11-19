using Exploration.Domain.Tours;
using Shared.Domain.Exceptions;

namespace Exploration.Domain.TourExecutions;

// Starting a tour needs the tour, whether the tourist bought it, and the tourist's session in
// progress, so the rule lives here and not in one of the two aggregates.
public sealed class TourStarting
{
    // Returns the session the tourist is now in: the continued one, or a new one the caller adds.
    public TourExecution Start(Tour tour, Guid touristId, bool isPurchased, TourExecution? inProgress, Location location,
        DateTime now)
    {
        if (tour.Status != TourStatus.Published)
        {
            throw new DomainException("Pokrenuti se može samo objavljena tura.");
        }
        if (!isPurchased)
        {
            throw new DomainException("Možete da pokrenete samo turu koju ste kupili.");
        }

        if (inProgress is null)
        {
            return new TourExecution(tour.Id, touristId, location, now);
        }
        if (inProgress.TourId != tour.Id)
        {
            throw new DomainException("Već vam je u toku druga tura.");
        }

        inProgress.Continue(now);
        return inProgress;
    }
}
