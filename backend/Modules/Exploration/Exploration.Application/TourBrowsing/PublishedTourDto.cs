using Exploration.Application.Tours;

namespace Exploration.Application.TourBrowsing;

// Price is null when the tour has no price in Payment yet; zero means the tour is free.
// PinLocation is the location of the first key point open without purchase, null when the tour has none.
public sealed record PublishedTourDto(TourDto Tour, long? Price, LocationDto? PinLocation);
