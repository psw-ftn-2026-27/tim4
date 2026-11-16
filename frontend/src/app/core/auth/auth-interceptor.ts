import { HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Auth } from './auth';

// The token goes only to our own server, never to an outside service such as the map's routing.
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const token = inject(Auth).accessToken();
  if (token === null || !request.url.startsWith('/api/')) {
    return next(request);
  }
  return next(request.clone({ setHeaders: { Authorization: `Bearer ${token}` } }));
};
