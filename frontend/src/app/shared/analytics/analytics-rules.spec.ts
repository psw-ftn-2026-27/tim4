import type { User } from '../../core/auth/auth';
import { analyticsCookieNames, canSend, readChoice, roleOf } from './analytics-rules';

describe('canSend', () => {
  it.each([
    ['undecided', 'G-TEST'],
    ['rejected', 'G-TEST'],
    ['accepted', null],
    ['accepted', ''],
  ] as const)('sends nothing when the choice is %s and the measurement ID is %s', (choice, id) => {
    expect(canSend(choice, id)).toBe(false);
  });

  it('sends once the visitor accepted and the environment has a measurement ID', () => {
    expect(canSend('accepted', 'G-TEST')).toBe(true);
  });
});

describe('roleOf', () => {
  const user = (roles: string[]): User => ({ id: '1', email: 'ana@example.com', roles });

  it('records a visitor who is not logged in as guest', () => {
    expect(roleOf(null)).toBe('guest');
  });

  it('records the role as the system stores it', () => {
    expect(roleOf(user(['explorer']))).toBe('explorer');
    expect(roleOf(user(['administrator']))).toBe('administrator');
  });

  it('records a user with both roles as administrator', () => {
    expect(roleOf(user(['explorer', 'administrator']))).toBe('administrator');
  });
});

describe('analyticsCookieNames', () => {
  it('finds only the analytics cookies and leaves the others alone', () => {
    const cookie = '_ga=GA1.1.123; explorer.theme=dark; _ga_ABC123=GS1.1.456; _gid=x; other=_ga';

    expect(analyticsCookieNames(cookie)).toEqual(['_ga', '_ga_ABC123']);
  });

  it('finds nothing when there are no cookies', () => {
    expect(analyticsCookieNames('')).toEqual([]);
  });
});

describe('readChoice', () => {
  it('reads a stored choice back', () => {
    expect(readChoice('accepted')).toBe('accepted');
    expect(readChoice('rejected')).toBe('rejected');
  });

  it('treats a missing or unknown value as not chosen yet, so the banner is shown', () => {
    expect(readChoice(null)).toBe('undecided');
    expect(readChoice('true')).toBe('undecided');
  });
});
