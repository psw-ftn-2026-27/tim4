import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import {
  PublishedTourDto,
  PublishedTourSearchDto,
  TourDifficulty,
  TourPreferenceDto,
  TourSort,
  TransportMode,
} from '../../api/exploration-api-types';
import { TourCard } from '../tour-card/tour-card';

const EMPTY_FORM = {
  name: '',
  easy: false,
  moderate: false,
  hard: false,
  tags: '',
  transport: '' as TransportMode | '',
  priceFrom: null as number | null,
  priceTo: null as number | null,
  sort: '' as TourSort | '',
};

const NO_CRITERIA: PublishedTourSearchDto = {
  name: null,
  difficulties: null,
  tags: null,
  transport: null,
  priceFrom: null,
  priceTo: null,
  sort: null,
};

@Component({
  imports: [FormField, RouterLink, TourCard],
  selector: 'app-tour-list',
  styleUrl: './tour-list.scss',
  templateUrl: './tour-list.html',
})
export class TourList {
  protected readonly auth = inject(Auth);

  protected readonly form = form(signal({ ...EMPTY_FORM }));

  // The criteria of the last search. The resource reads only these, so typing in the form
  // does not reload the list; clicking "Search" or choosing a sort does.
  private readonly applied = signal<PublishedTourSearchDto>(NO_CRITERIA);

  protected readonly tours = httpResource<PageResult<PublishedTourDto>>(
    () => `/api/exploration/tours/published?${this.toQuery(this.applied())}`,
  );

  // The server answers 204 No Content when the tourist has no preference, so the value is null.
  private readonly preference = httpResource<TourPreferenceDto | null>(
    () => '/api/exploration/preferences',
  );

  // The preference the "Use my preferences" button applies. Without one, when the tourist chose
  // to open the search with all tours, or when loading it failed (for example 403 for an
  // administrator), there is no button and no error message.
  protected readonly searchPreference = computed(() => {
    if (this.preference.error()) {
      return null;
    }
    const preference = this.preference.value();
    return preference?.openSearchWithPreferences ? preference : null;
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

  // True after "Use my preferences" until "Clear filters".
  protected readonly usingPreferences = signal(false);

  protected search(event: Event): void {
    event.preventDefault();
    this.applied.set(this.toDto());
  }

  // A new sort reloads the list at once, with the criteria of the last search.
  protected sortChanged(): void {
    const sort = this.form.sort().value();
    this.applied.update((applied) => ({ ...applied, sort: sort === '' ? null : sort }));
  }

  protected usePreferences(preference: TourPreferenceDto): void {
    this.form().reset({
      ...EMPTY_FORM,
      easy: preference.difficulties.includes('Easy'),
      moderate: preference.difficulties.includes('Moderate'),
      hard: preference.difficulties.includes('Hard'),
      tags: preference.tags.join(', '),
      transport: preference.transport ?? '',
    });
    this.applied.set(this.toDto());
    this.usingPreferences.set(true);
  }

  protected clear(): void {
    this.form().reset({ ...EMPTY_FORM });
    this.applied.set(NO_CRITERIA);
    this.usingPreferences.set(false);
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
    return {
      name: name === '' ? null : name,
      difficulties: difficulties.length === 0 ? null : difficulties,
      tags: tags.length === 0 ? null : tags,
      transport: transport === '' ? null : transport,
      priceFrom: this.wholePrice(this.form.priceFrom().value()),
      priceTo: this.wholePrice(this.form.priceTo().value()),
      sort: sort === '' ? null : sort,
    };
  }

  // Prices are whole EC, so a typed fraction is cut off. An empty field means no bound.
  private wholePrice(value: number | null): number | null {
    return value === null || Number.isNaN(value) ? null : Math.trunc(value);
  }

  // Every search starts at the first page. Empty criteria are left out of the URL.
  private toQuery(search: PublishedTourSearchDto): string {
    const query = new URLSearchParams({ page: '1', pageSize: '20' });
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
    if (search.sort !== null) {
      query.append('sort', search.sort);
    }
    return query.toString();
  }
}
