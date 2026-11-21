import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form, required, validate } from '@angular/forms/signals';
import { RouterLink } from '@angular/router';
import { serverMessage } from '../../../../shared/util/server-message';
import { KeyPointDto, TourDto, TourPricingType } from '../../api/exploration-api-types';
import { EditorSteps, readiness } from '../editor-steps/editor-steps';
import { TourAuthoring } from '../tour-authoring';

// The last step of the tour editor: the price, and publishing once everything is ready. The
// checklist repeats the server's rules, so "Objavi" is offered only when it will succeed.
@Component({
  imports: [EditorSteps, FormField, RouterLink],
  selector: 'app-tour-publish',
  styleUrl: './tour-publish.scss',
  templateUrl: './tour-publish.html',
})
export class TourPublish {
  private readonly tourAuthoring = inject(TourAuthoring);

  // The tour ID from the route mine/:id/publish.
  readonly id = input.required<string>();

  protected readonly tours = httpResource<TourDto[]>(() => '/api/exploration/tours/mine');
  protected readonly keyPoints = httpResource<KeyPointDto[]>(
    () => `/api/exploration/tours/mine/${this.id()}/key-points`,
  );
  protected readonly tour = computed(
    () => (this.tours.value() ?? []).find((tour) => tour.id === this.id()) ?? null,
  );
  protected readonly keyPointCount = computed(() =>
    this.keyPoints.hasValue() ? this.keyPoints.value().length : null,
  );

  protected readonly checklist = computed(() => {
    const tour = this.tour();
    return tour === null ? [] : readiness(tour, this.keyPointCount());
  });
  protected readonly ready = computed(() => this.checklist().every((item) => item.done));

  protected readonly form = form(
    signal({ pricingType: 'Free' as TourPricingType, amount: null as number | null }),
    (path) => {
      required(path.pricingType, { message: 'Izaberite da li je tura besplatna ili se plaća.' });
      // A paid tour costs a whole number of EC above zero; a free tour ignores the amount.
      validate(path.amount, ({ value, valueOf }) => {
        if (valueOf(path.pricingType) !== 'Paid') {
          return undefined;
        }
        const amount = value();
        return amount !== null && Number.isInteger(amount) && amount > 0
          ? undefined
          : { kind: 'wholePositive', message: 'Cena mora biti ceo broj EC veći od nule.' };
      });
    },
  );

  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly published = signal(false);

  protected async publish(event: Event): Promise<void> {
    event.preventDefault();
    if (!this.form().valid()) {
      this.form().markAsTouched();
      return;
    }
    const pricingType = this.form.pricingType().value();
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.tourAuthoring.publish(this.id(), {
        pricingType,
        // A free tour sends no amount, even if one was typed while "Plaća se" was chosen.
        pricingAmount: pricingType === 'Paid' ? this.form.amount().value() : null,
      });
      this.published.set(true);
      this.tours.reload();
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Tura nije objavljena. Pokušajte ponovo.'));
    } finally {
      this.pending.set(false);
    }
  }
}
