import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, throwError } from 'rxjs';

export const errorInterceptor: HttpInterceptorFn = (req, next) => {
  const router = inject(Router);

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      let errorMessage = 'An unexpected error occurred';

      switch (error.status) {
        case 0:
          errorMessage = 'Unable to connect to server. Please check your connection.';
          console.error('Network error:', error);
          break;

        case 401:
          errorMessage = 'Your session has expired. Please log in again.';
          localStorage.removeItem('myp-token');
          router.navigate(['/auth/login']);
          break;

        case 403:
          errorMessage = 'Access denied. You do not have permission to perform this action.';
          break;

        case 404:
          errorMessage = 'The requested resource was not found.';
          break;

        case 422:
          // Validation errors - extract message from response
          errorMessage = error.error?.message || 'Validation failed. Please check your input.';
          break;

        case 500:
          errorMessage = 'Server error. Please try again later.';
          console.error('Server error:', error);
          break;

        default:
          errorMessage = error.error?.message || `Error: ${error.status} - ${error.statusText}`;
          console.error('HTTP error:', error);
      }

      // In development, log all errors
      if (typeof window !== 'undefined' && (window as any).ngDevMode !== false) {
        console.error(`[HTTP Error] ${req.method} ${req.url}:`, {
          status: error.status,
          message: errorMessage,
          error: error.error
        });
      }

      return throwError(() => ({
        status: error.status,
        message: errorMessage,
        originalError: error
      }));
    })
  );
};
