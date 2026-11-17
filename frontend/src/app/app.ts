import { Component, computed, inject, signal } from '@angular/core';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { Auth } from './core/auth/auth';
import { Analytics } from './shared/analytics/analytics';
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

  // Which of the three bar states to show. A user with both roles sees the administrator bar.
  protected readonly role = computed(() => {
    const user = this.auth.user();
    if (user === null) {
      return 'guest';
    }
    return user.roles.includes('administrator') ? 'administrator' : 'explorer';
  });

  // Whether the item list is open on a narrow screen; a wide screen always shows it.
  protected readonly menuOpen = signal(false);

  protected toggleMenu(): void {
    this.menuOpen.update((open) => !open);
  }

  protected closeMenu(): void {
    this.menuOpen.set(false);
  }

  protected logout(): void {
    this.auth.logout();
    this.closeMenu();
  }
}
