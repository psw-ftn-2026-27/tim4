import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourDifficulty, TourPreferenceDto, TransportMode } from '../../api/exploration-api-types';
import { TourPreferences } from '../tour-preferences';

const EMPTY_FORM = {
  easy: false,
  moderate: false,
  hard: false,
  transport: '' as TransportMode | '',
  tags: '',
  searchOpens: 'all' as 'preferences' | 'all',
};

@Component({
  imports: [FormField],
  selector: 'app-preferences',
  styleUrl: './preferences.scss',
  templateUrl: './preferences.html',
})
export class Preferences {
  private readonly tourPreferences = inject(TourPreferences);

  // The server answers 204 No Content when the tourist has no preference, so the value is null.
  protected readonly preference = httpResource<TourPreferenceDto | null>(
    () => '/api/exploration/preferences',
  );

  protected readonly editing = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected readonly form = form(signal({ ...EMPTY_FORM }));

  // Without a saved preference the form is open right away; with one, only after "Edit".
  protected readonly showForm = computed(() => !this.preference.value() || this.editing());

  protected startEdit(preference: TourPreferenceDto): void {
    this.error.set(null);
    this.editing.set(true);
    this.form().reset({
      easy: preference.difficulties.includes('Easy'),
      moderate: preference.difficulties.includes('Moderate'),
      hard: preference.difficulties.includes('Hard'),
      transport: preference.transport ?? '',
      tags: preference.tags.join(', '),
      searchOpens: preference.openSearchWithPreferences ? 'preferences' : 'all',
    });
  }

  protected async save(event: Event): Promise<void> {
    event.preventDefault();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourPreferences.save(this.toDto());
      this.editing.set(false);
      this.preference.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not save the preferences.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async remove(): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourPreferences.remove();
      this.editing.set(false);
      this.form().reset({ ...EMPTY_FORM });
      this.preference.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Could not delete the preferences.'));
    } finally {
      this.pending.set(false);
    }
  }

  private toDto(): TourPreferenceDto {
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
    const transport = this.form.transport().value();
    return {
      difficulties,
      transport: transport === '' ? null : transport,
      tags: this.form
        .tags()
        .value()
        .split(',')
        .map((tag) => tag.trim())
        .filter((tag) => tag !== ''),
      openSearchWithPreferences: this.form.searchOpens().value() === 'preferences',
    };
  }
}
