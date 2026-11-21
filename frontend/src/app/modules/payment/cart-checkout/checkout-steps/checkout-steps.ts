import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

// The three steps of a purchase above the cart, the payment and the confirmation, so the
// tourist always sees where they are. A finished step is ticked; the cart can be reopened.
@Component({
  imports: [RouterLink],
  selector: 'app-checkout-steps',
  styleUrl: './checkout-steps.scss',
  templateUrl: './checkout-steps.html',
})
export class CheckoutSteps {
  // 1 cart, 2 payment, 3 confirmation.
  readonly step = input.required<1 | 2 | 3>();
  readonly centered = input(false);
}
