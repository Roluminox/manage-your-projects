import { HttpInterceptorFn } from '@angular/common/http';

const PUBLIC_ROUTES = [
  '/api/auth/login',
  '/api/auth/register',
  '/api/auth/refresh'
];

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  // Skip auth header for public routes
  const isPublicRoute = PUBLIC_ROUTES.some(route => req.url.includes(route));
  if (isPublicRoute) {
    return next(req);
  }

  const token = localStorage.getItem('myp-token');

  if (token) {
    const clonedRequest = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
    return next(clonedRequest);
  }

  return next(req);
};
