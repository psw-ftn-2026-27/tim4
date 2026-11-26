using Exploration.Application.Tours;

namespace Exploration.Application.TourBrowsing;

// PinLocation is the location of the tour's first key point, shown to the buyer even when that key point
// is hidden until purchase.
// IsInProgress is true only when the tour is the tourist's tour in progress.
public sealed record PurchasedTourDto(TourDto Tour, LocationDto PinLocation, bool IsInProgress);
