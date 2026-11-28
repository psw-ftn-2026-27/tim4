import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import { LocationMap, MapBounds, MapLocation } from '../../../../shared/map/location-map';
import { distanceKm } from '../../../../shared/map/path-length';
import {
  PublishedTourDto,
  PublishedTourSearchDto,
  TourDifficulty,
  TourPreferenceDto,
  TourSort,
  TransportMode,
} from '../../api/exploration-api-types';
import { TourBrowsing } from '../tour-browsing';
import { TourCard, difficultyLabels, transportLabels } from '../tour-card/tour-card';

const PAGE_SIZE = 20;

const EMPTY_FILTERS = {
  name: '',
  tags: '',
  easy: false,
  moderate: false,
  hard: false,
  transport: '' as TransportMode | '',
  priceTo: null as number | null,
  lengthTo: null as number | null,
  sort: '' as TourSort | '',
};

// The circle the server searches in, made from the part of the map the tourist chose.
interface SearchArea {
  latitude: number;
  longitude: number;
  radiusInKm: number;
}

// Find a tour: the list on the left and the map on the right show the same tours. Chips and
// the sort apply at once; the two text fields apply on Enter. Moving the map offers "Pretraži
// ovu oblast", which searches the circle around the visible area on the server and records the
// search (E23), so the old separate map search is this page now.
@Component({
  imports: [FormField, LocationMap, RouterLink, TourCard],
  selector: 'app-tour-list',
  styleUrl: './tour-list.scss',
  templateUrl: './tour-list.html',
})
export class TourList {
  private readonly auth = inject(Auth);
  private readonly tourBrowsing = inject(TourBrowsing);

  protected readonly filters = form(signal({ ...EMPTY_FILTERS }));

  // The text fields as of the last Enter, so typing does not reload the list on every key.
  private readonly appliedText = signal({ name: '', tags: '' });

  // The tourist's saved preference; 204 No Content reads as null.
  private readonly preference = httpResource<TourPreferenceDto | null>(
    () => '/api/exploration/preferences',
  );
  // 'auto' follows the preference's own "open the search with my preferences" choice until the
  // tourist turns the chip on or off.
  private readonly preferenceMode = signal<'auto' | 'on' | 'off'>('auto');
  protected readonly savedPreference = computed(() =>
    this.preference.hasValue() ? this.preference.value() : null,
  );
  protected readonly usingPreference = computed(() => {
    const preference = this.savedPreference();
    const mode = this.preferenceMode();
    return (
      preference !== null &&
      (mode === 'on' || (mode === 'auto' && preference.openSearchWithPreferences))
    );
  });
  protected readonly preferenceText = computed(() => {
    const preference = this.savedPreference();
    if (preference === null) {
      return '';
    }
    const parts = [
      ...preference.difficulties.map((difficulty) => difficultyLabels[difficulty]),
      ...(preference.transport === null ? [] : [transportLabels[preference.transport]]),
      ...preference.tags,
    ];
    return parts.join(', ');
  });

  // The visible area after the tourist last moved the map, and the area the list is limited to.
  protected readonly view = signal<MapBounds | null>(null);
  protected readonly area = signal<SearchArea | null>(null);
  private readonly areaView = signal<MapBounds | null>(null);
  protected readonly canSearchArea = computed(
    () => this.view() !== null && this.view() !== this.areaView(),
  );

  // Everything the server filters by, except the page.
  private readonly criteria = computed<PublishedTourSearchDto>(() => {
    const filters = this.filters().value();
    const text = this.appliedText();
    const area = this.area();
    const preference = this.usingPreference() ? this.savedPreference() : null;
    const difficulties: TourDifficulty[] = [];
    if (filters.easy) {
      difficulties.push('Easy');
    }
    if (filters.moderate) {
      difficulties.push('Moderate');
    }
    if (filters.hard) {
      difficulties.push('Hard');
    }
    const tags = text.tags
      .split(',')
      .map((tag) => tag.trim())
      .filter((tag) => tag !== '');
    return {
      name: text.name.trim() === '' ? null : text.name.trim(),
      difficulties: preference ? nullIfEmpty(preference.difficulties) : nullIfEmpty(difficulties),
      tags: preference ? nullIfEmpty(preference.tags) : nullIfEmpty(tags),
      transport: preference ? preference.transport : filters.transport || null,
      priceFrom: null,
      priceTo: wholeNumber(filters.priceTo),
      lengthFrom: null,
      lengthTo: positiveNumber(filters.lengthTo),
      sort: filters.sort || null,
      latitude: area?.latitude ?? null,
      longitude: area?.longitude ?? null,
      radiusInKm: area?.radiusInKm ?? null,
    };
  });

