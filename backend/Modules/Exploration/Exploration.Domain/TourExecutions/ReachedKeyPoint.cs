namespace Exploration.Domain.TourExecutions;

// A key point the session has reached, with the time it was reached.
public sealed record ReachedKeyPoint
{
    public Guid KeyPointId { get; private init; }
    public DateTime ReachedAt { get; private init; }

    private ReachedKeyPoint() { }

    public ReachedKeyPoint(Guid keyPointId, DateTime reachedAt)
    {
        KeyPointId = keyPointId;
        ReachedAt = reachedAt;
    }
}
