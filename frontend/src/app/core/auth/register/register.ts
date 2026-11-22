import { Component, computed, inject, input, signal } from '@angular/core';
import { FormField, email, form, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { Analytics } from '../../../shared/analytics/analytics';
import { serverMessage } from '../../../shared/util/server-message';
import { Auth } from '../auth';
import { safeReturnUrl } from '../login/login';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-register',
  styleUrls: ['../auth-layout.scss', './register.scss'],
  templateUrl: './register.html',
})
export class Register {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);
  private readonly analytics = inject(Analytics);

  readonly returnUrl = input<string>();

  protected readonly form = form(signal({ email: '', password: '' }), (path) => {
    required(path.email, { message: 'Unesite mejl.' });
    email(path.email, { message: 'Mejl nije ispravan.' });
  });

  // The password rules the server checks (the ASP.NET Identity defaults), shown while typing
  // instead of learned from the server's error.
  protected readonly rules = computed(() => {
    const password = this.form.password().value();
    return [
      { label: 'Najmanje 6 znakova', ok: password.length >= 6 },
      { label: 'Veliko slovo', ok: /[A-Z]/.test(password) },
      { label: 'Malo slovo', ok: /[a-z]/.test(password) },
      { label: 'Cifra', ok: /[0-9]/.test(password) },
      { label: 'Poseban znak (!, ?, # …)', ok: /[^A-Za-z0-9]/.test(password) },
    ];
  });
  protected readonly passwordOk = computed(() => this.rules().every((rule) => rule.ok));

  protected readonly showPassword = signal(false);
  protected readonly pending = signal(false);
  protected readonly error = signal<string | null>(null);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    if (!this.form().valid() || !this.passwordOk()) {
      this.form().markAsTouched();
      return;
    }
    this.pending.set(true);
    this.error.set(null);
    try {
      await this.auth.register(this.form.email().value(), this.form.password().value());
      this.analytics.track('sign_up', {});
      // Without a page to return to, a new tourist starts with the first steps.
      const returnUrl = safeReturnUrl(this.returnUrl());
      await this.router.navigateByUrl(returnUrl === '/' ? '/?novi=1' : returnUrl);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Registracija nije uspela. Pokušajte ponovo.'));
    } finally {
      this.pending.set(false);
    }
  }
}
