import { Injectable, inject } from '@angular/core';
import { NavigationEnd, Router } from '@angular/router';
import { environment } from '../../../environments/environment';
import { Auth } from '../../core/auth/auth';
import { analyticsCookieNames, canSend, roleOf } from './analytics-rules';
import { Consent } from './consent';
import { AnalyticsEventName, AnalyticsEvents } from './events';

// The parts of window that the official Google Analytics snippet uses.
interface GtagWindow {
  dataLayer: unknown[];
  gtag: (...args: unknown[]) => void;
}

// The only way to record an event: modules call track() and never Google Analytics
// directly. See README.md next to this file.
@Injectable({ providedIn: 'root' })
export class Analytics {
  private readonly consent = inject(Consent);
  private readonly auth = inject(Auth);
  private readonly router = inject(Router);
  private readonly measurementId = environment.gaMeasurementId;
  private loaded = false;

  constructor() {
    if (canSend(this.consent.choice(), this.measurementId)) {
      this.load();
    }
    // The one place outside shared/map that uses RxJS: a page view on every route change.
    this.router.events.subscribe((event) => {
      if (event instanceof NavigationEnd) {
        this.trackPageView();
      }
    });
  }

  accept(): void {
    this.consent.remember('accepted');
    if (this.measurementId === null) {
      return;
    }
    this.disabled(false);
    this.load();
    this.trackPageView();
  }

  reject(): void {
    this.consent.remember('rejected');
    if (this.measurementId !== null) {
      this.disabled(true);
    }
    this.deleteCookies();
  }

  // The × on the banner. Closing it before any choice counts as declining; once the
  // visitor has chosen, it only hides the banner and the choice stays as it was.
  dismiss(): void {
    if (this.consent.choice() === 'undecided') {
      this.reject();
    } else {
      this.consent.close();
    }
  }

  track<N extends AnalyticsEventName>(name: N, params: AnalyticsEvents[N]): void {
    if (!canSend(this.consent.choice(), this.measurementId)) {
      return;
    }
    this.window().gtag('event', name, { ...params, role: roleOf(this.auth.user()) });
  }

  private trackPageView(): void {
    this.track('page_view', { page_location: location.origin + this.router.url });
  }

  // The official snippet: a queue on window, then the script that reads it. Page views
  // are sent by trackPageView, so the script must not send its own on load.
  private load(): void {
    if (this.loaded || this.measurementId === null) {
      return;
    }
    this.loaded = true;
    const w = this.window();
    w.dataLayer = w.dataLayer ?? [];
    w.gtag = function () {
      // Google Analytics reads the arguments object itself, not an array.
      // eslint-disable-next-line prefer-rest-params
      w.dataLayer.push(arguments);
    };
    w.gtag('js', new Date());
    w.gtag('config', this.measurementId, { send_page_view: false });

    const script = document.createElement('script');
    script.async = true;
    script.src = `https://www.googletagmanager.com/gtag/js?id=${this.measurementId}`;
    document.head.appendChild(script);
  }

  // Google Analytics' own switch: while it is set, the loaded script sends nothing.
  private disabled(value: boolean): void {
    (window as unknown as Record<string, boolean>)[`ga-disable-${this.measurementId}`] = value;
  }

  // The cookies are set on the site's domain or one of its parents, so each is expired
  // on every candidate domain.
  private deleteCookies(): void {
    const parts = location.hostname.split('.');
    for (const name of analyticsCookieNames(document.cookie)) {
      document.cookie = `${name}=; Max-Age=0; path=/`;
      for (let i = 0; i < parts.length - 1; i++) {
        document.cookie = `${name}=; Max-Age=0; path=/; domain=.${parts.slice(i).join('.')}`;
      }
    }
  }

  private window(): GtagWindow {
    return window as unknown as GtagWindow;
  }
}
