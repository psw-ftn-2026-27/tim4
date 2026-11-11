import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import {
  DifficultySort,
  PublishedTourSearchDto,
  TourDifficulty,
  TourDto,
  TourPreferenceDto,
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
  sortByDifficulty: '' as DifficultySort | '',
};

const NO_CRITERIA: PublishedTourSearchDto = {
  name: null,
  difficulties: null,
  tags: null,
  transport: null,
  sortByDifficulty: null,
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
  // does not reload the list; clicking "Search" does.
  private readonly applied = signal<PublishedTourSearchDto>(NO_CRITERIA);

  protected readonly tours = httpResource<PageResult<TourDto>>(
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
    const checked: TourDifficulty[] = [];
    if (this.form.easy().value()) {
      checked.push('Easy');
    }
    if (this.form.moderate().value()) {
      checked.push('Moderate');
    }
    if (this.form.hard().value()) {
      checked.push('Hard');
    }
    return checked.length === 0 ? 'All' : checked.join(', ');
  });

  // True after "Use my preferences" until "Clear filters".
  protected readonly usingPreferences = signal(false);

  protected search(event: Event): void {
    event.preventDefault();
    this.applied.set(this.toDto());
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
    const sortByDifficulty = this.form.sortByDifficulty().value();
    return {
      name: name === '' ? null : name,
      difficulties: difficulties.length === 0 ? null : difficulties,
      tags: tags.length === 0 ? null : tags,
      transport: transport === '' ? null : transport,
      sortByDifficulty: sortByDifficulty === '' ? null : sortByDifficulty,
    };
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
    if (search.sortByDifficulty !== null) {
      query.append('sortByDifficulty', search.sortByDifficulty);
    }
    return query.toString();
  }
}
