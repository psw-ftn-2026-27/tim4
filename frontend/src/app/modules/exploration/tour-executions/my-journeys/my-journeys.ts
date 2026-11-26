import { httpResource } from '@angular/common/http';
import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import { LocationMap, MapLocation } from '../../../../shared/map/location-map';
import { formatKm, formatProgress } from '../../../../shared/util/format';
import { ActiveTourDto, PurchasedTourDto } from '../../api/exploration-api-types';
import { difficultyBadges, difficultyLabels } from '../../tour-browsing/tour-card/tour-card';

// The tourist's own tours: the tour in progress first, then every tour they bought, beside a map
// of where each one starts. The history of finished tours comes here once Exploration serves it
// (card B4).
@Component({
  imports: [LocationMap, RouterLink],
  selector: 'app-my-journeys',
  styleUrl: './my-journeys.scss',
  templateUrl: './my-journeys.html',
})
export class MyJourneys {
  // 204 No Content when no tour is in progress reads as null.
  protected readonly activeTour = httpResource<ActiveTourDto | null>(
    () => '/api/exploration/tour-executions/active',
  );

  // Every tour the tourist bought, free ones and archived ones included. Without purchases the
  // server sends an empty list, not an error.
  protected readonly purchased = httpResource<PurchasedTourDto[]>(
    () => '/api/exploration/tours/purchased',
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
      : formatProgress(active.reachedKeyPointCount, active.keyPoints.length);
  });

  protected readonly items = computed(() =>
    this.purchased.hasValue() ? this.purchased.value() : [],
  );

  // Every bought tour has a pin, at the place where the tour starts, so pins and cards share
  // their order.
  protected readonly pins = computed<MapLocation[]>(() =>
    this.items().map((item) => ({
      latitude: item.pinLocation.latitude,
      longitude: item.pinLocation.longitude,
      name: item.tour.name,
      tone: item.isInProgress ? 'next' : 'owned',
    })),
  );

  // The card or pin under the mouse, and the tour whose pin or "Na mapi" was clicked.
  protected readonly hovered = signal<string | null>(null);
  protected readonly chosen = signal<string | null>(null);
  protected readonly activeId = computed(() => this.hovered() ?? this.chosen());
  protected readonly activeIndex = computed(() => this.pinIndex(this.activeId()));
  protected readonly focusIndex = computed(() => this.pinIndex(this.chosen()));

  protected readonly difficultyLabels = difficultyLabels;
  protected readonly difficultyBadges = difficultyBadges;

  protected readonly formatKm = formatKm;

  protected hoverPin(index: number | null): void {
    this.hovered.set(index === null ? null : (this.items()[index]?.tour.id ?? null));
  }

  protected choosePin(index: number): void {
    this.chosen.set(this.items()[index]?.tour.id ?? null);
  }

  private pinIndex(tourId: string | null): number | null {
    const index = this.items().findIndex((item) => item.tour.id === tourId);
    return index === -1 ? null : index;
  }
}
