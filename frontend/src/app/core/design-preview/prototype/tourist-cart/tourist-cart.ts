import { Component, computed, signal } from '@angular/core';
import { RouterLink } from '@angular/router';
import {
  ProtoTour,
  cartTourIds,
  difficultyClasses,
  difficultyLabels,
  tours,
  walletBalance,
} from '../mock';

// PROTOTYPE of scenario S2, step 1. The cart is a list on the left and a summary on the right
// with the one way forward. A removed tour can be put back from the notice, so "Ukloni" needs no
// confirmation.
@Component({
  imports: [RouterLink],
  selector: 'app-tourist-cart',
  styleUrls: ['../checkout-steps.scss', './tourist-cart.scss'],
  templateUrl: './tourist-cart.html',
})
export class TouristCart {
  protected readonly difficultyLabels = difficultyLabels;
  protected readonly difficultyClasses = difficultyClasses;
  protected readonly balance = walletBalance;

  protected readonly ids = signal<string[]>([...cartTourIds]);
  protected readonly removed = signal<ProtoTour | null>(null);

  protected readonly items = computed(() =>
    this.ids().map((id) => tours.find((tour) => tour.id === id)!),
  );
  protected readonly total = computed(() =>
    this.items().reduce((sum, tour) => sum + tour.price, 0),
  );

  protected remove(tour: ProtoTour): void {
    this.ids.update((ids) => ids.filter((id) => id !== tour.id));
    this.removed.set(tour);
  }

  protected undo(): void {
    const tour = this.removed();
    if (tour !== null) {
      this.ids.update((ids) => [...ids, tour.id]);
      this.removed.set(null);
    }
  }

  protected emptyCart(): void {
    this.ids.set([]);
    this.removed.set(null);
  }
}
