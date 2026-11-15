import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { AddToCartDto } from '../api/payment-api-types';

const BASE_URL = '/api/payment/cart';

@Injectable({ providedIn: 'root' })
export class ShoppingCart {
  private readonly http = inject(HttpClient);

  async addToCart(dto: AddToCartDto): Promise<void> {
    await firstValueFrom(this.http.post<void>(`${BASE_URL}/items`, dto));
  }

  async removeCartItem(itemId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/items/${itemId}`));
  }

  prepareCartCheckout(): Promise<string> {
    return firstValueFrom(this.http.post<string>(`${BASE_URL}/checkouts`, {}));
  }

  confirmCartCheckout(checkoutId: string): Promise<string> {
    return firstValueFrom(
      this.http.post<string>(`${BASE_URL}/checkouts/${checkoutId}/confirm`, {}),
    );
  }
}
