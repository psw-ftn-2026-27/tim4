import {
  Component,
  DestroyRef,
  ElementRef,
  afterNextRender,
  computed,
  effect,
  inject,
  input,
  output,
  signal,
  untracked,
} from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import * as L from 'leaflet';
import { RouteSegment, routedPath } from './routed-path';

// The colour of a pin, named after the role it plays; each tone is a background and text token
// pair from _tokens.scss.
export type PinTone = 'default' | 'owned' | 'quiz' | 'social' | 'location' | 'muted' | 'next';

export interface MapLocation {
  latitude: number;
  longitude: number;
  name: string;
  // The short text inside the pin, such as a price ("40 EC") or a number ("2"). Without it the pin
  // is a small dot and the name shows only when it is clicked.
  label?: string;
  tone?: PinTone;
}

// A point the user picked on the map: latitude and longitude in decimal degrees.
export interface MapCoordinates {
  latitude: number;
  longitude: number;
}

// A circle on the map: its centre in decimal degrees and its radius in metres.
export interface MapArea {
  latitude: number;
  longitude: number;
  radiusMeters: number;
}

// The part of the map the user sees, in decimal degrees; a page filters "in this area" with it.
export interface MapBounds {
  north: number;
  south: number;
  east: number;
  west: number;
}

// The pin HTML is built by Leaflet outside the component's styles, so it refers to the tokens
// by name, inline.
const TONES: Record<PinTone, { bg: string; text: string }> = {
  default: { bg: '--color-surface', text: '--color-text' },
  owned: { bg: '--color-success-bg', text: '--color-success-text' },
  quiz: { bg: '--color-xp-bg', text: '--color-xp-text' },
  social: { bg: '--color-ec-bg', text: '--color-ec-text' },
  location: { bg: '--color-state-draft-bg', text: '--color-state-draft-text' },
  muted: { bg: '--color-muted-bg', text: '--color-muted-text' },
  next: { bg: '--color-accent', text: '--color-accent-text' },
};

// The view shown when there are no locations: the centre of Novi Sad.
const DEFAULT_CENTER: L.LatLngTuple = [45.2551, 19.8452];
const DEFAULT_ZOOM = 13;
const SINGLE_LOCATION_ZOOM = 15;
// The zoom the map flies to for one pin, unless it is already closer.
const FOCUS_ZOOM = 16;
const SELECTED_PIN_RADIUS = 14;
const SELECTED_PANE = 'selected';
// Pins closer than this on the screen, in pixels, are drawn as one group with a count.
const CLUSTER_PIXELS = 56;

// The public OSRM server of the OpenStreetMap community, walking profile: free and without a key.
const ROUTING_URL = 'https://routing.openstreetmap.de/routed-foot/route/v1/foot';
// How long one segment waits for the routing service before it is drawn as a straight line.
const ROUTING_TIMEOUT_MS = 5000;

// The part of the routing service's answer the map reads. Coordinates come as [longitude, latitude].
interface RoutingResponse {
  code: string;
  routes?: { distance: number; geometry: { coordinates: [number, number][] } }[];
}

