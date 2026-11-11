import { Component, computed, signal } from '@angular/core';
import { LocationMap, MapCoordinates, MapLocation } from '../../shared/map/location-map';

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

// A platform page that tries out the map: city challenges, a tour and an empty list, then a
// form that picks a location on the map.
@Component({
  imports: [LocationMap],
  selector: 'app-map-demo',
  styleUrl: './map-demo.scss',
  templateUrl: './map-demo.html',
})
export class MapDemo {
  protected readonly locations = signal<MapLocation[]>(CITY_CHALLENGES);
  protected readonly showPath = signal(false);
  protected readonly pathLength = signal(0);

  protected showChallenges(): void {
    this.locations.set(CITY_CHALLENGES);
    this.showPath.set(false);
  }

  protected showTour(): void {
    this.locations.set(TOUR_KEY_POINTS);
    this.showPath.set(true);
  }

  protected showEmpty(): void {
    this.locations.set([]);
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
}
