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
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import * as L from 'leaflet';
import { RouteSegment, routedPath } from './routed-path';

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
const POSITION_RADIUS = 8;
const POSITION_PANE = 'position';

// The public OSRM server of the OpenStreetMap community, walking profile: free and without a key.
const ROUTING_URL = 'https://routing.openstreetmap.de/routed-foot/route/v1/foot';
// How long one segment waits for the routing service before it is drawn as a straight line.
const ROUTING_TIMEOUT_MS = 5000;

// The part of the routing service's answer the map reads. Coordinates come as [longitude, latitude].
interface RoutingResponse {
  code: string;
  routes?: { distance: number; geometry: { coordinates: [number, number][] } }[];
}

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
// effect() and DestroyRef drive Leaflet, which lives outside the component tree, and HttpClient
// asks the routing service for the path. Pages use only the inputs and the outputs.
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
  // The tourist's position, drawn as its own marker. The page owns it, like selected.
  readonly position = input<MapCoordinates | null>(null);

  // The length of the path along the roads, in kilometres, sent once the routes of all
  // segments have arrived after the locations change.
  readonly pathLengthChange = output<number>();
  // The coordinates of every click on the map, sent only while the map is selectable.
  readonly selectedChange = output<MapCoordinates>();
  // The position in locations of the pin the user clicked.
  readonly locationClick = output<number>();

  private readonly host = inject(ElementRef<HTMLElement>);
  private readonly http = inject(HttpClient);

  // The lines of the path along the roads; null while their routes are being fetched.
  private readonly route = signal<MapCoordinates[][] | null>(null);

  // Counts the route requests, so the answer to an older list of locations is thrown away.
  private routeRequest = 0;

  // The list of locations the current routes belong to; null when there are none.
  private routedLocations: MapLocation[] | null = null;

  // Set once the map's element exists, so the effect below waits for it.
  private readonly map = signal<L.Map | undefined>(undefined);

  // Holds the pins, so one call clears them before they are drawn again.
  private readonly layers = L.layerGroup();

  // Holds the line apart from the pins, so routes that arrive later never refit the view.
  private readonly pathLayer = L.layerGroup();

  // Holds the selected pin apart from the other pins, so moving it never refits the view.
  private readonly selectedLayer = L.layerGroup();

  // Holds the tourist's position apart from the other pins, so moving it never refits the view.
  private readonly positionLayer = L.layerGroup();

  // The accent colour from _tokens.scss, read once the map's element exists.
  private selectedColor = '';

  // The colours of the position marker from _tokens.scss: a dark dot with a light border.
  private positionColor = '';
  private positionBorderColor = '';

  constructor() {
    afterNextRender(() => {
      const element = this.host.nativeElement.querySelector('.map') as HTMLElement;
      const map = L.map(element);
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
        maxZoom: 19,
      }).addTo(map);
      this.layers.addTo(map);
      this.pathLayer.addTo(map);
      // The selected pin gets its own pane above the other pins, so a pin on the same place
      // never hides it (Leaflet's pin pane has z-index 600).
      map.createPane(SELECTED_PANE).style.zIndex = '650';
      this.selectedLayer.addTo(map);
      // The position gets its own pane above the selected pin, so neither hides it.
      map.createPane(POSITION_PANE).style.zIndex = '660';
      this.positionLayer.addTo(map);
      map.on('click', (event: L.LeafletMouseEvent) => {
        if (this.selectable()) {
          this.selectedChange.emit({ latitude: event.latlng.lat, longitude: event.latlng.lng });
        }
      });
      const styles = getComputedStyle(element);
      this.selectedColor = styles.getPropertyValue('--color-accent').trim();
      this.positionColor = styles.getPropertyValue('--color-text').trim();
      this.positionBorderColor = styles.getPropertyValue('--color-surface').trim();
      this.map.set(map);
    });

    // Leaflet is outside the component tree, so an effect redraws the pins and fits the view
    // when the locations change.
    effect(() => {
      const map = this.map();
      if (map === undefined) {
        return;
      }
      this.draw(map, this.locations());
    });

    // The line follows showPath and the routes, without touching the pins or the view.
    effect(() => {
      if (this.map() === undefined) {
        return;
      }
      this.drawPath(this.showPath(), this.route());
    });

    // Routes are asked for only while the path is shown, and only once per list of locations,
    // so toggling showPath back on reuses the routes it already has.
    effect(() => {
      const locations = this.locations();
      if (locations === this.routedLocations) {
        return;
      }
      if (this.showPath()) {
        this.routedLocations = locations;
        void this.fetchRoute(locations);
      } else {
        // New locations while the path is hidden: drop the old routes and any answer on its way.
        this.routeRequest++;
        this.routedLocations = null;
        this.route.set(null);
      }
    });

    // The selected pin follows the selected input: a click only emits, and the page sets it.
    effect(() => {
      if (this.map() === undefined) {
        return;
      }
      this.drawSelected(this.selected());
    });

    // The position marker follows the position input, the same way.
    effect(() => {
      if (this.map() === undefined) {
        return;
      }
      this.drawPosition(this.position());
    });

    inject(DestroyRef).onDestroy(() => {
      // Any answer still on its way belongs to a map that no longer exists.
      this.routeRequest++;
      this.map()?.remove();
    });
  }

  // Hides the line, asks for the route of every segment, and then draws it and sends its length.
  private async fetchRoute(locations: MapLocation[]): Promise<void> {
    const request = ++this.routeRequest;
    this.route.set(null);
    const path = await routedPath(locations, (from, to) => this.fetchSegment(from, to));
    if (request !== this.routeRequest) {
      return;
    }
    this.route.set(path.segments);
    this.pathLengthChange.emit(path.lengthKm);
  }

  // The walking route between two locations, or null when the service finds none.
  private async fetchSegment(from: MapLocation, to: MapLocation): Promise<RouteSegment | null> {
    const url =
      `${ROUTING_URL}/${from.longitude},${from.latitude};${to.longitude},${to.latitude}` +
      '?overview=full&geometries=geojson';
    // A service that never answers would leave the length "računa se…" forever, so after the
    // timeout the request fails and the segment becomes a straight line.
    const response = await firstValueFrom(
      this.http.get<RoutingResponse>(url, { timeout: ROUTING_TIMEOUT_MS }),
    );
    const route = response.routes?.[0];
    if (response.code !== 'Ok' || route === undefined) {
      return null;
    }
    return {
      points: route.geometry.coordinates.map(([longitude, latitude]) => ({ latitude, longitude })),
      lengthKm: route.distance / 1000,
    };
  }

  private draw(map: L.Map, locations: MapLocation[]): void {
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

    if (points.length === 0) {
      map.setView(DEFAULT_CENTER, DEFAULT_ZOOM);
    } else if (points.length === 1) {
      map.setView(points[0], SINGLE_LOCATION_ZOOM);
    } else {
      map.fitBounds(L.latLngBounds(points), { padding: [32, 32] });
    }
  }

  // One line per segment, so a segment without a route stays a straight line on its own.
  private drawPath(showPath: boolean, route: MapCoordinates[][] | null): void {
    this.pathLayer.clearLayers();
    if (!showPath || route === null) {
      return;
    }
    route.forEach((segment) => {
      L.polyline(segment.map((point): L.LatLngTuple => [point.latitude, point.longitude])).addTo(
        this.pathLayer,
      );
    });
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

  // A small dark dot with a light border, so the position looks different from the pins and
  // from the green selected circle.
  private drawPosition(position: MapCoordinates | null): void {
    this.positionLayer.clearLayers();
    if (position === null) {
      return;
    }
    L.circleMarker([position.latitude, position.longitude], {
      pane: POSITION_PANE,
      // A click on the marker goes through to the map, so it still moves the position.
      interactive: false,
      radius: POSITION_RADIUS,
      weight: 3,
      color: this.positionBorderColor,
      fillColor: this.positionColor,
      fillOpacity: 1,
    }).addTo(this.positionLayer);
  }
}
