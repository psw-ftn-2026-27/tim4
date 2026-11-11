import { httpResource } from '@angular/common/http';
import { Component, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { PageResult } from '../../../../shared/api/page-result';
import {
  DifficultySort,
  PublishedTourSearchDto,
  TourDifficulty,
  TourDto,
  TransportMode,
} from '../../api/exploration-api-types';
import { TourCard } from '../tour-card/tour-card';

const EMPTY_FORM = {
  name: '',
  difficulty: '' as TourDifficulty | '',
  tags: '',
  transport: '' as TransportMode | '',
  sortByDifficulty: '' as DifficultySort | '',
};

const NO_CRITERIA: PublishedTourSearchDto = {
  name: null,
  difficulty: null,
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

  protected search(event: Event): void {
    event.preventDefault();
    this.applied.set(this.toDto());
  }

  protected clear(): void {
    this.form().reset({ ...EMPTY_FORM });
    this.applied.set(NO_CRITERIA);
  }

  private toDto(): PublishedTourSearchDto {
    const name = this.form.name().value().trim();
    const difficulty = this.form.difficulty().value();
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
      difficulty: difficulty === '' ? null : difficulty,
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
    if (search.difficulty !== null) {
      query.append('difficulty', search.difficulty);
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
