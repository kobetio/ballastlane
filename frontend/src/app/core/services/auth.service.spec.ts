import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { TestBed } from '@angular/core/testing';
import { environment } from '../../../environments/environment';
import { AuthResponse } from '../../models/auth.model';
import { AuthService } from './auth.service';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;

  const authResponse: AuthResponse = {
    userId: '11111111-1111-1111-1111-111111111111',
    name: 'Jane Doe',
    email: 'jane@example.com',
    token: 'fake-jwt-token',
    expiresAtUtc: new Date().toISOString()
  };

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()]
    });
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('starts unauthenticated with no stored session', () => {
    service = TestBed.inject(AuthService);

    expect(service.isAuthenticated()).toBe(false);
    expect(service.currentUser()).toBeNull();
    expect(service.token).toBeNull();
  });

  it('login stores the session and updates signals on success', () => {
    service = TestBed.inject(AuthService);
    service.login({ email: 'jane@example.com', password: 'P@ssword123' }).subscribe();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/auth/login`);
    expect(req.request.method).toBe('POST');
    req.flush(authResponse);

    expect(service.isAuthenticated()).toBe(true);
    expect(service.currentUser()).toEqual({ id: authResponse.userId, name: authResponse.name, email: authResponse.email });
    expect(service.token).toBe(authResponse.token);
    expect(JSON.parse(localStorage.getItem('mylibrary.session')!).token).toBe(authResponse.token);
  });

  it('register stores the session and updates signals on success', () => {
    service = TestBed.inject(AuthService);
    service.register({ name: 'Jane Doe', email: 'jane@example.com', password: 'P@ssword123' }).subscribe();

    const req = httpMock.expectOne(`${environment.apiBaseUrl}/auth/register`);
    expect(req.request.method).toBe('POST');
    req.flush(authResponse);

    expect(service.isAuthenticated()).toBe(true);
  });

  it('logout clears the session', () => {
    service = TestBed.inject(AuthService);
    service.login({ email: 'jane@example.com', password: 'P@ssword123' }).subscribe();
    httpMock.expectOne(`${environment.apiBaseUrl}/auth/login`).flush(authResponse);

    service.logout();

    expect(service.isAuthenticated()).toBe(false);
    expect(service.currentUser()).toBeNull();
    expect(localStorage.getItem('mylibrary.session')).toBeNull();
  });

  it('restores a previously stored session on construction', () => {
    localStorage.setItem(
      'mylibrary.session',
      JSON.stringify({ token: 'stored-token', user: { id: '1', name: 'Stored User', email: 'stored@example.com' } })
    );

    const restoredService = TestBed.inject(AuthService);

    expect(restoredService.isAuthenticated()).toBe(true);
    expect(restoredService.token).toBe('stored-token');
  });
});
