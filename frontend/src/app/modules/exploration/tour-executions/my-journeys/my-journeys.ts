import { httpResource } from '@angular/common/http';
import { Component, computed } from '@angular/core';
import { RouterLink } from '@angular/router';
import { ActiveTourDto } from '../../api/exploration-api-types';

// The tourist's own tours. Today the server knows only the tour in progress, so the page shows
// it, or how to start one. The list of bought tours and the history of finished ones come here
// once Exploration serves them (cards B1 and B4).
@Component({
  imports: [RouterLink],
  selector: 'app-my-journeys',
  styleUrl: './my-journeys.scss',
  templateUrl: './my-journeys.html',
})
export class MyJourneys {
  // 204 No Content when no tour is in progress reads as null.
  protected readonly activeTour = httpResource<ActiveTourDto | null>(
    () => '/api/exploration/tour-executions/active',
  );

  protected readonly active = computed(() =>
    this.activeTour.hasValue() ? this.activeTour.value() : null,
  );

  protected readonly next = computed(() => {
    const active = this.active();
    return active?.keyPoints.find((keyPoint) => keyPoint.id === active.nextKeyPointId) ?? null;
  });

  // "2 tačke", "5 tačaka".
  protected readonly pointsText = computed(() => {
    const count = this.active()?.keyPoints.length ?? 0;
    const last = count % 10;
    const lastTwo = count % 100;
    if (last === 1 && lastTwo !== 11) {
      return `${count} tačka`;
    }
    return last >= 2 && last <= 4 && (lastTwo < 12 || lastTwo > 14)
      ? `${count} tačke`
      : `${count} tačaka`;
  });

  protected readonly progress = computed(() => {
    const active = this.active();
    return active === null
      ? '0%'
      : `${(active.reachedKeyPointCount / active.keyPoints.length) * 100}%`;
  });
}
