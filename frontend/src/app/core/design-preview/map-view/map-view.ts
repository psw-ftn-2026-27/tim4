import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CoordinatesDto } from '../../../shared/api/coordinates-dto';
import { LocationMap, MapArea, MapCoordinates, MapLocation } from '../../../shared/map/location-map';
import { PositionSimulation } from '../../../shared/position/position-simulation';
import { repeatEvery } from '../../../shared/position/repeat-every';
import { serverMessage } from '../../../shared/util/server-message';
import { Auth } from '../../auth/auth';

// Challenges spread around Novi Sad, shown without a path.
const CITY_CHALLENGES: MapLocation[] = [
  { latitude: 45.2551, longitude: 19.8452, name: 'Kviz na Trgu slobode' },
  { latitude: 45.2517, longitude: 19.8618, name: 'Skrivena lokacija na Petrovaradinskoj tvrđavi' },
  { latitude: 45.2552, longitude: 19.8511, name: 'Kviz u Dunavskom parku' },
  { latitude: 45.2465, longitude: 19.8512, name: 'Skrivena lokacija na Štrandu' },
];

// The key points of a walking tour, in the order the tourist visits them.
const TOUR_KEY_POINTS: MapLocation[] = [
  { latitude: 45.2655, longitude: 19.8293, name: '1. Železnička stanica' },
  { latitude: 45.2575, longitude: 19.834, name: '2. Bulevar oslobođenja' },
  { latitude: 45.2551, longitude: 19.8452, name: '3. Trg slobode' },
  { latitude: 45.2552, longitude: 19.8511, name: '4. Dunavski park' },
  { latitude: 45.2517, longitude: 19.8618, name: '5. Petrovaradinska tvrđava' },
];

// A search area of 500 m around Trg slobode, as in E23.
const SEARCH_AREA: MapArea = { latitude: 45.2551, longitude: 19.8452, radiusMeters: 500 };

// A smaller area around the hidden location on the fortress, as in G21.
const HIDDEN_LOCATION_AREA: MapArea = { latitude: 45.2517, longitude: 19.8618, radiusMeters: 100 };

// The living example of the shared map (shared/map): city challenges, a tour and an empty
// list, then picking a location, adding a key point and moving an existing one. The position
// simulator, the route to the next key point and the periodic check are the parts of the
// style guide that call the server; the last section draws an area as a circle.
@Component({
  imports: [LocationMap, RouterLink],
  selector: 'app-map-view',
  styleUrl: './map-view.scss',
  templateUrl: './map-view.html',
})
export class MapView {
  protected readonly locations = signal<MapLocation[]>(CITY_CHALLENGES);
  protected readonly showPath = signal(false);
  // null while the map is fetching the routes, until it sends the length along the roads.
  protected readonly pathLength = signal<number | null>(null);
  protected readonly pathLengthText = computed(() => {
    const length = this.pathLength();
    return length === null ? 'računa se…' : `${length.toFixed(2)} km`;
  });

  protected showChallenges(): void {
    this.setLocations(CITY_CHALLENGES);
    this.showPath.set(false);
  }

  protected showTour(): void {
    this.setLocations(TOUR_KEY_POINTS);
    this.showPath.set(true);
  }

  protected showEmpty(): void {
    this.setLocations([]);
  }

  // The map sends a new length only when the locations really change, so only then is the
  // length cleared until it arrives.
  private setLocations(locations: MapLocation[]): void {
    if (locations !== this.locations()) {
      this.locations.set(locations);
      this.pathLength.set(null);
    }
  }

  protected togglePath(): void {
    this.showPath.update((shown) => !shown);
  }

  // The selecting map shows the tour's key points as other pins.
  // "Dodaj tačku" appends to it, so it starts as a copy of the tour's key points.
  protected readonly keyPoints = signal<MapLocation[]>([...TOUR_KEY_POINTS]);

  // The picked place; null until the user clicks the map.
  protected readonly selected = signal<MapCoordinates | null>(null);

  // The position of the key point the user clicked last, and of the one being moved.
  protected readonly lastClicked = signal<number | null>(null);
  protected readonly editing = signal<number | null>(null);

  // A position can be 0, so the template reads the name instead of the position.
  protected readonly editingName = computed(() => {
    const index = this.editing();
    return index === null ? null : this.keyPoints()[index].name;
  });

