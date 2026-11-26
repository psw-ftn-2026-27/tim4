import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { tap } from 'rxjs';
import { Auth } from './auth';

// The token goes only to our own server, never to an outside service such as the map's routing.
// When the server answers 401 to a request that carried the token, the token has expired: the
// user is signed out and sent to the login page, which brings them back afterwards.
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(Auth);
  const router = inject(Router);
  const token = auth.accessToken();
  if (token === null || !request.url.startsWith('/api/')) {
    return next(request);
  }
  return next(request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })).pipe(
    tap({
      error: (failure: unknown) => {
        // Several requests can fail at once with the same token; only the first one, while the
        // user is still signed in, signs out and opens the login page.
        if (
          failure instanceof HttpErrorResponse &&
          failure.status === 401 &&
          auth.accessToken() !== null
        ) {
          auth.logout();
          // The page being opened, if the request belongs to a navigation, else the current one.
          const navigation = router.currentNavigation();
          const target = navigation?.finalUrl ?? navigation?.extractedUrl;
          const returnUrl = target ? router.serializeUrl(target) : router.url;
          void router.navigate(['/login'], { queryParams: { returnUrl, isteklo: 1 } });
        }
      },
    }),
  );
};
