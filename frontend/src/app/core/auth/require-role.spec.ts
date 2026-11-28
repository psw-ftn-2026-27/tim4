import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import {
  ActivatedRouteSnapshot,
  Router,
  RouterStateSnapshot,
  UrlTree,
  provideRouter,
} from '@angular/router';
import { Auth, User } from './auth';
import { requireRole } from './require-role';

describe('requireRole', () => {
  const user = signal<User | null>(null);

  beforeEach(() => {
    user.set(null);
    TestBed.configureTestingModule({
      providers: [provideRouter([]), { provide: Auth, useValue: { user } }],
    });
  });

  function open(role: Parameters<typeof requireRole>[0], url = '/payment/cart') {
    const state = { url } as RouterStateSnapshot;
    return TestBed.runInInjectionContext(() =>
      requireRole(role)({} as ActivatedRouteSnapshot, state),
    );
  }

  function address(result: unknown): string {
    expect(result).toBeInstanceOf(UrlTree);
    return TestBed.inject(Router).serializeUrl(result as UrlTree);
  }

  it('sends a guest to the login page, with the page to come back to', () => {
    expect(address(open('explorer', '/payment/cart'))).toBe('/login?returnUrl=%2Fpayment%2Fcart');
  });

  it('sends a guest to the login page even when the page needs no role', () => {
    expect(address(open(undefined, '/exploration/mine'))).toBe(
      '/login?returnUrl=%2Fexploration%2Fmine',
    );
  });

  it('lets in a user with the role', () => {
    user.set({ id: '1', email: 'explorer1@explorer.dev', roles: ['explorer'] });

    expect(open('explorer')).toBe(true);
  });

  it('lets in any signed-in user when the page needs no role', () => {
    user.set({ id: '2', email: 'admin1@explorer.dev', roles: ['administrator'] });

    expect(open(undefined)).toBe(true);
  });

  it('shows "Nemate pristup" to a signed-in user without the role', () => {
    user.set({ id: '2', email: 'admin1@explorer.dev', roles: ['administrator'] });

    expect(address(open('explorer'))).toBe('/nema-pristupa');
  });

  it('shows "Nemate pristup" to a user with no roles at all', () => {
    user.set({ id: '3', email: 'nobody@explorer.dev', roles: [] });

    expect(address(open('administrator'))).toBe('/nema-pristupa');
  });
});
