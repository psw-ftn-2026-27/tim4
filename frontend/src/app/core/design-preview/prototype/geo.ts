import { ProtoPoint } from './proto-map/proto-map';

// Straight-line distance between two points on the Earth, in metres.
export function metersBetween(a: ProtoPoint, b: ProtoPoint): number {
  const radius = 6371000;
  const toRad = (deg: number): number => (deg * Math.PI) / 180;
  const dLat = toRad(b.latitude - a.latitude);
  const dLon = toRad(b.longitude - a.longitude);
  const h =
    Math.sin(dLat / 2) ** 2 +
    Math.cos(toRad(a.latitude)) * Math.cos(toRad(b.latitude)) * Math.sin(dLon / 2) ** 2;
  return 2 * radius * Math.asin(Math.sqrt(h));
}

// "350 m" or "1,6 km", as the screens show a distance.
export function distanceText(meters: number): string {
  return meters >= 1000
    ? `${(meters / 1000).toFixed(1).replace('.', ',')} km`
    : `${Math.round(meters / 10) * 10} m`;
}
