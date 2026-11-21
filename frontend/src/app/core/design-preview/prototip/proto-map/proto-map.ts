import { HttpClient } from '@angular/common/http';
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
import * as L from 'leaflet';
import { firstValueFrom } from 'rxjs';
import { RouteSegment, routedPath } from '../../../../shared/map/routed-path';

// The colour role of a pin, named after the token pair it uses.
export type PinTone = 'tour' | 'owned' | 'quiz' | 'social' | 'location' | 'muted' | 'next';

export interface ProtoPin {
  id: string;
  latitude: number;
  longitude: number;
  // The text inside the pin: a price for a tour, a short mark for a challenge.
  label: string;
  tone: PinTone;
}

export interface ProtoPoint {
  latitude: number;
  longitude: number;
}

// What the map shows: the page uses it to search "in this area".
export interface ProtoBounds {
  north: number;
  south: number;
  east: number;
  west: number;
}

// Background and text token of each tone; the pin HTML is built by Leaflet outside the
// component's styles, so it refers to the tokens inline.
const TONES: Record<PinTone, { bg: string; text: string }> = {
  tour: { bg: '--color-surface', text: '--color-text' },
  owned: { bg: '--color-success-bg', text: '--color-success-text' },
  quiz: { bg: '--color-xp-bg', text: '--color-xp-text' },
  social: { bg: '--color-ec-bg', text: '--color-ec-text' },
  location: { bg: '--color-state-draft-bg', text: '--color-state-draft-text' },
  muted: { bg: '--color-muted-bg', text: '--color-muted-text' },
  next: { bg: '--color-accent', text: '--color-accent-text' },
};

const DEFAULT_CENTER: L.LatLngTuple = [45.2551, 19.8452];
// Pins closer than this on the screen are drawn as one group with a count.
const CLUSTER_PIXELS = 56;
const ROUTING_URL = 'https://routing.openstreetmap.de/routed-foot/route/v1/foot';

interface RoutingResponse {
  code: string;
  routes?: { distance: number; geometry: { coordinates: [number, number][] } }[];
}

// PROTOTYPE ONLY, second round. A sketch of what shared/map should become:
// - pins in the theme's colours with their label inside, a highlighted pin that follows the list;
// - pins that overlap on screen join into one group with a count; a click on it zooms in;
// - the map flies to the chosen pin when it is out of view, and reports its view after the user
//   moves it, so the page can search "in this area";
// - the key points' path and the way from the position to the next point follow the roads;
// - two buttons on the map: "to my position" and "show everything".
// Like shared/map, it drives Leaflet with ElementRef, afterNextRender, effect() and DestroyRef,
// and asks the routing service with HttpClient. Once approved, this moves into LocationMap.
@Component({
  selector: 'app-proto-map',
  styleUrl: './proto-map.scss',
  templateUrl: './proto-map.html',
})
export class ProtoMap {
  readonly pins = input.required<ProtoPin[]>();
  // The pin drawn inverted and above the others: the card under the mouse or the chosen one.
  readonly activeId = input<string | null>(null);
  // When this pin changes and is out of view (or hidden in a group), the map flies to it.
  readonly focusId = input<string | null>(null);
  // The view is fitted to the pins on the first draw and whenever this key changes. Filtering
  // by the visible area keeps the key, so the view does not jump under the user.
  readonly fitKey = input('');
  // Key points joined in order; along the roads when roadPath is true.
  readonly path = input<ProtoPoint[]>([]);
  readonly roadPath = input(false);
  readonly position = input<ProtoPoint | null>(null);
  // While both are set, the walking route from the position to the target is drawn.
  readonly target = input<ProtoPoint | null>(null);
  // A circle: the range of the chosen challenge, in metres around its centre.
  readonly area = input<(ProtoPoint & { radiusMeters: number }) | null>(null);
  // While true, a click on the map (not on a pin) sends its coordinates, and the cursor says so.
  readonly selectable = input(false);
  // Groups of overlapping pins; off where every pin must stay visible (a tour's key points).
  readonly cluster = input(true);

  readonly pinClick = output<string>();
  readonly pinHover = output<string | null>();
  readonly mapClick = output<ProtoPoint>();
  // The visible area, after the user moved or zoomed the map.
  readonly viewChange = output<ProtoBounds>();
  // The length of the walking route to the target, in metres.
  readonly targetMeters = output<number>();

  protected readonly hasPosition = computed(() => this.position() !== null);

