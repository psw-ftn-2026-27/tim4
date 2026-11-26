import { Component, computed, inject, signal } from '@angular/core';
import { FormField, email, form, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';

// PROTOTYPE of registration. Today the server's password rules (ASP.NET Identity defaults) are
// learned only from its error. Here they are a checklist that turns green while typing, and a new
// tourist lands on the start page in its "first steps" state.
@Component({
  imports: [FormField, RouterLink],
  selector: 'app-guest-register',
  styleUrls: ['../auth-layout.scss', './guest-register.scss'],
  templateUrl: './guest-register.html',
})
export class GuestRegister {
  private readonly router = inject(Router);

  protected readonly registerForm = form(signal({ email: '', password: '' }), (path) => {
    required(path.email, { message: 'Unesite mejl.' });
    email(path.email, { message: 'Mejl nije ispravan.' });
  });
  protected readonly showPassword = signal(false);
  protected readonly pending = signal(false);

  // The Identity defaults the server checks.
  protected readonly rules = computed(() => {
    const password = this.registerForm.password().value();
    return [
      { label: 'Najmanje 6 znakova', ok: password.length >= 6 },
      { label: 'Veliko slovo', ok: /[A-Z]/.test(password) },
      { label: 'Malo slovo', ok: /[a-z]/.test(password) },
      { label: 'Cifra', ok: /[0-9]/.test(password) },
      { label: 'Poseban znak (!, ?, # …)', ok: /[^A-Za-z0-9]/.test(password) },
    ];
  });
  protected readonly passwordOk = computed(() => this.rules().every((rule) => rule.ok));

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    if (!this.registerForm().valid() || !this.passwordOk()) {
      this.registerForm().markAsTouched();
      return;
    }
    this.pending.set(true);
    await new Promise((resolve) => setTimeout(resolve, 500));
    await this.router.navigate(['/design-preview/prototype/tourist'], {
      queryParams: { novi: 1 },
    });
  }
}
