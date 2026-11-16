import { Component, inject } from '@angular/core';
import { RouterLink, RouterOutlet } from '@angular/router';
import { Auth } from './core/auth/auth';
import { Analytics } from './shared/analytics/analytics';
import { Consent } from './shared/analytics/consent';

@Component({
  imports: [RouterLink, RouterOutlet],
  selector: 'app-root',
  styleUrl: './app.scss',
  templateUrl: './app.html',
})
export class App {
  protected readonly auth = inject(Auth);
  protected readonly consent = inject(Consent);
  // Injected here so that page views are recorded from the first navigation on.
  protected readonly analytics = inject(Analytics);
}
