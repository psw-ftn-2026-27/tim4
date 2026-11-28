import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { firstValueFrom } from 'rxjs';
import { BarRefresh } from '../../../shared/bar/bar-refresh';
import { ApplyCouponDto, ConfirmCheckoutDto } from '../api/payment-api-types';

const BASE_URL = '/api/payment/cart';

@Injectable({ providedIn: 'root' })
export class CartCheckout {
  private readonly http = inject(HttpClient);
  private readonly barRefresh = inject(BarRefresh);

  async removeCartItem(itemId: string): Promise<void> {
    await firstValueFrom(this.http.delete<void>(`${BASE_URL}/items/${itemId}`));
    // The cart count in the bar changes while the tourist stays in the cart.
    this.barRefresh.refresh();
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
