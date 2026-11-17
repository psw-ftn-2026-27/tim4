import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';

const BASE_URL = '/api/payment/cart';

@Injectable({ providedIn: 'root' })
export class CartCheckout {
  private readonly http = inject(HttpClient);

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
