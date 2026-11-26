import { Injectable, signal } from '@angular/core';

// The bar shows the cart count, the EC balance and the tour in progress, and reads them again
// after every navigation. A command that changes one of them while the user stays on the page
// (adding to the cart, removing from it, buying a gift card) calls refresh() in its group's
// service right after the command's await, like analytics.track(), so the bar is right at once.
@Injectable({ providedIn: 'root' })
export class BarRefresh {
  private readonly version = signal(0);
  readonly changed = this.version.asReadonly();

  refresh(): void {
    this.version.update((version) => version + 1);
  }
}
