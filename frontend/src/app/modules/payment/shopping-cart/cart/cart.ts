import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { Auth } from '../../../../core/auth/auth';
import { serverMessage } from '../../../../shared/util/server-message';
import { CartDto } from '../../api/payment-api-types';
import { CartItems } from '../cart-items/cart-items';
import { ShoppingCart } from '../shopping-cart';

@Component({
  selector: 'app-cart',
  imports: [CartItems, RouterLink],
  templateUrl: './cart.html',
  styleUrl: './cart.scss',
})
export class Cart {
  private readonly shoppingCart = inject(ShoppingCart);
  private readonly router = inject(Router);
  protected readonly auth = inject(Auth);
  protected readonly isExplorer = computed(
    () => this.auth.user()?.roles.includes('explorer') ?? false,
  );
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly success = signal<string | null>(null);
  protected readonly cart = httpResource<CartDto>(() =>
    this.isExplorer() ? '/api/payment/cart' : undefined,
  );

  protected async remove(itemId: string): Promise<void> {
    this.pending.set(true);
    this.error.set(null);
    this.success.set(null);
    try {
      await this.shoppingCart.removeCartItem(itemId);
      this.cart.reload();
      this.success.set('Stavka je uklonjena iz korpe.');
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Nije moguce ukloniti stavku. Pokusaj ponovo.'));
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
      const checkoutId = await this.shoppingCart.prepareCartCheckout();
      await this.router.navigate(['/payment/checkout', checkoutId]);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Nije moguce pripremiti checkout. Pokusaj ponovo.'));
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
