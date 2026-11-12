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

// A point the user picked on the map: latitude and longitude in decimal degrees.
export interface MapCoordinates {
  latitude: number;
  longitude: number;
}

// The view shown when there are no locations: the centre of Novi Sad.
const DEFAULT_CENTER: L.LatLngTuple = [45.2551, 19.8452];
const DEFAULT_ZOOM = 13;
const SINGLE_LOCATION_ZOOM = 15;
const SELECTED_PIN_RADIUS = 14;
const SELECTED_PANE = 'selected';

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

  // When true, a click on the map sends its coordinates through selectedChange.
  readonly selectable = input(false);
  // The selected pin. The page owns it: it sets this input from what selectedChange sends.
  readonly selected = input<MapCoordinates | null>(null);

  // The length of the path through the locations, in kilometres, sent whenever the inputs change.
  readonly pathLengthChange = output<number>();
  // The coordinates of every click on the map, sent only while the map is selectable.
  readonly selectedChange = output<MapCoordinates>();
  // The position in locations of the pin the user clicked.
  readonly locationClick = output<number>();

  private readonly host = inject(ElementRef<HTMLElement>);

  // Set once the map's element exists, so the effect below waits for it.
  private readonly map = signal<L.Map | undefined>(undefined);

  // Holds the pins and the line, so one call clears them before they are drawn again.
  private readonly layers = L.layerGroup();

  // Holds the selected pin apart from the other pins, so moving it never refits the view.
  private readonly selectedLayer = L.layerGroup();

  // The accent colour from _tokens.scss, read once the map's element exists.
  private selectedColor = '';

  constructor() {
    afterNextRender(() => {
      const element = this.host.nativeElement.querySelector('.map') as HTMLElement;
      const map = L.map(element);
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
        maxZoom: 19,
      }).addTo(map);
      this.layers.addTo(map);
      // The selected pin gets its own pane above the other pins, so a pin on the same place
      // never hides it (Leaflet's pin pane has z-index 600).
      map.createPane(SELECTED_PANE).style.zIndex = '650';
      this.selectedLayer.addTo(map);
      map.on('click', (event: L.LeafletMouseEvent) => {
        if (this.selectable()) {
          this.selectedChange.emit({ latitude: event.latlng.lat, longitude: event.latlng.lng });
        }
      });
      this.selectedColor = getComputedStyle(element).getPropertyValue('--color-accent').trim();
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

    // The selected pin follows the selected input: a click only emits, and the page sets it.
    effect(() => {
      if (this.map() === undefined) {
        return;
      }
      this.drawSelected(this.selected());
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
      // Leaflet writes a string popup through innerHTML, and users type these names, so the
      // name goes in as text: a name like <img onerror=...> must never run as code.
      const label = document.createElement('span');
      label.textContent = location.name;
      L.marker(points[index], { icon: PIN_ICON })
        .bindPopup(label)
        .on('click', () => this.locationClick.emit(index))
        .addTo(this.layers);
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

  // A circle instead of a pin, so the selected place looks different from the other locations.
  private drawSelected(selected: MapCoordinates | null): void {
    this.selectedLayer.clearLayers();
    if (selected === null) {
      return;
    }
    L.circleMarker([selected.latitude, selected.longitude], {
      pane: SELECTED_PANE,
      // A click on the circle goes through to the map, so it still moves the selection.
      interactive: false,
      radius: SELECTED_PIN_RADIUS,
      weight: 4,
      color: this.selectedColor,
      fillColor: this.selectedColor,
      fillOpacity: 0.8,
    }).addTo(this.selectedLayer);
  }
}
