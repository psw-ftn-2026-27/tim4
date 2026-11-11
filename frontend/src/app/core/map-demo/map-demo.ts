import { Component, signal } from '@angular/core';
import { LocationMap, MapLocation } from '../../shared/map/location-map';

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

// A platform page that tries out the map: city challenges, a tour and an empty list.
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
}
