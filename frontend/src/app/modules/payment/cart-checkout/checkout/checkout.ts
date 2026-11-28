import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, form, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import { CartCheckoutDto } from '../../api/payment-api-types';
import { CartItems } from '../cart-items/cart-items';
import { CheckoutGiftCard } from '../checkout-gift-card/checkout-gift-card';
import { CheckoutSteps } from '../checkout-steps/checkout-steps';
import { CartCheckout } from '../cart-checkout';

@Component({
  selector: 'app-checkout',
  imports: [CartItems, CheckoutGiftCard, CheckoutSteps, FormField, RouterLink],
  templateUrl: './checkout.html',
  styleUrl: './checkout.scss',
})
export class Checkout {
  private readonly cartCheckout = inject(CartCheckout);
  private readonly router = inject(Router);
  readonly checkoutId = input.required<string>();
  private readonly auth = inject(Auth);
  private readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly couponError = signal<string | null>(null);
  protected readonly giftCardError = signal<string | null>(null);
  protected readonly couponForm = form(signal({ code: '' }), (path) => {
    required(path.code, { message: 'Unesite kod kupona.' });
  });
  protected readonly checkout = httpResource<CartCheckoutDto>(() =>
    this.isExplorer() ? `/api/payment/cart/checkouts/${this.checkoutId()}` : undefined,
  );
  protected readonly hasInvalidCoupon = computed(
    () => this.checkout.value()?.appliedCoupon?.isValid === false,
  );
  protected readonly hasInvalidGiftCard = computed(
    () => this.checkout.value()?.appliedGiftCard?.isValid === false,
  );
  protected readonly canPay = computed(() => {
    const checkout = this.checkout.value();
    return (
      !!checkout &&
      checkout.items.length > 0 &&
      checkout.availableBalance >= checkout.totalToPay &&
      !this.hasInvalidCoupon() &&
      !this.hasInvalidGiftCard()
    );
  });
  protected readonly couponProductName = computed(() => {
    const checkout = this.checkout.value();
    const productId = checkout?.appliedCoupon?.productId;
    return checkout?.items.find((item) => item.productId === productId)?.name ?? '';
  });
  // How many EC are missing, or 0 when the wallet covers the price.
  protected readonly missing = computed(() => {
    const checkout = this.checkout.value();
    return checkout === undefined
      ? 0
      : Math.max(0, checkout.totalToPay - checkout.availableBalance);
  });

  protected readonly loadError = computed(() =>
    serverMessage(
      this.checkout.error(),
      'Plaćanje nije učitano. Ako je korpa izmenjena, vratite se u korpu i nastavite ponovo.',
    ),
  );

  protected async applyCoupon(event: Event): Promise<void> {
    event.preventDefault();
    if (this.pending()) return;
    if (!this.couponForm().valid()) {
      this.couponForm().markAsTouched();
      return;
    }
    this.pending.set(true);
    this.couponError.set(null);
    this.error.set(null);
    try {
      await this.cartCheckout.applyCoupon(this.checkoutId(), {
        code: this.couponForm.code().value().trim(),
      });
      this.couponForm().reset({ code: '' });
      this.checkout.reload();
    } catch (failure) {
      this.couponError.set(serverMessage(failure, 'Kupon nije primenjen. Pokušajte ponovo.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async removeCoupon(): Promise<void> {
    if (this.pending()) return;
    this.pending.set(true);
    this.couponError.set(null);
    this.error.set(null);
    try {
      await this.cartCheckout.removeCoupon(this.checkoutId());
      this.checkout.reload();
    } catch (failure) {
      this.couponError.set(serverMessage(failure, 'Kupon nije uklonjen. Pokušajte ponovo.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async changeGiftCardUse(use: boolean): Promise<void> {
    if (this.pending()) return;
    this.pending.set(true);
    this.giftCardError.set(null);
    this.error.set(null);
    try {
      if (use) {
        await this.cartCheckout.useGiftCard(this.checkoutId());
      } else {
        await this.cartCheckout.removeGiftCard(this.checkoutId());
      }
    } catch (failure) {
      this.giftCardError.set(
        serverMessage(failure, 'Izmena poklon kartice nije uspela. Pokušajte ponovo.'),
      );
    } finally {
      this.pending.set(false);
      // Reloading shows the section again with the checkbox as the server saved it.
      this.checkout.reload();
    }
  }

  protected async confirm(): Promise<void> {
    const checkout = this.checkout.value();
    if (this.pending() || !this.canPay() || !checkout) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      const purchaseId = await this.cartCheckout.confirmCartCheckout(this.checkoutId(), {
        totalToPay: checkout.totalToPay,
      });
      await this.router.navigate(['/payment/purchases', purchaseId]);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Kupovina nije uspela. Pokušajte ponovo.'));
      this.checkout.reload();
    } finally {
      this.pending.set(false);
    }
  }

  protected reload(): void {
    this.error.set(null);
    this.couponError.set(null);
    this.giftCardError.set(null);
    this.checkout.reload();
  }
}
