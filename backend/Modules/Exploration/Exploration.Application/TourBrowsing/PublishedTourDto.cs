using Exploration.Application.Tours;

namespace Exploration.Application.TourBrowsing;

// Price is null when the tour has no price in Payment yet; zero means the tour is free.
// PinLocation is the location of the first key point open without purchase, null when the tour has none.
// IsInProgress is true only when the tour is the viewer's tour in progress.
// IsPurchased is true only when the viewer has bought the tour.
public sealed record PublishedTourDto(TourDto Tour, long? Price, LocationDto? PinLocation, bool IsInProgress,
    bool IsPurchased);
