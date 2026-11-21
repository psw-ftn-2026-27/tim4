import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, email, form, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { Analytics } from '../../../shared/analytics/analytics';
import { Auth } from '../auth';

// Where to go after signing in: the page that sent the user here (requireRole or an expired
// session), but only an address of this app, never one that leads elsewhere.
export function safeReturnUrl(returnUrl: string | undefined): string {
  return returnUrl !== undefined && returnUrl.startsWith('/') && !returnUrl.startsWith('//')
    ? returnUrl
    : '/';
}

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-login',
  styleUrl: '../auth-layout.scss',
  templateUrl: './login.html',
})
export class Login {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);
  private readonly analytics = inject(Analytics);

  // Query parameters: ?returnUrl=/payment/cart and ?isteklo=1 after an expired session.
  readonly returnUrl = input<string>();
  readonly isteklo = input<string>();

  protected readonly expired = computed(() => this.isteklo() !== undefined);

  protected readonly form = form(signal({ email: '', password: '' }), (path) => {
    required(path.email, { message: 'Unesite mejl.' });
    email(path.email, { message: 'Mejl nije ispravan.' });
    required(path.password, { message: 'Unesite lozinku.' });
  });

  protected readonly showPassword = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    if (!this.form().valid()) {
      this.form().markAsTouched();
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.auth.login(this.form.email().value(), this.form.password().value());
      this.analytics.track('login', {});
      await this.router.navigateByUrl(safeReturnUrl(this.returnUrl()));
    } catch {
      // Never which of the two was wrong.
      this.error.set('Mejl ili lozinka nisu tačni. Pokušajte ponovo.');
    } finally {
      this.pending.set(false);
    }
  }
}
