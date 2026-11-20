using Exploration.Domain.Tours;

namespace Exploration.Domain.TourExecutions;

// Checking the tourist's position needs the tour's key points and the session, so the rule lives
// here and not in one of the two aggregates.
public sealed class TourWalking
{
    private const double ProximityInMeters = 50;

    public PositionCheckOutcome CheckPosition(Tour tour, TourExecution execution, Location position, DateTime now)
    {
        execution.RecordActivity(now);

        var keyPointIds = tour.KeyPoints.Select(keyPoint => keyPoint.Id).ToList();
        var nextKeyPoint = tour.GetKeyPoint(execution.NextKeyPointId(keyPointIds));
        if (nextKeyPoint.Location.DistanceInKm(position) * 1000 > ProximityInMeters)
        {
            return PositionCheckOutcome.NotNear;
        }

        var isLast = nextKeyPoint.Id == keyPointIds[^1];
        execution.ReachKeyPoint(nextKeyPoint.Id, isLast, now);
        return isLast ? PositionCheckOutcome.CompletedTour : PositionCheckOutcome.ReachedKeyPoint;
    }
}
