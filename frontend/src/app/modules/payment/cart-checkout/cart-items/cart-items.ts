import { Component, input, output } from '@angular/core';
import { CartItemDto } from '../../api/payment-api-types';

@Component({
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
