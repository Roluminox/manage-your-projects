import { TestBed } from '@angular/core/testing';
import {
  HttpClient,
  HttpErrorResponse,
  provideHttpClient,
  withInterceptors
} from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { Router } from '@angular/router';
import { vi } from 'vitest';
import { authInterceptor } from './auth.interceptor';
import { AuthService } from '../services/auth.service';
import { of, throwError } from 'rxjs';

describe('AuthInterceptor', () => {
  let httpClient: HttpClient;
  let httpMock: HttpTestingController;
  let authServiceMock: {
    getAccessToken: ReturnType<typeof vi.fn>;
    getRefreshToken: ReturnType<typeof vi.fn>;
    refreshToken: ReturnType<typeof vi.fn>;
    logout: ReturnType<typeof vi.fn>;
  };
  let routerMock: {
    navigate: ReturnType<typeof vi.fn>;
  };

  beforeEach(() => {
    authServiceMock = {
      getAccessToken: vi.fn(),
      getRefreshToken: vi.fn(),
      refreshToken: vi.fn(),
      logout: vi.fn()
    };
    routerMock = {
      navigate: vi.fn()
    };

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(withInterceptors([authInterceptor])),
        provideHttpClientTesting(),
        { provide: AuthService, useValue: authServiceMock },
        { provide: Router, useValue: routerMock }
      ]
    });

    httpClient = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('token injection', () => {
    it('should add Authorization header when token exists', () => {
      authServiceMock.getAccessToken.mockReturnValue('test-token');

      httpClient.get('/api/data').subscribe();

      const req = httpMock.expectOne('/api/data');
      expect(req.request.headers.get('Authorization')).toBe('Bearer test-token');
      req.flush({});
    });

    it('should not add Authorization header when no token exists', () => {
      authServiceMock.getAccessToken.mockReturnValue(null);

      httpClient.get('/api/data').subscribe();

      const req = httpMock.expectOne('/api/data');
      expect(req.request.headers.has('Authorization')).toBe(false);
      req.flush({});
    });

    it('should not add Authorization header for login route', () => {
      authServiceMock.getAccessToken.mockReturnValue('test-token');

      httpClient.post('/api/auth/login', {}).subscribe();

      const req = httpMock.expectOne('/api/auth/login');
      expect(req.request.headers.has('Authorization')).toBe(false);
      req.flush({});
    });

    it('should not add Authorization header for register route', () => {
      authServiceMock.getAccessToken.mockReturnValue('test-token');

      httpClient.post('/api/auth/register', {}).subscribe();

      const req = httpMock.expectOne('/api/auth/register');
      expect(req.request.headers.has('Authorization')).toBe(false);
      req.flush({});
    });

    it('should not add Authorization header for refresh route', () => {
      authServiceMock.getAccessToken.mockReturnValue('test-token');

      httpClient.post('/api/auth/refresh', {}).subscribe();

      const req = httpMock.expectOne('/api/auth/refresh');
      expect(req.request.headers.has('Authorization')).toBe(false);
      req.flush({});
    });
  });

  describe('401 handling with token refresh', () => {
    it('should refresh token and retry request on 401', () => {
      authServiceMock.getAccessToken.mockReturnValue('expired-token');
      authServiceMock.getRefreshToken.mockReturnValue('refresh-token');
      authServiceMock.refreshToken.mockReturnValue(
        of({
          accessToken: 'new-token',
          refreshToken: 'new-refresh-token',
          user: { id: '1', email: 'test@example.com', username: 'test', displayName: 'Test' }
        })
      );

      let responseData: unknown;
      httpClient.get('/api/data').subscribe(data => {
        responseData = data;
      });

      // First request fails with 401
      const firstReq = httpMock.expectOne('/api/data');
      firstReq.flush(null, { status: 401, statusText: 'Unauthorized' });

      // After refresh, request is retried with new token
      const retryReq = httpMock.expectOne('/api/data');
      expect(retryReq.request.headers.get('Authorization')).toBe('Bearer new-token');
      retryReq.flush({ success: true });

      expect(responseData).toEqual({ success: true });
    });

    it('should logout and redirect when refresh fails', () => {
      authServiceMock.getAccessToken.mockReturnValue('expired-token');
      authServiceMock.getRefreshToken.mockReturnValue('invalid-refresh-token');
      authServiceMock.refreshToken.mockReturnValue(
        throwError(() => new HttpErrorResponse({ status: 401 }))
      );

      let errorCaught = false;
      httpClient.get('/api/data').subscribe({
        error: () => {
          errorCaught = true;
        }
      });

      const req = httpMock.expectOne('/api/data');
      req.flush(null, { status: 401, statusText: 'Unauthorized' });

      expect(authServiceMock.logout).toHaveBeenCalled();
      expect(routerMock.navigate).toHaveBeenCalledWith(['/auth/login']);
      expect(errorCaught).toBe(true);
    });

    it('should logout and redirect when no refresh token available', () => {
      authServiceMock.getAccessToken.mockReturnValue('expired-token');
      authServiceMock.getRefreshToken.mockReturnValue(null);

      let errorCaught = false;
      httpClient.get('/api/data').subscribe({
        error: () => {
          errorCaught = true;
        }
      });

      const req = httpMock.expectOne('/api/data');
      req.flush(null, { status: 401, statusText: 'Unauthorized' });

      expect(authServiceMock.logout).toHaveBeenCalled();
      expect(routerMock.navigate).toHaveBeenCalledWith(['/auth/login']);
      expect(errorCaught).toBe(true);
    });

    it('should not attempt refresh on 401 when no token was sent', () => {
      authServiceMock.getAccessToken.mockReturnValue(null);

      let errorCaught = false;
      httpClient.get('/api/data').subscribe({
        error: () => {
          errorCaught = true;
        }
      });

      const req = httpMock.expectOne('/api/data');
      req.flush(null, { status: 401, statusText: 'Unauthorized' });

      expect(authServiceMock.refreshToken).not.toHaveBeenCalled();
      expect(errorCaught).toBe(true);
    });
  });

  describe('non-401 errors', () => {
    it('should pass through 500 errors without refresh attempt', () => {
      authServiceMock.getAccessToken.mockReturnValue('test-token');

      let errorStatus: number | undefined;
      httpClient.get('/api/data').subscribe({
        error: (error: HttpErrorResponse) => {
          errorStatus = error.status;
        }
      });

      const req = httpMock.expectOne('/api/data');
      req.flush(null, { status: 500, statusText: 'Server Error' });

      expect(authServiceMock.refreshToken).not.toHaveBeenCalled();
      expect(errorStatus).toBe(500);
    });

    it('should pass through 404 errors without refresh attempt', () => {
      authServiceMock.getAccessToken.mockReturnValue('test-token');

      let errorStatus: number | undefined;
      httpClient.get('/api/data').subscribe({
        error: (error: HttpErrorResponse) => {
          errorStatus = error.status;
        }
      });

      const req = httpMock.expectOne('/api/data');
      req.flush(null, { status: 404, statusText: 'Not Found' });

      expect(authServiceMock.refreshToken).not.toHaveBeenCalled();
      expect(errorStatus).toBe(404);
    });
  });
});
