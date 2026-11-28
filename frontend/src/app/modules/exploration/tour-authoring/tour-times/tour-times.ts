import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form, min, required } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { formatMinutes } from '../../../../shared/util/format';
import { serverMessage } from '../../../../shared/util/server-message';
import { TourDto, TransportMode } from '../../api/exploration-api-types';
import { transportLabels } from '../../tour-browsing/tour-card/tour-card';
import { EditorSteps } from '../editor-steps/editor-steps';
import { TourAuthoring } from '../tour-authoring';

// The third step of the tour editor: how long the tour takes on foot, by bicycle or by car.
// A time can be added in any state of the tour; the server has no command to remove one yet.
@Component({
  imports: [EditorSteps, FormField, RouterLink],
  selector: 'app-tour-times',
  styleUrl: './tour-times.scss',
  templateUrl: './tour-times.html',
})
export class TourTimes {
  private readonly tourAuthoring = inject(TourAuthoring);

  // The tour ID from the route mine/:id/times.
  readonly id = input.required<string>();

  protected readonly tours = httpResource<TourDto[]>(() => '/api/exploration/tours/mine');
  protected readonly tour = computed(
    () => (this.tours.value() ?? []).find((tour) => tour.id === this.id()) ?? null,
  );

  protected readonly transportLabels = transportLabels;
  protected readonly formatMinutes = formatMinutes;

  protected readonly form = form(
    signal({ transport: 'Walking' as TransportMode, minutes: 30 as number | null }),
    (path) => {
      required(path.minutes, { message: 'Unesite broj minuta.' });
      min(path.minutes, 1, { message: 'Vreme mora biti bar 1 minut.' });
    },
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly notice = signal<string | null>(null);

  protected async add(event: Event): Promise<void> {
    event.preventDefault();
    if (!this.form().valid()) {
      this.form().markAsTouched();
      return;
    }
    const transport = this.form.transport().value();
    const minutes = this.form.minutes().value() ?? 0;
    this.pending.set(true);
    this.error.set(null);
    this.notice.set(null);
    try {
      await this.tourAuthoring.addTransportTime(this.id(), { transport, minutes });
      this.notice.set(`Dodato: ${transportLabels[transport]} ${formatMinutes(minutes)}.`);
      this.tours.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Vreme nije dodato. Pokušajte ponovo.'));
    } finally {
      this.pending.set(false);
    }
  }
}
