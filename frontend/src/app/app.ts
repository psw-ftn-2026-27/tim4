import { httpResource } from '@angular/common/http';
import { Component, computed, inject, signal } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Auth } from './core/auth/auth';
import type { ActiveTourDto } from './modules/exploration/api/exploration-api-types';
import type { CartDto, WalletBalanceDto } from './modules/payment/api/payment-api-types';
import { Analytics } from './shared/analytics/analytics';
import { BarRefresh } from './shared/bar/bar-refresh';
import { Consent } from './shared/analytics/consent';

@Component({
  imports: [RouterLink, RouterLinkActive, RouterOutlet],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {
  protected readonly auth = inject(Auth);
  protected readonly consent = inject(Consent);
  // Injected here so that page views are recorded from the first navigation on.
  protected readonly analytics = inject(Analytics);
  private readonly router = inject(Router);
  private readonly barRefresh = inject(BarRefresh);

  // Which of the three bar states to show. A user with both roles sees the administrator bar.
  protected readonly role = computed(() => {
    const user = this.auth.user();
    if (user === null) {
      return 'guest';
    }
    return user.roles.includes('administrator') ? 'administrator' : 'explorer';
  });

  // The part of the e-mail before "@", and its first letter for the round badge.
  protected readonly userName = computed(() => this.auth.user()?.email.split('@')[0] ?? '');
  protected readonly initial = computed(() => this.userName().charAt(0).toUpperCase());

  protected readonly userMenuOpen = signal(false);

  // The three live values of the tourist's bar: the cart count, the balance and the tour in
  // progress. The app shell is the one place outside a module that reads module endpoints
  // (frontend/AGENTS.md, "The app shell"). Each request repeats after every navigation and after
  // BarRefresh (shared/bar), which a command calls when it changes them without leaving the page.
  private readonly explorerUrl = (url: string) => () => {
    this.router.lastSuccessfulNavigation();
    this.barRefresh.changed();
    return this.role() === 'explorer' ? url : undefined;
  };

  private readonly cart = httpResource<CartDto>(this.explorerUrl('/api/payment/cart'));
  private readonly wallet = httpResource<WalletBalanceDto>(
    this.explorerUrl('/api/payment/wallet/balance'),
  );
  // 204 No Content when no tour is in progress, which reads as null.
  private readonly activeTour = httpResource<ActiveTourDto | null>(
    this.explorerUrl('/api/exploration/tour-executions/active'),
  );

  protected readonly cartCount = computed(() =>
    this.cart.hasValue() ? this.cart.value().items.length : 0,
  );
  protected readonly balance = computed(() =>
    this.wallet.hasValue() ? this.wallet.value().availableBalance : null,
  );
  protected readonly tourInProgress = computed(() => {
    const tour = this.activeTour.hasValue() ? this.activeTour.value() : null;
    return tour === null
      ? null
      : {
          name: tour.tourName,
          reached: tour.reachedKeyPointCount,
          points: pointsText(tour.keyPoints.length),
        };
  });

  protected toggleUserMenu(): void {
    this.userMenuOpen.update((open) => !open);
  }

  protected closeUserMenu(): void {
    this.userMenuOpen.set(false);
  }

  // After signing out the current page may need a signed-in user, so go to the home page.
  protected async logout(): Promise<void> {
    this.auth.logout();
    this.closeUserMenu();
    await this.router.navigateByUrl('/');
  }
}

// "2 tačke", "5 tačaka", "21 tačka".
function pointsText(count: number): string {
  const lastTwo = count % 100;
  const last = count % 10;
  if (last === 1 && lastTwo !== 11) {
    return `${count} tačka`;
  }
  if (last >= 2 && last <= 4 && (lastTwo < 12 || lastTwo > 14)) {
    return `${count} tačke`;
  }
  return `${count} tačaka`;
}