// The one exception to the Angular subset (see README.md): ElementRef, afterNextRender,
// effect() and DestroyRef drive Leaflet, which lives outside the component tree, and HttpClient
// asks the routing service for the path. Pages use only the inputs and the outputs.
@Component({
  selector: 'app-location-map',
  styleUrl: './location-map.scss',
  templateUrl: './location-map.html',
  host: { '[class.fill]': 'fill()' },
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
  // The next point the tourist walks to. While both position and target are set, the map draws
  // the walking route from the position to it and sends its length through targetDistanceChange.
  readonly target = input<MapCoordinates | null>(null);
  // A circle given by its centre and its radius in metres. The page owns it; a click never moves it.
  readonly area = input<MapArea | null>(null);

  // The position in locations of the pin drawn dark and above the others: the card under the
  // mouse in the list beside the map, or the chosen one.
  readonly activeIndex = input<number | null>(null);
  // When it changes to a pin the user cannot see (out of view or inside a group), the map flies to it.
  readonly focusIndex = input<number | null>(null);
  // Without it, the view is fitted to all pins whenever the locations change. With it, only when
  // the key changes: a page that filters by the visible area keeps the key, so the view stays put.
  readonly fitKey = input<string | null>(null);
  // Pins that overlap on the screen join into one round group with a count; a click zooms in.
  // Off by default, because a tour's key points must each stay visible.
  readonly cluster = input(false);
  // The map fills the height of its parent instead of its usual 400px; the page sets that height.
  readonly fill = input(false);

  // The length of the path along the roads, in kilometres, sent once the routes of all
  // segments have arrived after the locations change.
  readonly pathLengthChange = output<number>();
  // The coordinates of every click on the map, sent only while the map is selectable.
  readonly selectedChange = output<MapCoordinates>();
  // The position in locations of the pin the user clicked.
  readonly locationClick = output<number>();
  // The position in locations of the pin under the mouse, and null when the mouse leaves it.
  readonly locationHover = output<number | null>();
  // The visible area, sent after the user (not the page) moved or zoomed the map.
  readonly viewChange = output<MapBounds>();
  // The length of the walking route from the position to the target, in metres.
  readonly targetDistanceChange = output<number>();

  protected readonly hasPosition = computed(() => this.position() !== null);

  private readonly host = inject(ElementRef<HTMLElement>);
  private readonly http = inject(HttpClient);

  // The lines of the path along the roads; null while their routes are being fetched.
  private readonly route = signal<MapCoordinates[][] | null>(null);

  // Counts the route requests, so the answer to an older list of locations is thrown away.
  private routeRequest = 0;

  // The list of locations the current routes belong to; null when there are none.
  private routedLocations: MapLocation[] | null = null;

  // The line of the route from the position to the target; null while it is being fetched.
  private readonly targetRoute = signal<MapCoordinates[] | null>(null);

  // Counts the requests for that route, so the answer for an older position or target is thrown away.
  private targetRouteRequest = 0;

  // Set once the map's element exists, so the effects below wait for it.
  private readonly map = signal<L.Map | undefined>(undefined);

  // Changes on every zoom, so the groups of pins are made again for the new scale.
  private readonly zoom = signal(0);

  // What the view was last fitted to: a list of locations, or a fitKey.
  private fitted: MapLocation[] | string | null = null;

  // True while the map moves because the code asked it to; such moves are not sent as viewChange.
  private movingByCode = false;
  private movingByCodeTimer: ReturnType<typeof setTimeout> | undefined;
  private resizeObserver: ResizeObserver | null = null;

  // One layer group per kind of drawing, so redrawing one never touches the others or the view.
  private readonly layers = L.layerGroup();
  private readonly pathLayer = L.layerGroup();
  private readonly selectedLayer = L.layerGroup();
  private readonly positionLayer = L.layerGroup();
  private readonly targetRouteLayer = L.layerGroup();
  private readonly areaLayer = L.layerGroup();

  constructor() {
    afterNextRender(() => {
      const element = this.host.nativeElement.querySelector('.map') as HTMLElement;
      // No inertia: the map stops where the mouse lets go, and the new view is known at once.
      const map = L.map(element, { zoomControl: false, inertia: false });
      L.control.zoom({ position: 'bottomright' }).addTo(map);
      // A softer base map: the tiles are partly transparent over the app's background colour.
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
        maxZoom: 19,
        opacity: 0.62,
      }).addTo(map);
      this.areaLayer.addTo(map);
      this.pathLayer.addTo(map);
      this.targetRouteLayer.addTo(map);
      this.layers.addTo(map);
      // The selected pin gets its own pane above the other pins, so a pin on the same place
      // never hides it (Leaflet's pin pane has z-index 600).
      map.createPane(SELECTED_PANE).style.zIndex = '650';
      this.selectedLayer.addTo(map);
      this.positionLayer.addTo(map);
      map.on('click', (event: L.LeafletMouseEvent) => {
        if (this.selectable()) {
          this.selectedChange.emit({ latitude: event.latlng.lat, longitude: event.latlng.lng });
        }
      });
      map.on('zoomend', () => this.zoom.update((zoom) => zoom + 1));
      map.on('moveend', () => {
        if (this.movingByCode) {
          return;
        }
        const bounds = map.getBounds();
        this.viewChange.emit({
          north: bounds.getNorth(),
          south: bounds.getSouth(),
          east: bounds.getEast(),
          west: bounds.getWest(),
        });
      });
      // A map in a column whose width changes must be told, or it keeps grey tiles.
      this.resizeObserver = new ResizeObserver(() => {
        this.byCode();
        map.invalidateSize();
      });
      this.resizeObserver.observe(element);
      this.map.set(map);
    });

    // Fit the view first: the groups of pins depend on the scale it sets.
    effect(() => {
      const map = this.map();
      const key = this.fitKey();
      const locations = this.locations();
      if (map === undefined) {
        return;
      }
      const fitTo = key ?? locations;
      if (fitTo !== this.fitted) {
        this.fitted = fitTo;
        untracked(() => this.fitAll(map, false));
      }
    });

    // Leaflet is outside the component tree, so an effect redraws the pins when the locations,
    // the active pin or the scale change.
    effect(() => {
      const map = this.map();
      this.zoom();
      if (map !== undefined) {
        this.drawPins(map, this.locations(), this.activeIndex(), this.cluster());
      }
    });

    // Fly to the focused pin when the user cannot see it.
    effect(() => {
      const map = this.map();
      const index = this.focusIndex();
      if (map === undefined || index === null) {
        return;
      }
      const location = untracked(() => this.locations())[index];
      if (location === undefined) {
        return;
      }
      const point = L.latLng(location.latitude, location.longitude);
      const inView = map.getBounds().pad(-0.1).contains(point);
      if (!inView || untracked(() => this.isGrouped(map, index))) {
        this.byCode();
        map.flyTo(point, Math.max(map.getZoom(), FOCUS_ZOOM), { duration: 0.6 });
      }
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

    // The route to the target is asked for again whenever the position or the target changes,
    // and never while either is missing.
    effect(() => {
      const position = this.position();
      const target = this.target();
      if (position === null || target === null) {
        // Drop the old route and any answer on its way.
        this.targetRouteRequest++;
        this.targetRoute.set(null);
        return;
      }
      void this.fetchTargetRoute(position, target);
    });

    // The route to the target follows its signal, without touching the pins or the view.
    effect(() => {
      if (this.map() === undefined) {
        return;
      }
      this.drawTargetRoute(this.targetRoute());
    });

    // The circle follows the area input, the same way as the position marker.
    effect(() => {
      if (this.map() === undefined) {
        return;
      }
      this.drawArea(this.area());
    });

    // While a click picks a place, the cursor says so.
    effect(() => {
      const map = this.map();
      if (map !== undefined) {
        map.getContainer().style.cursor = this.selectable() ? 'crosshair' : '';
      }
    });

    inject(DestroyRef).onDestroy(() => {
      // Any answer still on its way belongs to a map that no longer exists.
      this.routeRequest++;
      this.targetRouteRequest++;
      clearTimeout(this.movingByCodeTimer);
      this.resizeObserver?.disconnect();
      this.map()?.remove();
    });
  }

  // ---- The two buttons on the map ----

  protected toPosition(): void {
    const map = this.map();
    const position = this.position();
    if (map !== undefined && position !== null) {
      this.byCode();
      map.flyTo([position.latitude, position.longitude], Math.max(map.getZoom(), FOCUS_ZOOM), {
        duration: 0.6,
      });
    }
  }

  protected showAll(): void {
    const map = this.map();
    if (map !== undefined) {
      this.fitAll(map, true);
    }
  }

  // Marks the next moves as made by the code, not the user. A move to where the map already is
  // fires no moveend, so the mark clears itself after the animation instead of waiting for one.
  private byCode(): void {
    this.movingByCode = true;
    clearTimeout(this.movingByCodeTimer);
    this.movingByCodeTimer = setTimeout(() => (this.movingByCode = false), 900);
  }

  // Every pin, and the position when there is one, in view.
  private fitAll(map: L.Map, animate: boolean): void {
    const position = this.position();
    const points = [...this.locations(), ...(position === null ? [] : [position])].map(
      (point): L.LatLngTuple => [point.latitude, point.longitude],
    );
    this.byCode();
    if (points.length === 0) {
      map.setView(DEFAULT_CENTER, DEFAULT_ZOOM, { animate });
    } else if (points.length === 1) {
      map.setView(points[0], SINGLE_LOCATION_ZOOM, { animate });
    } else {
      map.fitBounds(L.latLngBounds(points), { padding: [48, 48], animate });
    }
  }

  // ---- Routes along the roads ----

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

  // Asks for the walking route from the position to the target. It is one segment of the same
  // kind as the tour's path, so it falls back to a straight line the same way.
  private async fetchTargetRoute(position: MapCoordinates, target: MapCoordinates): Promise<void> {
    const request = ++this.targetRouteRequest;
    // routedPath takes locations, and the two ends of this route have no names.
    const path = await routedPath(
      [
        { ...position, name: '' },
        { ...target, name: '' },
      ],
      (from, to) => this.fetchSegment(from, to),
    );
    if (request !== this.targetRouteRequest) {
      return;
    }
    this.targetRoute.set(path.segments[0]);
    this.targetDistanceChange.emit(Math.round(path.lengthKm * 1000));
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

  // ---- Pins ----

  // Greedy grouping by distance on the screen; the active pin always stays on its own.
  private groups(map: L.Map, locations: MapLocation[], active: number | null): number[][] {
    const groups: { center: L.Point; indexes: number[] }[] = [];
    locations.forEach((location, index) => {
      const point = map.latLngToLayerPoint([location.latitude, location.longitude]);
      const group =
        index === active
          ? undefined
          : groups.find(
              (candidate) =>
                candidate.indexes[0] !== active &&
                candidate.center.distanceTo(point) < CLUSTER_PIXELS,
            );
      if (group === undefined) {
        groups.push({ center: point, indexes: [index] });
      } else {
        group.indexes.push(index);
      }
    });
    return groups.map((group) => group.indexes);
  }

  private isGrouped(map: L.Map, index: number): boolean {
    return (
      this.cluster() &&
      this.groups(map, this.locations(), null).some(
        (group) => group.length > 1 && group.includes(index),
      )
    );
  }

  private drawPins(
    map: L.Map,
    locations: MapLocation[],
    active: number | null,
    cluster: boolean,
  ): void {
    this.layers.clearLayers();
    const groups = cluster
      ? this.groups(map, locations, active)
      : locations.map((_location, index) => [index]);
    for (const group of groups) {
      if (group.length === 1) {
        this.drawPin(locations[group[0]], group[0], group[0] === active);
      } else {
        this.drawGroup(
          map,
          group.map((index) => locations[index]),
        );
      }
    }
  }

  private drawPin(location: MapLocation, index: number, active: boolean): void {
    // Leaflet writes a string popup through innerHTML, and users type these names, so the
    // name goes in as text: a name like <img onerror=...> must never run as code.
    const name = document.createElement('span');
    name.textContent = location.name;
    L.marker([location.latitude, location.longitude], {
      icon: pinIcon(location, active),
      keyboard: true,
      title: location.name,
      zIndexOffset: active ? 1000 : 0,
    })
      .bindPopup(name, { offset: [0, location.label === undefined ? -8 : -30] })
      .on('click', () => this.locationClick.emit(index))
      .on('mouseover', () => this.locationHover.emit(index))
      .on('mouseout', () => this.locationHover.emit(null))
      .addTo(this.layers);
  }

  // A round bubble with a count; a click zooms in until the group falls apart.
  private drawGroup(map: L.Map, locations: MapLocation[]): void {
    const bounds = L.latLngBounds(
      locations.map((location): L.LatLngTuple => [location.latitude, location.longitude]),
    );
    L.marker(bounds.getCenter(), {
      icon: groupIcon(locations.length),
      keyboard: true,
      title: `${locations.length} na ovom mestu, kliknite da uvećate`,
    })
      .on('click', () => {
        this.byCode();
        map.flyToBounds(bounds, { padding: [80, 80], maxZoom: 18, duration: 0.5 });
      })
      .addTo(this.layers);
  }

  // ---- Lines, the position and the circle ----

  // One line per segment, so a segment without a route stays a straight line on its own.
  private drawPath(showPath: boolean, route: MapCoordinates[][] | null): void {
    this.pathLayer.clearLayers();
    if (!showPath || route === null) {
      return;
    }
    route.forEach((segment) => {
      L.polyline(
        segment.map((point): L.LatLngTuple => [point.latitude, point.longitude]),
        { color: this.token('--color-accent'), weight: 4, opacity: 0.85 },
      ).addTo(this.pathLayer);
    });
  }

  // A circle instead of a pin, so the selected place looks different from the other locations.
  private drawSelected(selected: MapCoordinates | null): void {
    this.selectedLayer.clearLayers();
    if (selected === null) {
      return;
    }
    const color = this.token('--color-accent');
    L.circleMarker([selected.latitude, selected.longitude], {
      pane: SELECTED_PANE,
      // A click on the circle goes through to the map, so it still moves the selection.
      interactive: false,
      radius: SELECTED_PIN_RADIUS,
      weight: 4,
      color,
      fillColor: color,
      fillOpacity: 0.8,
    }).addTo(this.selectedLayer);
  }

  // A dark dot with a light border and a soft ring, above everything else.
  private drawPosition(position: MapCoordinates | null): void {
    this.positionLayer.clearLayers();
    if (position === null) {
      return;
    }
    L.marker([position.latitude, position.longitude], {
      icon: L.divIcon({
        className: '',
        html:
          '<span style="display:block;width:18px;height:18px;border-radius:50%;' +
          'background:var(--color-text);border:3px solid var(--color-surface);' +
          'box-shadow:0 0 0 6px color-mix(in srgb, var(--color-text) 18%, transparent)"></span>',
        iconSize: [18, 18],
        iconAnchor: [9, 9],
      }),
      // A click on the marker goes through to the map, so it still moves the position.
      interactive: false,
      zIndexOffset: 2000,
    }).addTo(this.positionLayer);
  }

  // A dark line with a light casing under it, like navigation apps, so it reads on any street.
  private drawTargetRoute(route: MapCoordinates[] | null): void {
    this.targetRouteLayer.clearLayers();
    if (route === null || route.length < 2) {
      return;
    }
    const points = route.map((point): L.LatLngTuple => [point.latitude, point.longitude]);
    L.polyline(points, { color: this.token('--color-surface'), weight: 9, opacity: 0.9 }).addTo(
      this.targetRouteLayer,
    );
    L.polyline(points, { color: this.token('--color-text'), weight: 5 }).addTo(
      this.targetRouteLayer,
    );
  }

  // L.circle, not L.circleMarker: its radius is in metres, so it keeps its size against the
  // streets when the map is zoomed.
  private drawArea(area: MapArea | null): void {
    this.areaLayer.clearLayers();
    if (area === null) {
      return;
    }
    const color = this.token('--color-accent');
    L.circle([area.latitude, area.longitude], {
      // A click on the circle goes through to the map, and the circle never moves.
      interactive: false,
      radius: area.radiusMeters,
      weight: 2,
      dashArray: '6 6',
      color,
      fillColor: color,
      fillOpacity: 0.1,
    }).addTo(this.areaLayer);
  }

  // A colour token's value, for the lines Leaflet draws as SVG attributes.
  private token(name: string): string {
    return getComputedStyle(this.host.nativeElement).getPropertyValue(name).trim();
  }
}

// A pin with a label is a rounded tag with a small tail under it, like a price on a booking
// site; without one it is a dot. The active pin is dark and slightly larger.
function pinIcon(location: MapLocation, active: boolean): L.DivIcon {
  const tone = TONES[location.tone ?? 'default'];
  const bg = active ? '--color-text' : tone.bg;
  const text = active ? '--color-surface' : tone.text;
  const border = active || location.tone === 'next' ? bg : '--color-control-border';
  const shadow = 'box-shadow:0 2px 6px rgb(0 0 0 / 0.18);';
  if (location.label === undefined) {
    const size = active ? 18 : 14;
    const fill = location.tone === undefined && !active ? '--color-accent' : bg;
    return L.divIcon({
      className: '',
      html:
        `<span style="display:block;width:${size}px;height:${size}px;border-radius:50%;` +
        `background:var(${fill});border:2px solid var(--color-surface);${shadow}"></span>`,
      iconSize: [size, size],
      iconAnchor: [size / 2, size / 2],
    });
  }
  const html =
    `<span style="position:relative;display:inline-block;transform:translate(-50%,-100%)` +
    `${active ? ' scale(1.12)' : ''};transform-origin:50% 100%;` +
    `background:var(${bg});color:var(${text});border:1px solid var(${border});` +
    `border-radius:999px;padding:2px 10px;font:600 13px/1.4 var(--font-family);` +
    `white-space:nowrap;${shadow}">${escapeHtml(location.label)}` +
    `<span style="position:absolute;left:50%;bottom:-6px;width:10px;height:10px;` +
    `background:var(${bg});border-right:1px solid var(${border});` +
    `border-bottom:1px solid var(${border});transform:translateX(-50%) rotate(45deg)"></span>` +
    `</span>`;
  return L.divIcon({ className: '', html, iconSize: [0, 0], iconAnchor: [0, 6] });
}

function groupIcon(count: number): L.DivIcon {
  const html =
    `<span style="display:flex;align-items:center;justify-content:center;width:36px;` +
    `height:36px;border-radius:50%;transform:translate(-50%,-50%);` +
    `background:var(--color-surface);color:var(--color-text);` +
    `border:2px solid var(--color-accent);font:700 14px/1 var(--font-family);` +
    `box-shadow:0 0 0 5px color-mix(in srgb, var(--color-accent) 22%, transparent),` +
    `0 2px 6px rgb(0 0 0 / 0.18);">${count}</span>`;
  return L.divIcon({ className: '', html, iconSize: [0, 0] });
}

// Labels come from data users type, so they go into the pin's HTML as text, never as markup.
function escapeHtml(text: string): string {
  const span = document.createElement('span');
  span.textContent = text;
  return span.innerHTML;
}
