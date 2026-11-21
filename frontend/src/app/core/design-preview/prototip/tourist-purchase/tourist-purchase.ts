import { Component, computed, input } from '@angular/core';
import { RouterLink } from '@angular/router';
import { cartTourIds, difficultyClasses, difficultyLabels, tours, walletBalance } from '../mock';

// PROTOTYPE of scenario S2, step 3. The point of the screen is the next step, so each bought
// tour gets its own "Pokreni turu" and the receipt is folded away.
@Component({
  imports: [RouterLink],
  selector: 'app-tourist-purchase',
  styleUrls: ['../checkout-steps.scss', './tourist-purchase.scss'],
  templateUrl: './tourist-purchase.html',
})
export class TouristPurchase {
  // The sum the payment step sent, from the query string.
  readonly paid = input<string | undefined>(undefined);

  protected readonly difficultyLabels = difficultyLabels;
  protected readonly difficultyClasses = difficultyClasses;
  protected readonly items = cartTourIds.map((id) => tours.find((tour) => tour.id === id)!);
  protected readonly subtotal = this.items.reduce((sum, tour) => sum + tour.price, 0);
  protected readonly paidAmount = computed(() => Number(this.paid() ?? this.subtotal));
  protected readonly saved = computed(() => this.subtotal - this.paidAmount());
  protected readonly balanceAfter = computed(() => walletBalance - this.paidAmount());
}
