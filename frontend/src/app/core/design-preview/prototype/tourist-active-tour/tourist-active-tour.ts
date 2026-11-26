import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { distanceText, metersBetween } from '../geo';
import { activeReachedCount, activeTourId, secrets, tours } from '../mock';
import { ProtoMap, ProtoPin, ProtoPoint } from '../proto-map/proto-map';

// How close the position must be to the next key point to reach it, in metres.
const REACH_METERS = 60;
const START: ProtoPoint = { latitude: 45.2468, longitude: 19.8512 };

// PROTOTYPE of scenario S3, the tour in progress, second round. On a walk the map is the screen,
// as in navigation apps: it fills the page, and the rest floats over it. On top, where to go next
// and how far along the roads; when a point is reached, its secret opens over the map as the
// moment of the tour; the secrets found so far wait in a drawer; leaving the tour is in a menu.
@Component({
  imports: [ProtoMap, RouterLink],
  selector: 'app-tourist-active-tour',
  styleUrl: './tourist-active-tour.scss',
  templateUrl: './tourist-active-tour.html',
})
export class TouristActiveTour {
  protected readonly tour = tours.find((tour) => tour.id === activeTourId)!;
  private readonly tourSecrets = secrets[activeTourId];

  protected readonly reached = signal(activeReachedCount);
  protected readonly position = signal<ProtoPoint | null>(START);
  // The point reached by the latest move; its secret opens over the map until it is closed.
  protected readonly justReached = signal<number | null>(null);
  protected readonly secretsOpen = signal(false);
  protected readonly menuOpen = signal(false);
  protected readonly confirmingAbandon = signal(false);
  // Metres along the roads to the next point, as the map's route reports them.
  protected readonly routeMeters = signal<number | null>(null);

  protected readonly total = this.tour.keyPoints.length;
  protected readonly completed = computed(() => this.reached() === this.total);
  protected readonly next = computed(() =>
    this.completed() ? null : this.tour.keyPoints[this.reached()],
  );

  protected readonly found = computed(() =>
    this.tour.keyPoints.slice(0, this.reached()).map((point, index) => ({
      index,
      name: point.name,
      secret: this.tourSecrets[index],
    })),
  );

  protected readonly newSecret = computed(() => {
    const index = this.justReached();
    return index === null ? null : this.found()[index];
  });

  protected readonly distance = computed(() => {
    const meters = this.routeMeters();
    if (meters !== null) {
      return distanceText(meters);
    }
    const next = this.next();
    const position = this.position();
    return next === null || position === null ? null : distanceText(metersBetween(position, next));
  });

  // About 5 km/h on foot.
  protected readonly minutes = computed(() => {
    const meters = this.routeMeters();
    return meters === null ? null : Math.max(1, Math.round(meters / 83));
  });

  protected readonly pins = computed<ProtoPin[]>(() =>
    this.tour.keyPoints.map((point, index) => ({
      id: String(index),
      latitude: point.latitude,
      longitude: point.longitude,
      label:
        index < this.reached()
          ? `✓ ${index + 1}`
          : index === this.reached()
            ? `${index + 1}. ${point.name}`
            : `${index + 1}`,
      tone: index < this.reached() ? 'owned' : index === this.reached() ? 'next' : 'muted',
    })),
  );

  protected readonly progress = computed(() => `${(this.reached() / this.total) * 100}%`);

  protected move(point: ProtoPoint): void {
    if (this.completed()) {
      return;
    }
    this.position.set(point);
    this.routeMeters.set(null);
    const next = this.next();
    if (next !== null && metersBetween(point, next) <= REACH_METERS) {
      this.justReached.set(this.reached());
      this.reached.update((count) => count + 1);
    }
  }

  // Prototype shortcut: put the position right on the next point.
  protected walkToNext(): void {
    const next = this.next();
    if (next !== null) {
      this.move({ latitude: next.latitude, longitude: next.longitude });
    }
  }

  protected restart(): void {
    this.reached.set(activeReachedCount);
    this.justReached.set(null);
    this.position.set(START);
    this.routeMeters.set(null);
  }
}
