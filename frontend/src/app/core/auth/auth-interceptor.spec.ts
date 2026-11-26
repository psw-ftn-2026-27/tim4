import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { signal } from '@angular/core';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { Auth } from './auth';
import { authInterceptor } from './auth-interceptor';

describe('authInterceptor', () => {
  const token = signal<string | null>('token-1');
  const logout = vi.fn(() => token.set(null));
  let http: HttpClient;
  let server: HttpTestingController;
  let navigate: ReturnType<typeof vi.spyOn>;

  beforeEach(() => {
    token.set('token-1');
    logout.mockClear();
    TestBed.configureTestingModule({
      providers: [
        provideRouter([]),
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: Auth, useValue: { accessToken: token, logout } },
      ],
    });
    http = TestBed.inject(HttpClient);
    server = TestBed.inject(HttpTestingController);
    navigate = vi.spyOn(TestBed.inject(Router), 'navigate').mockResolvedValue(true);
  });

  afterEach(() => {
    server.verify();
  });

  it('sends the token to our own server', () => {
    http.get('/api/payment/wallet').subscribe();

    const request = server.expectOne('/api/payment/wallet');
    expect(request.request.headers.get('Authorization')).toBe('Bearer token-1');
    request.flush({});
  });

  it('never sends the token to an outside service', () => {
    http.get('https://routing.openstreetmap.de/route').subscribe();

    const request = server.expectOne('https://routing.openstreetmap.de/route');
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush({});
  });

  it('sends no header when nobody is signed in', () => {
    token.set(null);
    http.get('/api/exploration/tours').subscribe();

    const request = server.expectOne('/api/exploration/tours');
    expect(request.request.headers.has('Authorization')).toBe(false);
    request.flush({});
  });

  it('signs out and opens the login page when the token has expired (401)', () => {
    http.get('/api/payment/wallet').subscribe({ error: () => undefined });

    server
      .expectOne('/api/payment/wallet')
      .flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(logout).toHaveBeenCalledTimes(1);
    expect(navigate).toHaveBeenCalledWith(['/login'], {
      queryParams: { returnUrl: '/', isteklo: 1 },
    });
  });

  it('signs out only once when several requests fail with the same token', () => {
    http.get('/api/payment/wallet').subscribe({ error: () => undefined });
    http.get('/api/payment/cart').subscribe({ error: () => undefined });

    server
      .expectOne('/api/payment/wallet')
      .flush(null, { status: 401, statusText: 'Unauthorized' });
    server.expectOne('/api/payment/cart').flush(null, { status: 401, statusText: 'Unauthorized' });

    expect(logout).toHaveBeenCalledTimes(1);
    expect(navigate).toHaveBeenCalledTimes(1);
  });

  it('keeps the user signed in on other errors, such as 403', () => {
    http.get('/api/games/admin').subscribe({ error: () => undefined });

    server.expectOne('/api/games/admin').flush(null, { status: 403, statusText: 'Forbidden' });

    expect(logout).not.toHaveBeenCalled();
    expect(navigate).not.toHaveBeenCalled();
  });
});
