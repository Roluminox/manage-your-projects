import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { AuthService } from '../services/auth.service';

const PUBLIC_ROUTES = ['/auth/login', '/auth/register', '/auth/refresh'];

let isRefreshing = false;
const refreshTokenSubject = new BehaviorSubject<string | null>(null);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  const isPublicRoute = PUBLIC_ROUTES.some(route => req.url.includes(route));
  if (isPublicRoute) {
    return next(req);
  }

  const token = authService.getAccessToken();
  const authReq = token ? addTokenToRequest(req, token) : req;

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && token) {
        return handleUnauthorizedError(req, next, authService, router);
      }
      return throwError(() => error);
    })
  );
};

function addTokenToRequest(
  req: Parameters<HttpInterceptorFn>[0],
  token: string
): Parameters<HttpInterceptorFn>[0] {
  return req.clone({
    setHeaders: {
      Authorization: `Bearer ${token}`
    }
  });
}

function handleUnauthorizedError(
  req: Parameters<HttpInterceptorFn>[0],
  next: Parameters<HttpInterceptorFn>[1],
  authService: AuthService,
  router: Router
) {
  if (!isRefreshing) {
    isRefreshing = true;
    refreshTokenSubject.next(null);

    const refreshToken = authService.getRefreshToken();
    if (!refreshToken) {
      return handleRefreshFailure(authService, router);
    }

    return authService.refreshToken().pipe(
      switchMap(response => {
        isRefreshing = false;
        refreshTokenSubject.next(response.accessToken);
        return next(addTokenToRequest(req, response.accessToken));
      }),
      catchError(refreshError => {
        isRefreshing = false;
        refreshTokenSubject.next(null);
        return handleRefreshFailure(authService, router, refreshError);
      })
    );
  }

  return refreshTokenSubject.pipe(
    filter((token): token is string => token !== null),
    take(1),
    switchMap(token => next(addTokenToRequest(req, token)))
  );
}

function handleRefreshFailure(
  authService: AuthService,
  router: Router,
  error?: unknown
) {
  authService.logout();
  router.navigate(['/auth/login']);
  return throwError(() => error ?? new Error('Session expired'));
}
