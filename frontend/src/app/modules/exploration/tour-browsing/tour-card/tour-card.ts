import { Component, computed, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { formatKm, formatMinutes } from '../../../../shared/util/format';
import { TourDifficulty, TourDto, TransportMode } from '../../api/exploration-api-types';

const EXCERPT_LENGTH = 150;

export const difficultyLabels: Record<TourDifficulty, string> = {
  Easy: 'Lako',
  Moderate: 'Srednje',
  Hard: 'Teško',
};

export const difficultyBadges: Record<TourDifficulty, string> = {
  Easy: 'badge badge--easy',
  Moderate: 'badge badge--moderate',
  Hard: 'badge badge--hard',
};

export const transportLabels: Record<TransportMode, string> = {
  Walking: 'Peške',
  Bicycle: 'Biciklom',
  Car: 'Automobilom',
};

// One tour in the list beside the map: what it is on the left, and on the right what it is to
// this tourist, the tour in progress, a bought tour, their own tour or a price, with the one
// action that fits.
@Component({
  imports: [RouterLink],
  selector: 'app-tour-card',
  styleUrl: './tour-card.scss',
  templateUrl: './tour-card.html',
})
export class TourCard {
  readonly tour = input.required<TourDto>();
  // Null when the tour has no price yet; zero when it is free.
  readonly price = input.required<number | null>();
  readonly isInProgress = input(false);
  readonly isPurchased = input(false);
  readonly isOwn = input(false);
  // Highlighted while its pin is under the mouse or chosen on the map.
  readonly active = input(false);
  // False when the tour has no place on the map, so "Na mapi" has nothing to show.
  readonly hasPin = input(false);

  // "⌖ Na mapi": the page flies the map to the tour's pin.
  readonly showOnMap = output<void>();

  protected readonly difficultyLabels = difficultyLabels;
  protected readonly difficultyBadges = difficultyBadges;

  // The first 150 characters of the description, with "…" when it is longer.
  protected readonly excerpt = computed(() => {
    const description = this.tour().description;
    return description.length > EXCERPT_LENGTH
      ? `${description.slice(0, EXCERPT_LENGTH)}…`
      : description;
  });

  protected readonly length = computed(() => formatKm(this.tour().lengthInKm));

  protected readonly times = computed(() =>
    this.tour().transportTimes.map((time) => ({
      transport: time.transport,
      text: `${transportLabels[time.transport]} ${formatMinutes(time.minutes)}`,
    })),
  );
}
