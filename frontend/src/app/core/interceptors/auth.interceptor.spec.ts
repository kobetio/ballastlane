import { HttpClient, provideHttpClient, withInterceptors } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router, provideRouter } from '@angular/router';
import { vi } from 'vitest';
import { environment } from '../../../environments/environment';
import { AuthService } from '../services/auth.service';
import { authInterceptor } from './auth.interceptor';

const loginUrl = `${environment.apiBaseUrl}/auth/login`;

describe('authInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let authService: AuthService;
  let router: Router;

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        provideRouter([])
      ]
    });

    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
    authService = TestBed.inject(AuthService);
    router = TestBed.inject(Router);
  });

  afterEach(() => httpMock.verify());

  it('does not attach an Authorization header when there is no token', () => {
    http.get('/api/books').subscribe();

    const req = httpMock.expectOne('/api/books');
    expect(req.request.headers.has('Authorization')).toBe(false);
    req.flush([]);
  });

  it('attaches a Bearer Authorization header when a token is present', () => {
    authService.login({ email: 'jane@example.com', password: 'P@ssword123' }).subscribe();
    httpMock.expectOne(loginUrl).flush({
      userId: '1',
      name: 'Jane',
      email: 'jane@example.com',
      token: 'fake-token',
      expiresAtUtc: new Date().toISOString()
    });

    http.get('/api/books').subscribe();

    const req = httpMock.expectOne('/api/books');
    expect(req.request.headers.get('Authorization')).toBe('Bearer fake-token');
    req.flush([]);
  });

  it('logs out and redirects to /login on a 401 from a non-auth endpoint', () => {
    const navigateSpy = vi.spyOn(router, 'navigate');

    http.get('/api/books').subscribe({ error: () => {} });
    httpMock.expectOne('/api/books').flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });

    expect(authService.isAuthenticated()).toBe(false);
    expect(navigateSpy).toHaveBeenCalledWith(['/login']);
  });

  it('does not log out on a 401 from the login endpoint itself', () => {
    const navigateSpy = vi.spyOn(router, 'navigate');

    authService.login({ email: 'jane@example.com', password: 'wrong' }).subscribe({ error: () => {} });
    httpMock.expectOne(loginUrl).flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });

    expect(navigateSpy).not.toHaveBeenCalled();
  });
});
