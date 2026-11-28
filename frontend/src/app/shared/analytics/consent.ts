import { Injectable, computed, signal } from '@angular/core';
import { ConsentChoice, readChoice } from './analytics-rules';

const CHOICE_KEY = 'explorer.analytics-consent';

// The visitor's choice about analytics. It is kept in the browser, the same for a guest
// and a logged-in user, until the visitor changes it.
@Injectable({ providedIn: 'root' })
export class Consent {
  private readonly stored = signal<ConsentChoice>(readChoice(localStorage.getItem(CHOICE_KEY)));
  private readonly settingsOpen = signal(false);

  readonly choice = this.stored.asReadonly();
  readonly bannerOpen = computed(() => this.stored() === 'undecided' || this.settingsOpen());

  remember(choice: 'accepted' | 'rejected'): void {
    this.stored.set(choice);
    localStorage.setItem(CHOICE_KEY, choice);
    this.settingsOpen.set(false);
  }

  // Hides the banner opened from Cookie settings, without changing the choice.
  close(): void {
    this.settingsOpen.set(false);
  }

  // Shows the banner again with the current choice, without changing it.
  openSettings(): void {
    this.settingsOpen.set(true);
  }
}
