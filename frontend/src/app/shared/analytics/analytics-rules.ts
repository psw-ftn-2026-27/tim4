import type { User } from '../../core/auth/auth';

export type ConsentChoice = 'undecided' | 'accepted' | 'rejected';

export type AnalyticsRole = 'guest' | 'explorer' | 'administrator';

// A stored value that is missing or unknown means the visitor has not chosen yet.
export function readChoice(stored: string | null): ConsentChoice {
  if (stored === 'accepted' || stored === 'rejected') {
    return stored;
  }
  return 'undecided';
}

// Something is sent only after the visitor accepted, and only when the environment has a
// measurement ID; locally and in tests it has none.
export function canSend(choice: ConsentChoice, measurementId: string | null): boolean {
  return choice === 'accepted' && measurementId !== null && measurementId !== '';
}

// The role as the system records it. A user with both roles is recorded as administrator.
export function roleOf(user: User | null): AnalyticsRole {
  if (user === null) {
    return 'guest';
  }
  return user.roles.includes('administrator') ? 'administrator' : 'explorer';
}

// The names of the analytics cookies in document.cookie: _ga and _ga_<ID>.
export function analyticsCookieNames(cookie: string): string[] {
  return cookie
    .split(';')
    .map((pair) => pair.split('=')[0].trim())
    .filter((name) => name === '_ga' || name.startsWith('_ga_'));
}
