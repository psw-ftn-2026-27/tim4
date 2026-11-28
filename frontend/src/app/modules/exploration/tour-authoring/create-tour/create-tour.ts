import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form, min, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourDifficulty } from '../../api/exploration-api-types';
import { TourAuthoring } from '../tour-authoring';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-create-tour',
  styleUrl: './create-tour.scss',
  templateUrl: './create-tour.html',
})
export class CreateTour {
  private readonly tourAuthoring = inject(TourAuthoring);
  private readonly router = inject(Router);

  protected readonly form = form(
    signal({
      name: '',
      description: '',
      difficulty: 'Easy' as TourDifficulty,
      tags: '',
      groupSizeMin: 1,
      groupSizeMax: 10,
    }),
    (path) => {
      required(path.name, { message: 'Unesite naziv ture.' });
      required(path.description, { message: 'Unesite opis ture.' });
      required(path.groupSizeMin, { message: 'Unesite najmanju grupu.' });
      min(path.groupSizeMin, 1, { message: 'Grupa ima bar jednu osobu.' });
      required(path.groupSizeMax, { message: 'Unesite najveću grupu.' });
      min(path.groupSizeMax, 1, { message: 'Grupa ima bar jednu osobu.' });
    },
  );

  protected readonly descriptionLength = computed(() => this.form.description().value().length);

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    if (!this.form().valid()) {
      this.form().markAsTouched();
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      const tour = await this.tourAuthoring.create({
        name: this.form.name().value(),
        description: this.form.description().value(),
        difficulty: this.form.difficulty().value(),
        tags: this.form
          .tags()
          .value()
          .split(',')
          .map((tag) => tag.trim())
          .filter((tag) => tag !== ''),
        recommendedGroupSizeMin: this.form.groupSizeMin().value(),
        recommendedGroupSizeMax: this.form.groupSizeMax().value(),
      });
      // The next thing a new tour needs is its key points.
      await this.router.navigate(['/exploration/mine', tour.id, 'key-points']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Tura nije napravljena. Pokušajte ponovo.'));
    } finally {
      this.pending.set(false);
    }
  }
}
