import {
  Component,
  DestroyRef,
  ElementRef,
  afterNextRender,
  effect,
  inject,
  input,
  output,
  signal,
} from '@angular/core';
import * as L from 'leaflet';
import { pathLengthKm } from './path-length';

export interface MapLocation {
  latitude: number;
  longitude: number;
  name: string;
}

// The view shown when there are no locations: the centre of Novi Sad.
const DEFAULT_CENTER: L.LatLngTuple = [45.2551, 19.8452];
const DEFAULT_ZOOM = 13;
const SINGLE_LOCATION_ZOOM = 15;

// Leaflet's default pin finds its images through its own CSS, which breaks once the app is
// bundled. angular.json copies the images to "leaflet/", and every pin uses this icon.
const PIN_ICON = L.icon({
  iconUrl: 'leaflet/marker-icon.png',
  iconRetinaUrl: 'leaflet/marker-icon-2x.png',
  shadowUrl: 'leaflet/marker-shadow.png',
  iconSize: [25, 41],
  iconAnchor: [12, 41],
  popupAnchor: [1, -34],
  shadowSize: [41, 41],
});

// The one exception to the Angular subset (see README.md): ElementRef, afterNextRender,
// effect() and DestroyRef drive Leaflet, which lives outside the component tree. Pages use
// only the inputs and the output.
@Component({
  selector: 'app-location-map',
  styleUrl: './location-map.scss',
  templateUrl: './location-map.html',
})
export class LocationMap {
  readonly locations = input.required<MapLocation[]>();
  readonly showPath = input(false);

  // The length of the path through the locations, in kilometres, sent whenever the inputs change.
  readonly pathLengthChange = output<number>();

  private readonly host = inject(ElementRef<HTMLElement>);

  // Set once the map's element exists, so the effect below waits for it.
  private readonly map = signal<L.Map | undefined>(undefined);

  // Holds the pins and the line, so one call clears them before they are drawn again.
  private readonly layers = L.layerGroup();

  constructor() {
    afterNextRender(() => {
      const element = this.host.nativeElement.querySelector('.map') as HTMLElement;
      const map = L.map(element);
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
        maxZoom: 19,
      }).addTo(map);
      this.layers.addTo(map);
      this.map.set(map);
    });

    // Leaflet is outside the component tree, so an effect redraws it when an input changes.
    effect(() => {
      const map = this.map();
      if (map === undefined) {
        return;
      }
      this.draw(map, this.locations(), this.showPath());
      this.pathLengthChange.emit(pathLengthKm(this.locations()));
    });

    inject(DestroyRef).onDestroy(() => this.map()?.remove());
  }

  private draw(map: L.Map, locations: MapLocation[], showPath: boolean): void {
    this.layers.clearLayers();

    const points = locations.map((location): L.LatLngTuple => [
      location.latitude,
      location.longitude,
    ]);

    locations.forEach((location, index) => {
      L.marker(points[index], { icon: PIN_ICON }).bindPopup(location.name).addTo(this.layers);
    });

    if (showPath && points.length > 1) {
      L.polyline(points).addTo(this.layers);
    }

    if (points.length === 0) {
      map.setView(DEFAULT_CENTER, DEFAULT_ZOOM);
    } else if (points.length === 1) {
      map.setView(points[0], SINGLE_LOCATION_ZOOM);
    } else {
      map.fitBounds(L.latLngBounds(points), { padding: [32, 32] });
    }
  }
}
