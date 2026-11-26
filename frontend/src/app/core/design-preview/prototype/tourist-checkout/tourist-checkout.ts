import { Component, computed, inject, signal } from '@angular/core';
import { FormField, form } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { availableGiftCard, cartTourIds, tours, validCoupon, walletBalance } from '../mock';

// PROTOTYPE of scenario S2, step 2. What is paid on the left, with the coupon and the gift card
// under it; on the right the sum line by line and one "Plati" button. When the wallet does not
// cover the sum, the page says how much is missing and where EC come from.
@Component({
  imports: [FormField, RouterLink],
  selector: 'app-tourist-checkout',
  styleUrls: ['../checkout-steps.scss', './tourist-checkout.scss'],
  templateUrl: './tourist-checkout.html',
})
export class TouristCheckout {
  private readonly router = inject(Router);

  protected readonly giftCard = availableGiftCard;
  protected readonly items = cartTourIds.map((id) => tours.find((tour) => tour.id === id)!);
  protected readonly subtotal = this.items.reduce((sum, tour) => sum + tour.price, 0);

  // Prototype switch: the wallet as it is, or almost empty.
  protected readonly balance = signal(walletBalance);
  protected readonly lowBalance = 20;
  protected readonly fullBalance = walletBalance;

  protected readonly couponForm = form(signal({ code: '' }));
  protected readonly couponApplied = signal(false);
  protected readonly couponError = signal<string | null>(null);
  protected readonly useGiftCard = signal(false);
  protected readonly paying = signal(false);

  protected readonly couponTour = this.items.find((tour) => tour.id === validCoupon.tourId)!;
  protected readonly couponDiscount = Math.floor(
    (this.couponTour.price * validCoupon.discountPercent) / 100,
  );

  protected readonly afterCoupon = computed(
    () => this.subtotal - (this.couponApplied() ? this.couponDiscount : 0),
  );
  // The card is used whole and lowers the price after the coupon; what is left of it is lost.
  protected readonly giftDiscount = computed(() =>
    this.useGiftCard() ? Math.min(this.giftCard.amount, this.afterCoupon()) : 0,
  );
  protected readonly toPay = computed(() => this.afterCoupon() - this.giftDiscount());
  protected readonly missing = computed(() => Math.max(0, this.toPay() - this.balance()));

  protected applyCoupon(event: Event): void {
    event.preventDefault();
    const code = this.couponForm.code().value().trim().toUpperCase();
    if (code === '') {
      return;
    }
    if (code === validCoupon.code) {
      this.couponApplied.set(true);
      this.couponError.set(null);
      this.couponForm().reset({ code: '' });
    } else {
      this.couponError.set('Kupon sa ovim kodom ne postoji ili je istekao.');
    }
  }

  protected removeCoupon(): void {
    this.couponApplied.set(false);
  }

  protected async pay(): Promise<void> {
    this.paying.set(true);
    await new Promise((resolve) => setTimeout(resolve, 700));
    await this.router.navigate(['/design-preview/prototype/tourist/purchase'], {
      queryParams: { paid: this.toPay() },
    });
  }
}
