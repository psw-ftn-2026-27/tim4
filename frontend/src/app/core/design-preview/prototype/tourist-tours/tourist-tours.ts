import { Component, computed, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import {
  ProtoTour,
  Transport,
  difficultyClasses,
  difficultyLabels,
  tours,
  transportLabels,
} from '../mock';
import { ProtoBounds, ProtoMap, ProtoPin, ProtoPoint } from '../proto-map/proto-map';

type Sort = 'recommended' | 'priceAsc' | 'priceDesc' | 'lengthAsc';

const EMPTY = {
  query: '',
  easy: false,
  moderate: false,
  hard: false,
  transport: '' as Transport | '',
  maxPrice: null as number | null,
  sort: 'recommended' as Sort,
};

// The preference the tourist saved; the chip applies it to the filters.
const PREFERENCE = { easy: true, moderate: true, hard: false, transport: 'Walking' as Transport };

// PROTOTYPE of scenario S1 (find a tour). One screen instead of today's list and map search:
// results on the left, a tall map on the right, both showing the same tours. Filters apply as
// they change, the way booking sites do, so there is no "Pretraži" button.
@Component({
  imports: [FormField, ProtoMap, RouterLink],
  selector: 'app-tourist-tours',
  styleUrl: './tourist-tours.scss',
  templateUrl: './tourist-tours.html',
})
export class TouristTours {
  protected readonly difficultyLabels = difficultyLabels;
  protected readonly difficultyClasses = difficultyClasses;
  protected readonly transportLabels = transportLabels;
  protected readonly total = tours.length;

  protected readonly filters = form(signal({ ...EMPTY }));
  protected readonly usingPreference = signal(false);

  // The card or pin under the mouse, and the one chosen with a click on the map.
  protected readonly hovered = signal<string | null>(null);
  protected readonly chosen = signal<string | null>(null);
  protected readonly activeId = computed(() => this.hovered() ?? this.chosen());

  // The area the map showed when the user last moved it, and the area the list is limited to
  // since "Pretraži ovu oblast". The button shows only while the two differ.
  protected readonly view = signal<ProtoBounds | null>(null);
  protected readonly area = signal<ProtoBounds | null>(null);
  protected readonly areaApplied = computed(() => this.area() !== null);
  protected readonly canSearchArea = computed(
    () => this.view() !== null && this.view() !== this.area(),
  );

  // The view is fitted again only when the filters change, never when the area does.
  protected readonly fitKey = computed(() => JSON.stringify(this.filters().value()));

  protected readonly chosenTour = computed(
    () => tours.find((tour) => tour.id === this.chosen()) ?? null,
  );

  protected readonly results = computed(() => {
    const f = this.filters().value();
    const difficulties = [
      f.easy ? 'Easy' : null,
      f.moderate ? 'Moderate' : null,
      f.hard ? 'Hard' : null,
    ].filter((d) => d !== null);
    const query = f.query.trim().toLowerCase();
    const list = tours.filter(
      (tour) =>
        (query === '' ||
          tour.name.toLowerCase().includes(query) ||
          tour.tags.some((tag) => tag.includes(query))) &&
        (difficulties.length === 0 || difficulties.includes(tour.difficulty)) &&
        (f.transport === '' || tour.times[f.transport] !== undefined) &&
        (f.maxPrice === null || Number.isNaN(f.maxPrice) || tour.price <= f.maxPrice) &&
        inside(this.area(), tour.keyPoints[0]),
    );
    const sorted = [...list];
    switch (f.sort) {
      case 'priceAsc':
        sorted.sort((a, b) => a.price - b.price);
        break;
      case 'priceDesc':
        sorted.sort((a, b) => b.price - a.price);
        break;
      case 'lengthAsc':
        sorted.sort((a, b) => a.lengthKm - b.lengthKm);
        break;
    }
    return sorted;
  });

  protected readonly pins = computed<ProtoPin[]>(() =>
    this.results().map((tour) => ({
      id: tour.id,
      latitude: tour.keyPoints[0].latitude,
      longitude: tour.keyPoints[0].longitude,
      label: this.pinLabel(tour),
      tone: tour.relation === 'purchased' || tour.relation === 'inProgress' ? 'owned' : 'tour',
    })),
  );

  // Text of the closed chips: what is chosen, or the name of the filter.
  protected readonly difficultyChip = computed(() => {
    const f = this.filters().value();
    const chosen = [
      f.easy ? 'Lako' : null,
      f.moderate ? 'Srednje' : null,
      f.hard ? 'Teško' : null,
    ].filter((d) => d !== null);
    return chosen.length === 0 ? 'Težina' : chosen.join(', ');
  });

  protected readonly transportChip = computed(() => {
    const transport = this.filters.transport().value();
    return transport === '' ? 'Prevoz' : transportLabels[transport];
  });

  protected readonly activeFilterCount = computed(() => {
    const f = this.filters().value();
    return (
      [f.easy, f.moderate, f.hard].filter(Boolean).length +
      (f.transport !== '' ? 1 : 0) +
      (f.maxPrice !== null && !Number.isNaN(f.maxPrice) ? 1 : 0)
    );
  });

  protected togglePreference(): void {
    if (this.usingPreference()) {
      this.clear();
      return;
    }
    this.filters().value.update((value) => ({ ...value, ...PREFERENCE }));
    this.usingPreference.set(true);
  }

  protected clear(): void {
    this.filters().reset({ ...EMPTY });
    this.usingPreference.set(false);
    this.area.set(null);
  }

  // A click on a pin picks its card and brings it into view in the list.
  protected searchArea(): void {
    this.area.set(this.view());
  }

  protected clearArea(): void {
    this.area.set(null);
  }

  protected choose(id: string): void {
    this.chosen.set(id);
    document.getElementById('tour-' + id)?.scrollIntoView({ behavior: 'smooth', block: 'center' });
  }

  protected times(tour: ProtoTour): { label: string; minutes: number }[] {
    return (Object.keys(tour.times) as Transport[]).map((mode) => ({
      label: transportLabels[mode],
      minutes: tour.times[mode]!,
    }));
  }

  protected duration(minutes: number): string {
    const hours = Math.floor(minutes / 60);
    const rest = minutes % 60;
    if (hours === 0) {
      return `${rest} min`;
    }
    return rest === 0 ? `${hours} h` : `${hours} h ${rest} min`;
  }

  private pinLabel(tour: ProtoTour): string {
    switch (tour.relation) {
      case 'purchased':
        return '✓ Kupljena';
      case 'inProgress':
        return '▶ U toku';
      default:
        return tour.price === 0 ? 'Besplatno' : `${tour.price} EC`;
    }
  }
}

function inside(area: ProtoBounds | null, point: ProtoPoint): boolean {
  return (
    area === null ||
    (point.latitude <= area.north &&
      point.latitude >= area.south &&
      point.longitude <= area.east &&
      point.longitude >= area.west)
  );
}