  // The map is fitted again when the filters change, but not when only the area does: the
  // tourist chose that view, so it stays put under the mouse.
  protected readonly fitKey = computed(() =>
    JSON.stringify({ ...this.criteria(), latitude: null, longitude: null, radiusInKm: null }),
  );

  // A page belongs to one set of criteria; new criteria start again at page 1.
  private readonly pageState = signal({ key: '', page: 1 });
  protected readonly page = computed(() =>
    this.pageState().key === JSON.stringify(this.criteria()) ? this.pageState().page : 1,
  );

  protected readonly tours = httpResource<PageResult<PublishedTourDto>>(
    () => `/api/exploration/tours/published?${toQuery(this.criteria(), this.page())}`,
  );

  protected readonly items = computed(() =>
    this.tours.hasValue() ? this.tours.value().items : [],
  );
  protected readonly totalCount = computed(() =>
    this.tours.hasValue() ? this.tours.value().totalCount : 0,
  );
  // "1 tura", "3 ture", "6 tura".
  protected readonly countText = computed(() => {
    const count = this.totalCount();
    const last = count % 10;
    const lastTwo = count % 100;
    if (last === 1 && lastTwo !== 11) {
      return `${count} tura`;
    }
    return last >= 2 && last <= 4 && (lastTwo < 12 || lastTwo > 14)
      ? `${count} ture`
      : `${count} tura`;
  });
  protected readonly pageCount = computed(() =>
    Math.max(1, Math.ceil(this.totalCount() / PAGE_SIZE)),
  );

  // The tours with a place on the map, in the order of their pins.
  private readonly pinned = computed(() =>
    this.items().filter((item) => item.pinLocation !== null),
  );

  protected readonly pins = computed<MapLocation[]>(() =>
    this.pinned().map((item) => ({
      latitude: item.pinLocation!.latitude,
      longitude: item.pinLocation!.longitude,
      name: item.tour.name,
      label: this.pinLabel(item),
      tone: item.isInProgress ? 'next' : this.isOwn(item) ? 'muted' : 'default',
    })),
  );

  // The card or pin under the mouse, and the tour whose pin was clicked or whose "Na mapi" was.
  protected readonly hovered = signal<string | null>(null);
  protected readonly chosen = signal<string | null>(null);
  protected readonly activeId = computed(() => this.hovered() ?? this.chosen());
  protected readonly activeIndex = computed(() => this.pinIndex(this.activeId()));
  protected readonly focusIndex = computed(() => this.pinIndex(this.chosen()));
  protected readonly chosenItem = computed(
    () => this.items().find((item) => item.tour.id === this.chosen()) ?? null,
  );

  // Text of the closed chips: what is chosen, or the name of the filter.
  protected readonly difficultyChip = computed(() => {
    const filters = this.filters().value();
    const chosen = [
      filters.easy ? 'Lako' : null,
      filters.moderate ? 'Srednje' : null,
      filters.hard ? 'Teško' : null,
    ].filter((label) => label !== null);
    return chosen.length === 0 ? 'Težina' : chosen.join(', ');
  });
  protected readonly transportChip = computed(() => {
    const transport = this.filters.transport().value();
    return transport === '' ? 'Prevoz' : transportLabels[transport];
  });
  protected readonly priceChip = computed(() => {
    const price = wholeNumber(this.filters.priceTo().value());
    return price === null ? 'Cena' : `Do ${price} EC`;
  });
  protected readonly lengthChip = computed(() => {
    const length = positiveNumber(this.filters.lengthTo().value());
    return length === null ? 'Dužina' : `Do ${String(length).replace('.', ',')} km`;
  });

  protected readonly anyFilter = computed(() => {
    const criteria = this.criteria();
    return (
      criteria.name !== null ||
      criteria.tags !== null ||
      criteria.difficulties !== null ||
      criteria.transport !== null ||
      criteria.priceTo !== null ||
      criteria.lengthTo !== null ||
      criteria.latitude !== null
    );
  });

  protected applyText(event: Event): void {
    event.preventDefault();
    this.appliedText.set({
      name: this.filters.name().value(),
      tags: this.filters.tags().value(),
    });
  }

