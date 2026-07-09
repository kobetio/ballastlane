import { provideHttpClient } from '@angular/common/http';
import { provideHttpClientTesting } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, RouterStateSnapshot, provideRouter } from '@angular/router';
import { authGuard, guestGuard } from './auth.guard';

describe('authGuard / guestGuard', () => {
  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])]
    });
  });

  function setAuthenticated(): void {
    localStorage.setItem(
      'mylibrary.session',
      JSON.stringify({ token: 'fake-token', user: { id: '1', name: 'Jane', email: 'jane@example.com' } })
    );
  }

  function runGuard(guard: typeof authGuard, url = '/books') {
    return TestBed.runInInjectionContext(() =>
      guard({} as ActivatedRouteSnapshot, { url } as RouterStateSnapshot)
    );
  }

  it('authGuard allows navigation when authenticated', () => {
    setAuthenticated();

    expect(runGuard(authGuard)).toBe(true);
  });

  it('authGuard redirects to /login with a returnUrl when not authenticated', () => {
    const result = runGuard(authGuard, '/books');

    expect(result).not.toBe(true);
    expect(String(result)).toContain('/login');
    expect(String(result)).toContain('returnUrl');
  });

  it('guestGuard allows navigation when not authenticated', () => {
    expect(runGuard(guestGuard)).toBe(true);
  });

  it('guestGuard redirects to /books when already authenticated', () => {
    setAuthenticated();

    const result = runGuard(guestGuard);

    expect(result).not.toBe(true);
    expect(String(result)).toContain('/books');
  });
});
