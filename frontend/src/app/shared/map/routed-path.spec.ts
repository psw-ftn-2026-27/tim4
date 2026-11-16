import type { MapLocation } from './location-map';
import { pathLengthKm } from './path-length';
import { RouteSegment, routedPath } from './routed-path';

const FIRST: MapLocation = { latitude: 0, longitude: 0, name: 'A' };
const SECOND: MapLocation = { latitude: 0, longitude: 1, name: 'B' };
const THIRD: MapLocation = { latitude: 0, longitude: 3, name: 'C' };

// A route along the roads of the given length, between the two locations.
function route(from: MapLocation, to: MapLocation, lengthKm: number): RouteSegment {
  return { points: [from, to], lengthKm };
}

describe('routedPath', () => {
  it('is the sum of the route lengths between consecutive locations, without returning from the last to the first', async () => {
    const asked: string[] = [];
    const fetchSegment = async (from: MapLocation, to: MapLocation) => {
      asked.push(`${from.name}-${to.name}`);
      return route(from, to, from === FIRST ? 150 : 250);
    };

    const path = await routedPath([FIRST, SECOND, THIRD], fetchSegment);

    expect(path.lengthKm).toBe(400);
    expect(asked).toEqual(['A-B', 'B-C']);
  });

  it('counts a segment without a route with its straight-line distance, and the others with their route lengths', async () => {
    const fetchSegment = async (from: MapLocation, to: MapLocation) =>
      from === FIRST ? null : route(from, to, 250);

    const path = await routedPath([FIRST, SECOND, THIRD], fetchSegment);

    expect(path.lengthKm).toBeCloseTo(pathLengthKm([FIRST, SECOND]) + 250, 6);
  });

  it('is 0 km for no locations and for a single location, without asking the routing service', async () => {
    const fetchSegment = vi.fn(async (from: MapLocation, to: MapLocation) => route(from, to, 1));

    expect((await routedPath([], fetchSegment)).lengthKm).toBe(0);
    expect((await routedPath([FIRST], fetchSegment)).lengthKm).toBe(0);
    expect(fetchSegment).not.toHaveBeenCalled();
  });
});
