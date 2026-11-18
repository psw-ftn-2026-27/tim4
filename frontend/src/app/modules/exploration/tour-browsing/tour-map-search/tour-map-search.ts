import { httpResource } from '@angular/common/http';
import { Component, computed, signal } from '@angular/core';
import { FormField, form, min, validate } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { PageResult } from '../../../../shared/api/page-result';
import { LocationMap, MapCoordinates, MapLocation } from '../../../../shared/map/location-map';
import {
  PublishedTourDto,
  PublishedTourSearchDto,
  TourDifficulty,
  TourSort,
  TransportMode,
} from '../../api/exploration-api-types';
import { TourCard } from '../tour-card/tour-card';

const EMPTY_FORM = {
  point: null as MapCoordinates | null,
  radiusInKm: null as number | null,
  name: '',
  easy: false,
  moderate: false,
  hard: false,
  tags: '',
  transport: '' as TransportMode | '',
  priceFrom: null as number | null,
  priceTo: null as number | null,
  lengthFrom: null as number | null,
  lengthTo: null as number | null,
  sort: '' as TourSort | '',
};

const NO_CRITERIA: PublishedTourSearchDto = {
  name: null,
  difficulties: null,
  tags: null,
  transport: null,
  priceFrom: null,
  priceTo: null,
  lengthFrom: null,
  lengthTo: null,
  sort: null,
  latitude: null,
  longitude: null,
  radiusInKm: null,
};

@Component({
  imports: [FormField, LocationMap, RouterLink, TourCard],
  selector: 'app-tour-map-search',
  styleUrl: './tour-map-search.scss',
  templateUrl: './tour-map-search.html',
})
export class TourMapSearch {
  protected readonly form = form(signal({ ...EMPTY_FORM }), (path) => {
    min(path.lengthFrom, 0, { message: 'Granica ne može biti negativna.' });
    min(path.lengthTo, 0, { message: 'Granica ne može biti negativna.' });
    // The circle needs both the point and the radius, and the radius must be above zero.
    validate(path.radiusInKm, ({ value, valueOf }) => {
      const radius = this.kilometres(value());
      const point = valueOf(path.point);
      if (radius === null && point === null) {
        return null;
      }
      if (radius === null) {
        return { kind: 'radiusMissing', message: 'Unesite poluprečnik za izabranu tačku.' };
      }
      if (point === null) {
        return { kind: 'pointMissing', message: 'Izaberite tačku na mapi za ovaj poluprečnik.' };
      }
      return radius > 0
        ? null
        : { kind: 'positive', message: 'Poluprečnik mora biti veći od nule.' };
    });
  });

  // The criteria of the last search. The resource reads only these, so typing in the form or
  // clicking the map does not reload the tours; clicking "Pretraži" or choosing a sort does.
  private readonly applied = signal<PublishedTourSearchDto>(NO_CRITERIA);

  // Every new search, sort, or clearing starts again at the first page.
  protected readonly page = signal(1);
  private readonly pageSize = 20;

  protected readonly tours = httpResource<PageResult<PublishedTourDto>>(
    () => `/api/exploration/tours/published?${this.toQuery(this.applied(), this.page())}`,
  );

  protected readonly pageCount = computed(() =>
    Math.max(1, Math.ceil((this.tours.value()?.totalCount ?? 0) / this.pageSize)),
  );

  // The map shows the same tours as the list; a tour without a pin location has no pin.
  protected readonly pins = computed<MapLocation[]>(() => {
    if (!this.tours.hasValue()) {
      return [];
    }
    const pins: MapLocation[] = [];
    for (const item of this.tours.value().items) {
      if (item.pinLocation !== null) {
        pins.push({
          latitude: item.pinLocation.latitude,
          longitude: item.pinLocation.longitude,
          name: item.tour.name,
        });
      }
    }
    return pins;
  });

  // The text of the closed difficulty dropdown. Nothing checked means all difficulties.
  protected readonly difficultySummary = computed(() => {
    const checked: string[] = [];
    if (this.form.easy().value()) {
      checked.push('Lako');
    }
    if (this.form.moderate().value()) {
      checked.push('Srednje');
    }
    if (this.form.hard().value()) {
      checked.push('Teško');
    }
    return checked.length === 0 ? 'Sve' : checked.join(', ');
  });

