import { httpResource } from '@angular/common/http';
import { Component, computed, inject, input, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import { CartCheckoutDto } from '../../api/payment-api-types';
import { CartItems } from '../cart-items/cart-items';
import { ShoppingCart } from '../shopping-cart';

@Component({
  selector: 'app-checkout',
  imports: [CartItems, RouterLink],
  templateUrl: './checkout.html',
  styleUrl: './checkout.scss',
})
export class Checkout {
  private readonly shoppingCart = inject(ShoppingCart);
  private readonly router = inject(Router);
  readonly checkoutId = input.required<string>();
  protected readonly auth = inject(Auth);
  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly checkout = httpResource<CartCheckoutDto>(() =>
    this.isExplorer() ? `/api/payment/cart/checkouts/${this.checkoutId()}` : undefined,
  );
  protected readonly canPay = computed(() => {
    const checkout = this.checkout.value();
    return !!checkout && checkout.items.length > 0 && checkout.availableBalance >= checkout.total;
  });
  protected readonly loadError = computed(() =>
    serverMessage(
      this.checkout.error(),
      'Checkout nije moguce ucitati. Ako je korpa izmenjena, vrati se i pripremi novi checkout.',
    ),
  );

  protected async confirm(): Promise<void> {
    if (this.pending() || !this.canPay()) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      const purchaseId = await this.shoppingCart.confirmCartCheckout(this.checkoutId());
      await this.router.navigate(['/payment/purchases', purchaseId]);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Kupovina nije uspela. Pokusaj ponovo.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected reload(): void {
    this.error.set(null);
    this.checkout.reload();
  }
}
