using Exploration.Domain.TourExecutions;

namespace Exploration.Application.TourExecutions;

// Only the outcome of a position check; the page reads the rest through the queries.
public sealed record PositionCheckResultDto(PositionCheckOutcome Outcome);
