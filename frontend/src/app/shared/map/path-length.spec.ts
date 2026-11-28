import type { MapLocation } from './location-map';
import { pathLengthKm } from './path-length';

const PARIS: MapLocation = { latitude: 48.8566, longitude: 2.3522, name: 'Pariz' };
const LONDON: MapLocation = { latitude: 51.5074, longitude: -0.1278, name: 'London' };

describe('pathLengthKm', () => {
  it('equals the known distance in kilometres between two locations', () => {
    // The great-circle distance between the centres of Paris and London is about 343.56 km.
    expect(pathLengthKm([PARIS, LONDON])).toBeCloseTo(343.56, 1);
  });

  it('is the sum of both segments for three locations, without returning from the last to the first', () => {
    const first: MapLocation = { latitude: 0, longitude: 0, name: 'A' };
    const second: MapLocation = { latitude: 0, longitude: 1, name: 'B' };
    const third: MapLocation = { latitude: 0, longitude: 3, name: 'C' };

    const segments = pathLengthKm([first, second]) + pathLengthKm([second, third]);

    expect(pathLengthKm([first, second, third])).toBeCloseTo(segments, 6);
  });

  it('is 0 km for no locations and for a single location', () => {
    expect(pathLengthKm([])).toBe(0);
    expect(pathLengthKm([PARIS])).toBe(0);
  });
});
