import { Component, computed, input } from '@angular/core';
import { TourDto } from '../../api/exploration-api-types';

const EXCERPT_LENGTH = 150;

@Component({
  selector: 'app-tour-card',
  styleUrl: './tour-card.scss',
  templateUrl: './tour-card.html',
})
export class TourCard {
  readonly tour = input.required<TourDto>();
  // Null when the tour has no price yet; zero when it is free.
  readonly price = input.required<number | null>();

  // The first 150 characters of the description, with "..." when it is longer.
  protected readonly excerpt = computed(() => {
    const description = this.tour().description;
    return description.length > EXCERPT_LENGTH
      ? `${description.slice(0, EXCERPT_LENGTH)}...`
      : description;
  });
}
