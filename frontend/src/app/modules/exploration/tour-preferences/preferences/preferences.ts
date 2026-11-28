import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { FormField, form } from '@angular/forms/signals';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourDifficulty, TourPreferenceDto, TransportMode } from '../../api/exploration-api-types';
import { difficultyLabels, transportLabels } from '../../tour-browsing/tour-card/tour-card';
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
  imports: [FormField, RouterLink],
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

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly notice = signal<string | null>(null);
  protected readonly confirmingDelete = signal(false);

  protected readonly form = form(signal({ ...EMPTY_FORM }));

  protected readonly editing = signal(false);
  protected readonly saved = computed(() =>
    this.preference.hasValue() ? this.preference.value() : null,
  );
  // Without a saved preference the form is open right away; with one, after "Izmeni".
  protected readonly showForm = computed(() => this.saved() === null || this.editing());

  protected readonly summary = computed(() => {
    const saved = this.saved();
    if (saved === null) {
      return null;
    }
    return {
      difficulty:
        saved.difficulties.length === 0
          ? 'svejedno'
          : saved.difficulties.map((difficulty) => difficultyLabels[difficulty]).join(', '),
      transport: saved.transport === null ? 'svejedno' : transportLabels[saved.transport],
      tags: saved.tags.length === 0 ? 'bez oznaka' : saved.tags.join(', '),
      opens: saved.openSearchWithPreferences
        ? 'pretraga se otvara sa vašim preferencama'
        : 'pretraga se otvara sa svim turama',
    };
  });

  protected startEdit(): void {
    const preference = this.saved();
    if (preference === null) {
      return;
    }
    this.error.set(null);
    this.notice.set(null);
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
    this.notice.set(null);
    try {
      await this.tourPreferences.save(this.toDto());
      this.notice.set('Preference su sačuvane.');
      this.editing.set(false);
      this.preference.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Preference nisu sačuvane. Pokušajte ponovo.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async remove(): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.notice.set(null);
    try {
      await this.tourPreferences.remove();
      this.confirmingDelete.set(false);
      this.notice.set('Preference su obrisane.');
      this.form().reset({ ...EMPTY_FORM });
      this.preference.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Preference nisu obrisane. Pokušajte ponovo.'));
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
