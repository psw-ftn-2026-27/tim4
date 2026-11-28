import type { MapCoordinates, MapLocation } from './location-map';
import { pathLengthKm } from './path-length';

// The route between two consecutive locations: the line that follows the roads and its length.
export interface RouteSegment {
  points: MapCoordinates[];
  lengthKm: number;
}

// The whole path: one line per pair of consecutive locations, and the sum of their lengths.
export interface RoutedPath {
  segments: MapCoordinates[][];
  lengthKm: number;
}

// Asks for the route of every pair of consecutive locations, in the given order, and joins
// them into one path. The last location is not joined back to the first. A segment for which
// fetchSegment returns null or fails is a straight line, counted with its straight-line
// distance; the other segments keep their routes. Fewer than two locations give 0 km, and
// fetchSegment is never called.
export async function routedPath(
  locations: MapLocation[],
  fetchSegment: (from: MapLocation, to: MapLocation) => Promise<RouteSegment | null>,
): Promise<RoutedPath> {
  const pairs = locations.slice(1).map((to, index) => [locations[index], to] as const);

  const segments = await Promise.all(
    pairs.map(async ([from, to]) => {
      try {
        return (await fetchSegment(from, to)) ?? straightSegment(from, to);
      } catch {
        return straightSegment(from, to);
      }
    }),
  );

  return {
    segments: segments.map((segment) => segment.points),
    lengthKm: segments.reduce((total, segment) => total + segment.lengthKm, 0),
  };
}

function straightSegment(from: MapLocation, to: MapLocation): RouteSegment {
  return {
    points: [
      { latitude: from.latitude, longitude: from.longitude },
      { latitude: to.latitude, longitude: to.longitude },
    ],
    lengthKm: pathLengthKm([from, to]),
  };
}
