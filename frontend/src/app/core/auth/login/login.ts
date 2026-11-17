import { Component, inject, signal } from '@angular/core';
import { FormField, email, form, required } from '@angular/forms/signals';
import { Router, RouterLink } from '@angular/router';
import { Analytics } from '../../../shared/analytics/analytics';
import { serverMessage } from '../../../shared/util/server-message';
import { Auth } from '../auth';

@Component({
  imports: [FormField, RouterLink],
  selector: 'app-login',
  styleUrl: './login.scss',
  templateUrl: './login.html',
})
export class Login {
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);
  private readonly analytics = inject(Analytics);

  protected readonly form = form(signal({ email: '', password: '' }), (path) => {
    required(path.email, { message: 'Email is required.' });
    email(path.email, { message: 'Enter a valid email address.' });
    required(path.password, { message: 'Password is required.' });
  });

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
      await this.router.navigate(['/']);
    } catch (failure) {
      this.error.set(serverMessage(failure, 'Login failed. Please try again.'));
    } finally {
      this.pending.set(false);
    }
  }
}
