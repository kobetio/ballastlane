import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

/**
 * Attaches the JWT (if any) to every outgoing request as `Authorization: Bearer {token}`,
 * and clears the session + redirects to `/login` whenever the API responds 401 — e.g. once
 * the token expires mid-session — so the user isn't left looking at a broken authenticated
 * page.
 */
export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const token = authService.token;
  const authorizedRequest = token
    ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : request;

  const isAuthEndpoint = request.url.includes('/auth/login') || request.url.includes('/auth/register');

  return next(authorizedRequest).pipe(
    catchError((error: unknown) => {
      // A 401 from the login/register endpoints just means "wrong credentials" or is otherwise
      // handled by the calling component; only treat 401s from other endpoints as "the session
      // expired", since there's nothing to log out of yet on those two.
      if (!isAuthEndpoint && error instanceof HttpErrorResponse && error.status === 401) {
        authService.logout();
        router.navigate(['/login']);
      }
      return throwError(() => error);
    })
  );
};