  protected togglePreference(): void {
    this.preferenceMode.set(this.usingPreference() ? 'off' : 'on');
  }

  protected clear(): void {
    this.filters().reset({ ...EMPTY_FILTERS });
    this.appliedText.set({ name: '', tags: '' });
    this.preferenceMode.set('off');
    this.clearArea();
  }

  // The circle around the visible area: its centre, and the distance to a corner, so every
  // tour the tourist sees on the map is inside it. The search is recorded, as before (E23).
  protected searchArea(): void {
    const view = this.view();
    if (view === null) {
      return;
    }
    const latitude = (view.north + view.south) / 2;
    const longitude = (view.east + view.west) / 2;
    const radiusInKm = Math.max(
      0.1,
      distanceKm({ latitude, longitude }, { latitude: view.north, longitude: view.east }),
    );
    const area = { latitude, longitude, radiusInKm: Math.round(radiusInKm * 10) / 10 };
    this.area.set(area);
    this.areaView.set(view);
    const criteria = this.criteria();
    void this.record(area, criteria);
  }

  protected clearArea(): void {
    this.area.set(null);
    this.areaView.set(null);
    this.view.set(null);
  }

  protected changePage(change: number): void {
    const page = this.page() + change;
    if (page >= 1 && page <= this.pageCount()) {
      this.pageState.set({ key: JSON.stringify(this.criteria()), page });
    }
  }

  protected hoverPin(index: number | null): void {
    this.hovered.set(index === null ? null : (this.pinned()[index]?.tour.id ?? null));
  }

  protected choosePin(index: number): void {
    this.chosen.set(this.pinned()[index]?.tour.id ?? null);
  }

  protected isOwn(item: PublishedTourDto): boolean {
    return item.tour.authorId === this.auth.user()?.id;
  }

  protected priceText(item: PublishedTourDto): string {
    if (item.price === null) {
      return 'Cena nije određena';
    }
    return item.price === 0 ? 'Besplatno' : `${item.price} EC`;
  }

  private pinLabel(item: PublishedTourDto): string {
    if (item.isInProgress) {
      return '▶ U toku';
    }
    if (item.isPurchased) {
      return '✓ Kupljena';
    }
    if (this.isOwn(item)) {
      return '✎ Vaša';
    }
    return item.price === null ? '—' : item.price === 0 ? 'Besplatno' : `${item.price} EC`;
  }

  private pinIndex(id: string | null): number | null {
    const index = this.pinned().findIndex((item) => item.tour.id === id);
    return index === -1 ? null : index;
  }

  private async record(area: SearchArea, criteria: PublishedTourSearchDto): Promise<void> {
    try {
      await this.tourBrowsing.recordLocationSearch({
        ...area,
        name: criteria.name,
        difficulties: criteria.difficulties,
        tags: criteria.tags,
        transport: criteria.transport,
        priceFrom: criteria.priceFrom,
        priceTo: criteria.priceTo,
        lengthFrom: criteria.lengthFrom,
        lengthTo: criteria.lengthTo,
      });
    } catch {
      // The tourist does not know that searches are recorded, so a failure shows nothing.
    }
  }
}

function nullIfEmpty<T>(values: T[]): T[] | null {
  return values.length === 0 ? null : values;
}

// Prices are whole EC; an empty or invalid field means no bound.
function wholeNumber(value: number | null): number | null {
  return value === null || Number.isNaN(value) || value < 0 ? null : Math.trunc(value);
}

function positiveNumber(value: number | null): number | null {
  return value === null || Number.isNaN(value) || value <= 0 ? null : value;
}

// Empty criteria are left out of the URL.
function toQuery(search: PublishedTourSearchDto, page: number): string {
  const query = new URLSearchParams({ page: String(page), pageSize: String(PAGE_SIZE) });
  const add = (key: string, value: string | number | null) => {
    if (value !== null) {
      query.append(key, String(value));
    }
  };
  add('name', search.name);
  for (const difficulty of search.difficulties ?? []) {
    query.append('difficulties', difficulty);
  }
  for (const tag of search.tags ?? []) {
    query.append('tags', tag);
  }
  add('transport', search.transport);
  add('priceFrom', search.priceFrom);
  add('priceTo', search.priceTo);
  add('lengthFrom', search.lengthFrom);
  add('lengthTo', search.lengthTo);
  add('sort', search.sort);
  add('latitude', search.latitude);
  add('longitude', search.longitude);
  add('radiusInKm', search.radiusInKm);
  return query.toString();
}
