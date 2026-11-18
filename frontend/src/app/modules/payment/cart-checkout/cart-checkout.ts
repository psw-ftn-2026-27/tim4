import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { ApplyCouponDto, ConfirmCheckoutDto } from '../api/payment-api-types';

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

  async applyCoupon(checkoutId: string, dto: ApplyCouponDto): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/checkouts/${checkoutId}/coupon`, dto));
  }

  async removeCoupon(checkoutId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/checkouts/${checkoutId}/coupon`));
  }

  async useGiftCard(checkoutId: string): Promise<void> {
    await firstValueFrom(this.http.put<void>(`${BASE_URL}/checkouts/${checkoutId}/gift-card`, {}));
  }

  async removeGiftCard(checkoutId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/checkouts/${checkoutId}/gift-card`));
  }

  confirmCartCheckout(checkoutId: string, dto: ConfirmCheckoutDto): Promise<string> {
    return firstValueFrom(
      this.http.post<string>(`${BASE_URL}/checkouts/${checkoutId}/confirm`, dto),
    );
  }
}
