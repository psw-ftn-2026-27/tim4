import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { Auth } from './auth';

export type Role = 'explorer' | 'administrator';

// A route guard: who may open a page. Without a role, any signed-in user may. A guest goes to
// the login page and comes back after signing in; a signed-in user without the role sees
// "Nemate pristup". The server checks the same roles again on every request.
//
//   { path: 'cart', component: Cart, canActivate: [requireRole('explorer')] }
export function requireRole(role?: Role): CanActivateFn {
  return (_route, state) => {
    const user = inject(Auth).user();
    const router = inject(Router);
    if (user === null) {
      return router.createUrlTree(['/login'], { queryParams: { returnUrl: state.url } });
    }
    if (role === undefined || user.roles.includes(role)) {
      return true;
    }
    return router.createUrlTree(['/nema-pristupa']);
  };
}
