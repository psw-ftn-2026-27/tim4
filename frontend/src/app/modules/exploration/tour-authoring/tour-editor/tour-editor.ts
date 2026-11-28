import { httpResource } from '@angular/common/http';
import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { formatKm, formatMinutes } from '../../../../shared/util/format';
import { TourDto } from '../../api/exploration-api-types';
import {
  difficultyBadges,
  difficultyLabels,
  transportLabels,
} from '../../tour-browsing/tour-card/tour-card';
import { EditorSteps, readiness } from '../editor-steps/editor-steps';

// The first step of the tour editor: what the tour is, and what it still lacks. The name and
// the description are set when the tour is created; the server has no command to change them
// yet, so this step only shows them.
@Component({
  imports: [EditorSteps, RouterLink],
  selector: 'app-tour-editor',
  styleUrl: './tour-editor.scss',
  templateUrl: './tour-editor.html',
})
export class TourEditor {
  // The tour ID from the route mine/:id.
  readonly id = input.required<string>();

  protected readonly tours = httpResource<TourDto[]>(() => '/api/exploration/tours/mine');
  protected readonly tour = computed(
    () => (this.tours.value() ?? []).find((tour) => tour.id === this.id()) ?? null,
  );

  protected readonly difficultyLabels = difficultyLabels;
  protected readonly difficultyBadges = difficultyBadges;

  protected readonly facts = computed(() => {
    const tour = this.tour();
    if (tour === null) {
      return [];
    }
    return [
      formatKm(tour.lengthInKm),
      `grupa ${tour.recommendedGroupSizeMin}–${tour.recommendedGroupSizeMax} osoba`,
      ...tour.transportTimes.map(
        (time) => `${transportLabels[time.transport]} ${formatMinutes(time.minutes)}`,
      ),
    ];
  });

  protected readonly checklist = computed(() => {
    const tour = this.tour();
    return tour === null ? [] : readiness(tour, null);
  });

  protected readonly descriptionLength = computed(() => this.tour()?.description.length ?? 0);
}
