import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CoordinatesDto } from '../../../shared/api/coordinates-dto';
import { LocationMap, MapCoordinates, MapLocation } from '../../../shared/map/location-map';
import { PositionSimulation } from '../../../shared/position/position-simulation';
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

// The living example of the shared map (shared/map): city challenges, a tour and an empty
// list, then picking a location, adding a key point and moving an existing one. The last
// section, the position simulator, is the one part of the style guide that calls the server.
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
}