  protected search(event: Event): void {
    event.preventDefault();
    if (!this.form().valid()) {
      this.form().markAsTouched();
      return;
    }
    this.applied.set(this.toDto());
    this.page.set(1);
  }

  // A new sort reloads the tours at once, with the criteria of the last search.
  protected sortChanged(): void {
    const sort = this.form.sort().value();
    this.applied.update((applied) => ({ ...applied, sort: sort === '' ? null : sort }));
    this.page.set(1);
  }

  protected changePage(change: number): void {
    const page = this.page() + change;
    if (page < 1 || page > this.pageCount()) {
      return;
    }
    this.page.set(page);
  }

  protected clear(): void {
    this.form().reset({ ...EMPTY_FORM });
    this.applied.set(NO_CRITERIA);
    this.page.set(1);
  }

  private toDto(): PublishedTourSearchDto {
    const name = this.form.name().value().trim();
    const difficulties: TourDifficulty[] = [];
    if (this.form.easy().value()) {
      difficulties.push('Easy');
    }
    if (this.form.moderate().value()) {
      difficulties.push('Moderate');
    }
    if (this.form.hard().value()) {
      difficulties.push('Hard');
    }
    const tags = this.form
      .tags()
      .value()
      .split(',')
      .map((tag) => tag.trim())
      .filter((tag) => tag !== '');
    const transport = this.form.transport().value();
    const sort = this.form.sort().value();
    // The form lets through only both or neither of the point and the radius.
    const point = this.form.point().value();
    return {
      name: name === '' ? null : name,
      difficulties: difficulties.length === 0 ? null : difficulties,
      tags: tags.length === 0 ? null : tags,
      transport: transport === '' ? null : transport,
      priceFrom: this.wholePrice(this.form.priceFrom().value()),
      priceTo: this.wholePrice(this.form.priceTo().value()),
      lengthFrom: this.kilometres(this.form.lengthFrom().value()),
      lengthTo: this.kilometres(this.form.lengthTo().value()),
      sort: sort === '' ? null : sort,
      latitude: point?.latitude ?? null,
      longitude: point?.longitude ?? null,
      radiusInKm: this.kilometres(this.form.radiusInKm().value()),
    };
  }

  // Prices are whole EC, so a typed fraction is cut off. An empty field means no bound.
  private wholePrice(value: number | null): number | null {
    return value === null || Number.isNaN(value) ? null : Math.trunc(value);
  }

  // Kilometres may have decimals, so they are sent as typed. An empty field means no value.
  private kilometres(value: number | null): number | null {
    return value === null || Number.isNaN(value) ? null : value;
  }

  // Empty criteria are left out of the URL.
  private toQuery(search: PublishedTourSearchDto, page: number): string {
    const query = new URLSearchParams({ page: String(page), pageSize: String(this.pageSize) });
    if (search.name !== null) {
      query.append('name', search.name);
    }
    for (const difficulty of search.difficulties ?? []) {
      query.append('difficulties', difficulty);
    }
    for (const tag of search.tags ?? []) {
      query.append('tags', tag);
    }
    if (search.transport !== null) {
      query.append('transport', search.transport);
    }
    if (search.priceFrom !== null) {
      query.append('priceFrom', String(search.priceFrom));
    }
    if (search.priceTo !== null) {
      query.append('priceTo', String(search.priceTo));
    }
    if (search.lengthFrom !== null) {
      query.append('lengthFrom', String(search.lengthFrom));
    }
    if (search.lengthTo !== null) {
      query.append('lengthTo', String(search.lengthTo));
    }
    if (search.sort !== null) {
      query.append('sort', search.sort);
    }
    if (search.latitude !== null) {
      query.append('latitude', String(search.latitude));
    }
    if (search.longitude !== null) {
      query.append('longitude', String(search.longitude));
    }
    if (search.radiusInKm !== null) {
      query.append('radiusInKm', String(search.radiusInKm));
    }
    return query.toString();
  }
}
