import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import { CartDto, WalletBalanceDto } from '../../api/payment-api-types';
import { CheckoutSteps } from '../checkout-steps/checkout-steps';
import { CartItems } from '../cart-items/cart-items';
import { CartCheckout } from '../cart-checkout';

@Component({
  selector: 'app-cart',
  imports: [CartItems, CheckoutSteps, RouterLink],
  templateUrl: './cart.html',
  styleUrl: './cart.scss',
})
export class Cart {
  private readonly cartCheckout = inject(CartCheckout);
  private readonly router = inject(Router);
  private readonly auth = inject(Auth);
  private readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);
  protected readonly cart = httpResource<CartDto>(() =>
    this.isExplorer() ? '/api/payment/cart' : undefined,
  );
  // Shown beside the total, so the tourist knows before paying whether the EC are enough.
  protected readonly wallet = httpResource<WalletBalanceDto>(() =>
    this.isExplorer() ? '/api/payment/wallet/balance' : undefined,
  );
  protected readonly balance = computed(() =>
    this.wallet.hasValue() ? this.wallet.value().availableBalance : null,
  );

  protected async remove(itemId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.success.set(null);
    try {
      await this.cartCheckout.removeCartItem(itemId);
      this.cart.reload();
      this.success.set('✓ Tura je uklonjena iz korpe.');
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Stavka nije uklonjena. Pokušajte ponovo.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected async checkout(): Promise<void> {
    if (this.pending() || !this.cart.value()?.items.length) {
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    this.success.set(null);
    try {
      const checkoutId = await this.cartCheckout.prepareCartCheckout();
      await this.router.navigate(['/payment/checkout', checkoutId]);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Plaćanje nije pripremljeno. Pokušajte ponovo.'));
    } finally {
      this.pending.set(false);
    }
  }

  protected reload(): void {
    this.error.set(null);
    this.success.set(null);
    this.cart.reload();
  }
}
