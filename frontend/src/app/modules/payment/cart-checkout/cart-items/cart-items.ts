import { Component, input, output } from '@angular/core';
import { RouterLink } from '@angular/router';
import { CartItemDto } from '../../api/payment-api-types';

// The tours of a cart or a checkout, one line each: the name links to the tour, the price on
// the right, and in the cart a quiet "Ukloni" under it.
@Component({
  imports: [RouterLink],
  selector: 'app-cart-items',
  templateUrl: './cart-items.html',
  styleUrl: './cart-items.scss',
})
export class CartItems {
  readonly items = input.required<CartItemDto[]>();
  readonly removable = input(false);
  readonly pending = input(false);
  readonly removeItem = output<string>();
}
