import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { AuthService } from './auth.service';
import { AuthResponse, LoginRequest, RegisterRequest, User } from '../models/auth.models';
import { environment } from '../../../environments/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  const apiUrl = `${environment.apiUrl}/auth`;

  const mockUser: User = {
    id: '123',
    email: 'test@example.com',
    username: 'testuser',
    displayName: 'Test User'
  };

  const mockAuthResponse: AuthResponse = {
    accessToken: 'test-access-token',
    refreshToken: 'test-refresh-token',
    user: mockUser
  };

  beforeEach(() => {
    localStorage.clear();

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        AuthService
      ]
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  describe('login', () => {
    it('should send login request and store tokens', () => {
      const loginRequest: LoginRequest = {
        email: 'test@example.com',
        password: 'Password1!'
      };

      service.login(loginRequest).subscribe(response => {
        expect(response).toEqual(mockAuthResponse);
        expect(localStorage.getItem('myp-token')).toBe('test-access-token');
        expect(localStorage.getItem('myp-refresh-token')).toBe('test-refresh-token');
      });

      const req = httpMock.expectOne(`${apiUrl}/login`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(loginRequest);
      req.flush(mockAuthResponse);
    });
  });

  describe('register', () => {
    it('should send register request and return userId', () => {
      const registerRequest: RegisterRequest = {
        email: 'test@example.com',
        username: 'testuser',
        password: 'Password1!',
        displayName: 'Test User'
      };
      const registerResponse = { userId: '123' };

      service.register(registerRequest).subscribe(response => {
        expect(response).toEqual(registerResponse);
      });

      const req = httpMock.expectOne(`${apiUrl}/register`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(registerRequest);
      req.flush(registerResponse);
    });
  });

  describe('refreshToken', () => {
    it('should refresh tokens when refresh token exists', () => {
      localStorage.setItem('myp-refresh-token', 'old-refresh-token');

      service.refreshToken().subscribe(response => {
        expect(response).toEqual(mockAuthResponse);
        expect(localStorage.getItem('myp-token')).toBe('test-access-token');
        expect(localStorage.getItem('myp-refresh-token')).toBe('test-refresh-token');
      });

      const req = httpMock.expectOne(`${apiUrl}/refresh`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ refreshToken: 'old-refresh-token' });
      req.flush(mockAuthResponse);
    });

    it('should throw error when no refresh token exists', () => {
      expect(() => service.refreshToken()).toThrowError('No refresh token available');
    });
  });

  describe('getCurrentUser', () => {
    it('should fetch current user', () => {
      service.getCurrentUser().subscribe(user => {
        expect(user).toEqual(mockUser);
      });

      const req = httpMock.expectOne(`${apiUrl}/me`);
      expect(req.request.method).toBe('GET');
      req.flush(mockUser);
    });
  });

  describe('logout', () => {
    it('should clear tokens from localStorage', () => {
      localStorage.setItem('myp-token', 'test-token');
      localStorage.setItem('myp-refresh-token', 'test-refresh');

      service.logout();

      expect(localStorage.getItem('myp-token')).toBeNull();
      expect(localStorage.getItem('myp-refresh-token')).toBeNull();
    });
  });

  describe('getAccessToken', () => {
    it('should return token when it exists', () => {
      localStorage.setItem('myp-token', 'test-token');
      expect(service.getAccessToken()).toBe('test-token');
    });

    it('should return null when token does not exist', () => {
      expect(service.getAccessToken()).toBeNull();
    });
  });

  describe('getRefreshToken', () => {
    it('should return refresh token when it exists', () => {
      localStorage.setItem('myp-refresh-token', 'test-refresh');
      expect(service.getRefreshToken()).toBe('test-refresh');
    });

    it('should return null when refresh token does not exist', () => {
      expect(service.getRefreshToken()).toBeNull();
    });
  });

  describe('isAuthenticated', () => {
    it('should return true when token exists', () => {
      localStorage.setItem('myp-token', 'test-token');
      expect(service.isAuthenticated()).toBe(true);
    });

    it('should return false when token does not exist', () => {
      expect(service.isAuthenticated()).toBe(false);
    });
  });
});
