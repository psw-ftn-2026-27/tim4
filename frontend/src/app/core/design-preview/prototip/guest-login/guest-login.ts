import { Component, inject, signal } from '@angular/core';
import { FormField, email, form, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';

// PROTOTYPE of signing in. Serbian, a password that can be shown, one error for a wrong pair
// (never which of the two was wrong), and after signing in each role lands on its own start page.
@Component({
  imports: [FormField, RouterLink],
  selector: 'app-guest-login',
  styleUrl: '../auth-layout.scss',
  templateUrl: './guest-login.html',
})
export class GuestLogin {
  private readonly router = inject(Router);

  protected readonly loginForm = form(signal({ email: '', password: '' }), (path) => {
    required(path.email, { message: 'Unesite mejl.' });
    email(path.email, { message: 'Mejl nije ispravan.' });
    required(path.password, { message: 'Unesite lozinku.' });
  });
  protected readonly showPassword = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly pending = signal(false);

  protected async submit(event: Event): Promise<void> {
    event.preventDefault();
    if (!this.loginForm().valid()) {
      this.loginForm().markAsTouched();
      return;
    }
    const value = this.loginForm().value();
    this.error.set(null);
    this.pending.set(true);
    await new Promise((resolve) => setTimeout(resolve, 500));
    this.pending.set(false);
    // Prototype: any password but "pogresna" signs in; an admin1... address opens the admin app.
    if (value.password === 'pogresna') {
      this.error.set('Mejl ili lozinka nisu tačni. Pokušajte ponovo.');
      return;
    }
    await this.router.navigateByUrl(
      value.email.startsWith('admin')
        ? '/design-preview/prototype/admin'
        : '/design-preview/prototype/tourist',
    );
  }
}
