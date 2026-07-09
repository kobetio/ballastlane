import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthResponse, CurrentUser, LoginRequest, RegisterRequest } from '../../models/auth.model';

interface StoredSession {
  token: string;
  user: CurrentUser;
}

/**
 * Owns the authenticated session: performs register/login HTTP calls, persists the JWT and
 * user profile to `localStorage` so a page refresh doesn't log the user out, and exposes the
 * current state as signals for components/guards/interceptor to read reactively.
 */
@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly storageKey = 'mylibrary.session';
  private readonly baseUrl = `${environment.apiBaseUrl}/auth`;

  private readonly session = signal<StoredSession | null>(this.readStoredSession());

  readonly currentUser = computed(() => this.session()?.user ?? null);
  readonly isAuthenticated = computed(() => this.session() !== null);

  get token(): string | null {
    return this.session()?.token ?? null;
  }

  register(request: RegisterRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/register`, request)
      .pipe(tap((response) => this.setSession(response)));
  }

  login(request: LoginRequest): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/login`, request)
      .pipe(tap((response) => this.setSession(response)));
  }

  logout(): void {
    localStorage.removeItem(this.storageKey);
    this.session.set(null);
  }

  private setSession(response: AuthResponse): void {
    const stored: StoredSession = {
      token: response.token,
      user: { id: response.userId, name: response.name, email: response.email }
    };
    localStorage.setItem(this.storageKey, JSON.stringify(stored));
    this.session.set(stored);
  }

  private readStoredSession(): StoredSession | null {
    const raw = localStorage.getItem(this.storageKey);
    if (!raw) {
      return null;
    }
    try {
      return JSON.parse(raw) as StoredSession;
    } catch {
      return null;
    }
  }
}
