namespace Exploration.Application.TourExecutions;

// A key point the session has reached. Only a reached key point carries its secret to the client.
public sealed record ReachedKeyPointDto(Guid Id, string Name, string? Secret);
