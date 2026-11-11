import type { MapLocation } from './location-map';

const EARTH_RADIUS_KM = 6371;

// The length of the path through the locations in the given order, in kilometres: the sum of
// the straight-line (great-circle) distances between consecutive locations. The last location
// is not joined back to the first. Fewer than two locations give 0 km.
export function pathLengthKm(locations: MapLocation[]): number {
  let total = 0;
  for (let i = 1; i < locations.length; i++) {
    total += distanceKm(locations[i - 1], locations[i]);
  }
  return total;
}

// The haversine formula: the distance between two points on a sphere the size of the Earth.
function distanceKm(from: MapLocation, to: MapLocation): number {
  const latitudeDelta = toRadians(to.latitude - from.latitude);
  const longitudeDelta = toRadians(to.longitude - from.longitude);
  const a =
    Math.sin(latitudeDelta / 2) ** 2 +
    Math.cos(toRadians(from.latitude)) *
      Math.cos(toRadians(to.latitude)) *
      Math.sin(longitudeDelta / 2) ** 2;
  return 2 * EARTH_RADIUS_KM * Math.asin(Math.sqrt(a));
}

function toRadians(degrees: number): number {
  return (degrees * Math.PI) / 180;
}