  private readonly host = inject(ElementRef<HTMLElement>);
  private readonly http = inject(HttpClient);
  private readonly map = signal<L.Map | undefined>(undefined);
  // Bumped on every zoom, so the groups are made again for the new scale.
  private readonly zoom = signal(0);
  private readonly pinLayer = L.layerGroup();
  private readonly pathLayer = L.layerGroup();
  private readonly targetLayer = L.layerGroup();
  private readonly positionLayer = L.layerGroup();
  private readonly areaLayer = L.layerGroup();
  private readonly pathRoute = signal<ProtoPoint[][] | null>(null);
  private readonly targetRoute = signal<ProtoPoint[] | null>(null);
  private routeRequest = 0;
  private targetRequest = 0;
  private fittedKey: string | null = null;
  // True while the map moves because the code asked it to; such moves are not reported.
  private movingByCode = false;
  private byCodeTimer: ReturnType<typeof setTimeout> | undefined;
  private resizeObserver: ResizeObserver | null = null;

  constructor() {
    afterNextRender(() => {
      const element = this.host.nativeElement.querySelector('.map') as HTMLElement;
      // No inertia: the map stops where the mouse lets go, and moveend comes at once.
      const map = L.map(element, { zoomControl: false, inertia: false });
      L.control.zoom({ position: 'bottomright' }).addTo(map);
      // A softer base map: the tiles are half transparent over the parchment background.
      L.tileLayer('https://{s}.tile.openstreetmap.org/{z}/{x}/{y}.png', {
        attribution: '&copy; <a href="https://www.openstreetmap.org/copyright">OpenStreetMap</a>',
        maxZoom: 19,
        opacity: 0.62,
      }).addTo(map);
      this.areaLayer.addTo(map);
      this.pathLayer.addTo(map);
      this.targetLayer.addTo(map);
      this.pinLayer.addTo(map);
      this.positionLayer.addTo(map);
      map.on('click', (event: L.LeafletMouseEvent) => {
        if (this.selectable()) {
          this.mapClick.emit({ latitude: event.latlng.lat, longitude: event.latlng.lng });
        }
      });
      map.on('zoomend', () => this.zoom.update((z) => z + 1));
      map.on('moveend', () => {
        if (this.movingByCode) {
          return;
        }
        const b = map.getBounds();
        this.viewChange.emit({
          north: b.getNorth(),
          south: b.getSouth(),
          east: b.getEast(),
          west: b.getWest(),
        });
      });
      this.map.set(map);
      const resize = new ResizeObserver(() => map.invalidateSize());
      resize.observe(element);
      this.resizeObserver = resize;
    });

    // Fit the view first, then group: the groups depend on the scale.
    effect(() => {
      const map = this.map();
      const key = this.fitKey();
      if (map !== undefined && key !== this.fittedKey) {
        this.fittedKey = key;
        untracked(() => this.fitAll(map, false));
      }
    });

    effect(() => {
      const map = this.map();
      this.zoom();
      if (map !== undefined) {
        this.drawPins(map, this.pins(), this.activeId(), this.cluster());
      }
    });

    // Fly to the focused pin when the user cannot see it.
    effect(() => {
      const map = this.map();
      const id = this.focusId();
      if (map === undefined || id === null) {
        return;
      }
      const pin = untracked(() => this.pins()).find((p) => p.id === id);
      if (pin === undefined) {
        return;
      }
      const point = L.latLng(pin.latitude, pin.longitude);
      const inView = map.getBounds().pad(-0.1).contains(point);
      if (!inView || this.isGrouped(map, pin)) {
        this.byCode();
        map.flyTo(point, Math.max(map.getZoom(), 16), { duration: 0.6 });
      }
    });

    // Routes along the roads for the path, asked once per list of points.
    effect(() => {
      const points = this.path();
      const road = this.roadPath();
      if (!road || points.length < 2) {
        this.routeRequest++;
        this.pathRoute.set(null);
        return;
      }
      void this.fetchPath(points);
    });

    effect(() => {
      if (this.map() === undefined) {
        return;
      }
      this.pathLayer.clearLayers();
      const routed = this.pathRoute();
      const lines = routed ?? [this.path()];
      for (const line of lines) {
        if (line.length > 1) {
          L.polyline(
            line.map((p): L.LatLngTuple => [p.latitude, p.longitude]),
            {
              color: this.token('--color-accent'),
              weight: 4,
              opacity: routed === null ? 0.6 : 0.85,
              dashArray: routed === null ? '8 8' : undefined,
            },
          ).addTo(this.pathLayer);
        }
      }
    });

    effect(() => {
      const position = this.position();
      const target = this.target();
      if (position === null || target === null) {
        this.targetRequest++;
        this.targetRoute.set(null);
        return;
      }
      void this.fetchTarget(position, target);
    });

    effect(() => {
      if (this.map() === undefined) {
        return;
      }
      this.targetLayer.clearLayers();
      const route = this.targetRoute();
      if (route !== null && route.length > 1) {
        const latLngs = route.map((p): L.LatLngTuple => [p.latitude, p.longitude]);
        // A white casing under the line, like navigation apps, so it reads on any street colour.
        L.polyline(latLngs, {
          color: this.token('--color-surface'),
          weight: 9,
          opacity: 0.9,
        }).addTo(this.targetLayer);
        L.polyline(latLngs, { color: this.token('--color-text'), weight: 5 }).addTo(
          this.targetLayer,
        );
      }
    });

    effect(() => {
      if (this.map() === undefined) {
        return;
      }
      this.positionLayer.clearLayers();
      const position = this.position();
      if (position !== null) {
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
          zIndexOffset: 2000,
          interactive: false,
        }).addTo(this.positionLayer);
      }
    });

    effect(() => {
      if (this.map() === undefined) {
        return;
      }
      this.areaLayer.clearLayers();
      const area = this.area();
      if (area !== null) {
        const color = this.token('--color-accent');
        L.circle([area.latitude, area.longitude], {
          radius: area.radiusMeters,
          color,
          weight: 2,
          dashArray: '6 6',
          fillColor: color,
          fillOpacity: 0.08,
          interactive: false,
        }).addTo(this.areaLayer);
      }
    });

    effect(() => {
      const map = this.map();
      if (map !== undefined) {
        map.getContainer().style.cursor = this.selectable() ? 'crosshair' : '';
      }
    });

    inject(DestroyRef).onDestroy(() => {
      this.routeRequest++;
      this.targetRequest++;
      this.resizeObserver?.disconnect();
      this.map()?.remove();
    });
  }

  // Marks the next moves as made by the code, not the user. A move to where the map already is
  // fires no moveend, so the mark clears itself after the animation instead of waiting for one.
  private byCode(): void {
    this.movingByCode = true;
    clearTimeout(this.byCodeTimer);
    this.byCodeTimer = setTimeout(() => (this.movingByCode = false), 900);
  }

  // ---- The two buttons on the map ----

  protected toPosition(): void {
    const map = this.map();
    const position = this.position();
    if (map !== undefined && position !== null) {
      this.byCode();
      map.flyTo([position.latitude, position.longitude], Math.max(map.getZoom(), 16), {
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

  // ---- Drawing ----

  private fitAll(map: L.Map, animate: boolean): void {
    const position = this.position();
    const points = [...this.pins(), ...this.path(), ...(position === null ? [] : [position])].map(
      (p): L.LatLngTuple => [p.latitude, p.longitude],
    );
    this.byCode();
    if (points.length === 0) {
      map.setView(DEFAULT_CENTER, 13, { animate });
    } else if (points.length === 1) {
      map.setView(points[0], 16, { animate });
    } else {
      map.fitBounds(L.latLngBounds(points), { padding: [64, 64], animate });
    }
  }

  // Greedy grouping by screen distance; the active pin always stays on its own.
  private groups(map: L.Map, pins: ProtoPin[], active: string | null): ProtoPin[][] {
    const groups: { center: L.Point; pins: ProtoPin[] }[] = [];
    for (const pin of pins) {
      const point = map.latLngToLayerPoint([pin.latitude, pin.longitude]);
      const group =
        pin.id === active
          ? undefined
          : groups.find(
              (g) => g.pins[0].id !== active && g.center.distanceTo(point) < CLUSTER_PIXELS,
            );
      if (group) {
        group.pins.push(pin);
      } else {
        groups.push({ center: point, pins: [pin] });
      }
    }
    return groups.map((g) => g.pins);
  }

  private isGrouped(map: L.Map, pin: ProtoPin): boolean {
    if (!this.cluster()) {
      return false;
    }
    return this.groups(map, this.pins(), null).some(
      (group) => group.length > 1 && group.includes(pin),
    );
  }

  private drawPins(map: L.Map, pins: ProtoPin[], active: string | null, cluster: boolean): void {
    this.pinLayer.clearLayers();
    const groups = cluster ? this.groups(map, pins, active) : pins.map((pin) => [pin]);
    for (const group of groups) {
      if (group.length === 1) {
        const pin = group[0];
        L.marker([pin.latitude, pin.longitude], {
          icon: this.icon(pin, pin.id === active),
          keyboard: true,
          title: pin.label,
          zIndexOffset: pin.id === active ? 1000 : 0,
        })
          .on('click', () => this.pinClick.emit(pin.id))
          .on('mouseover', () => this.pinHover.emit(pin.id))
          .on('mouseout', () => this.pinHover.emit(null))
          .addTo(this.pinLayer);
      } else {
        const bounds = L.latLngBounds(group.map((p): L.LatLngTuple => [p.latitude, p.longitude]));
        L.marker(bounds.getCenter(), {
          icon: this.groupIcon(group.length),
          keyboard: true,
          title: `${group.length} na ovom mestu, kliknite da uvećate`,
        })
          .on('click', () => {
            this.byCode();
            map.flyToBounds(bounds, { padding: [80, 80], maxZoom: 18, duration: 0.5 });
          })
          .addTo(this.pinLayer);
      }
    }
  }

  // A rounded label with a small tail under it, like a price tag on a booking site.
  private icon(pin: ProtoPin, active: boolean): L.DivIcon {
    const tone = TONES[pin.tone];
    const bg = active ? '--color-text' : tone.bg;
    const text = active ? '--color-surface' : tone.text;
    const border = active
      ? '--color-text'
      : pin.tone === 'next'
        ? tone.bg
        : '--color-control-border';
    const html =
      `<span style="position:relative;display:inline-block;transform:translate(-50%,-100%)` +
      `${active ? ' scale(1.12)' : ''};transform-origin:50% 100%;` +
      `background:var(${bg});color:var(${text});border:1px solid var(${border});` +
      `border-radius:999px;padding:2px 10px;font:600 13px/1.4 var(--font-family);` +
      `white-space:nowrap;box-shadow:0 2px 6px rgb(0 0 0 / 0.18);">` +
      `${escape(pin.label)}` +
      `<span style="position:absolute;left:50%;bottom:-6px;width:10px;height:10px;` +
      `background:var(${bg});border-right:1px solid var(${border});` +
      `border-bottom:1px solid var(${border});transform:translateX(-50%) rotate(45deg)"></span>` +
      `</span>`;
    return L.divIcon({ className: '', html, iconSize: [0, 0], iconAnchor: [0, 6] });
  }

  // A round bubble with the count of pins it stands for.
  private groupIcon(count: number): L.DivIcon {
    const html =
      `<span style="display:flex;align-items:center;justify-content:center;width:36px;` +
      `height:36px;border-radius:50%;transform:translate(-50%,-50%);` +
      `background:var(--color-surface);color:var(--color-text);` +
      `border:2px solid var(--color-accent);font:700 14px/1 var(--font-family);` +
      `box-shadow:0 0 0 5px color-mix(in srgb, var(--color-accent) 22%, transparent),` +
      `0 2px 6px rgb(0 0 0 / 0.18);">${count}</span>`;
    return L.divIcon({ className: '', html, iconSize: [0, 0] });
  }

  // ---- Routes along the roads ----

  private async fetchPath(points: ProtoPoint[]): Promise<void> {
    const request = ++this.routeRequest;
    this.pathRoute.set(null);
    const path = await routedPath(
      points.map((p) => ({ ...p, name: '' })),
      (from, to) => this.fetchSegment(from, to),
    );
    if (request === this.routeRequest) {
      this.pathRoute.set(path.segments);
    }
  }

  private async fetchTarget(from: ProtoPoint, to: ProtoPoint): Promise<void> {
    const request = ++this.targetRequest;
    const path = await routedPath(
      [
        { ...from, name: '' },
        { ...to, name: '' },
      ],
      (a, b) => this.fetchSegment(a, b),
    );
    if (request === this.targetRequest) {
      this.targetRoute.set(path.segments[0]);
      this.targetMeters.emit(Math.round(path.lengthKm * 1000));
    }
  }

  private async fetchSegment(from: ProtoPoint, to: ProtoPoint): Promise<RouteSegment | null> {
    const url =
      `${ROUTING_URL}/${from.longitude},${from.latitude};${to.longitude},${to.latitude}` +
      '?overview=full&geometries=geojson';
    const response = await firstValueFrom(this.http.get<RoutingResponse>(url, { timeout: 5000 }));
    const route = response.routes?.[0];
    if (response.code !== 'Ok' || route === undefined) {
      return null;
    }
    return {
      points: route.geometry.coordinates.map(([longitude, latitude]) => ({ latitude, longitude })),
      lengthKm: route.distance / 1000,
    };
  }

  private token(name: string): string {
    return getComputedStyle(this.host.nativeElement).getPropertyValue(name).trim();
  }
}

// Labels are mock text here, but the real map takes names users type, so they never go in as HTML.
function escape(text: string): string {
  const span = document.createElement('span');
  span.textContent = text;
  return span.innerHTML;
}