  // Adds the picked place as the next key point, joined to the others, and clears the pick.
  protected addPoint(): void {
    const picked = this.selected();
    if (picked === null) {
      return;
    }
    this.keyPoints.update((points) => [
      ...points,
      { ...picked, name: `${points.length + 1}. Nova tačka` },
    ]);
    this.selected.set(null);
  }

  // Starts moving the key point clicked last: the green circle marks its saved place.
  protected startEdit(): void {
    const index = this.lastClicked();
    if (index === null) {
      return;
    }
    const point = this.keyPoints()[index];
    this.editing.set(index);
    this.selected.set({ latitude: point.latitude, longitude: point.longitude });
  }

  // While a key point is being moved, the picked place replaces its location.
  protected pick(coordinates: MapCoordinates): void {
    const index = this.editing();
    if (index === null) {
      this.selected.set(coordinates);
      return;
    }
    this.keyPoints.update((points) =>
      points.map((point, i) => (i === index ? { ...point, ...coordinates } : point)),
    );
    this.editing.set(null);
    this.selected.set(null);
  }

  // The position simulator: only a tourist sets a position, so the request is made only then.
  protected readonly auth = inject(Auth);
  private readonly positionSimulation = inject(PositionSimulation);
  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );

  // The server answers 204 No Content when the tourist has no position, so the value is null.
  protected readonly position = httpResource<CoordinatesDto | null>(() =>
    this.isExplorer() ? '/api/positions/mine' : undefined,
  );
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  // A click on the map saves the position; the marker moves only once the resource reloads.
  protected async setPosition(coordinates: MapCoordinates): Promise<void> {
    if (this.pending()) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.positionSimulation.setPosition(coordinates);
      this.position.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Poziciju nije moguće sačuvati.'));
    } finally {
      this.pending.set(false);
    }
  }

  // The route to the next key point. The position comes from the same resource as the
  // simulator's, so it is always the one saved on the server.
  protected readonly tourKeyPoints = TOUR_KEY_POINTS;

  // The position in the tour of the key point picked as the next one; null until a pin is clicked.
  protected readonly nextIndex = signal<number | null>(null);
  protected readonly nextPoint = computed(() => {
    const index = this.nextIndex();
    return index === null ? null : TOUR_KEY_POINTS[index];
  });

  // A failed save in this section shows under its own map, not under the simulator's.
  protected readonly routeError = signal<string | null>(null);

  // A click on the empty map saves the position, like in the simulator.
  protected async setRoutePosition(coordinates: MapCoordinates): Promise<void> {
    if (this.pending()) {
      return;
    }
    this.pending.set(true);
    this.routeError.set(null);
    try {
      await this.positionSimulation.setPosition(coordinates);
      this.position.reload();
    } catch (failure) {
      this.routeError.set(serverMessage(failure, 'Poziciju nije moguće sačuvati.'));
    } finally {
      this.pending.set(false);
    }
  }

  // The periodic check, the model for E22 and G22. On every tick a module's page sends the last
  // saved position with its own request; this section only writes out what would be sent.
  private readonly tickCount = signal(0);
  protected readonly tickText = signal('');
  protected readonly checkStopped = signal(false);

  private checkPosition(): void {
    this.tickCount.update((count) => count + 1);
    const tick = this.tickCount();
    // Not loaded yet, failed or not set: the tick sends nothing.
    const position = this.position.hasValue() ? this.position.value() : null;
    if (!position) {
      this.tickText.set(`Otkucaj ${tick}: preskočen — pozicija nije postavljena`);
      return;
    }
    this.tickText.set(
      `Otkucaj ${tick}: poslata bi bila pozicija ` +
        `${position.latitude.toFixed(5)}, ${position.longitude.toFixed(5)}`,
    );
  }

  // Declared after the position resource, because the first tick runs right away.
  private readonly check = repeatEvery(10_000, () => this.checkPosition());

  // In a module, the server's answer that ends the check (tour finished, challenge solved)
  // stops it the same way.
  protected stopCheck(): void {
    this.check.stop();
    this.checkStopped.set(true);
  }

  // The area on the map: a search area as in E23 and a smaller one around a hidden location as
  // in G21. In a module the radius comes from the page's form or from the server.
  protected readonly area = signal<MapArea | null>(null);

  protected showSearchArea(): void {
    this.area.set(SEARCH_AREA);
  }

  protected showHiddenLocationArea(): void {
    this.area.set(HIDDEN_LOCATION_AREA);
  }
}
