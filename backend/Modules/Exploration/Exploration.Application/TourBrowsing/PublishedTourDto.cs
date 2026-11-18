using Exploration.Application.Tours;

namespace Exploration.Application.TourBrowsing;

// Price is null when the tour has no price in Payment yet; zero means the tour is free.
public sealed record PublishedTourDto(TourDto Tour, long? Price);
