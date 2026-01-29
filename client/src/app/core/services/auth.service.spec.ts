import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { vi } from 'vitest';
import { AuthService } from './auth.service';
import { TokenStorageService } from './token-storage.service';
import { AuthResponse, LoginRequest, RegisterRequest, User } from '../models/auth.models';
import { environment } from '../../../environments/environment';

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let tokenStorageMock: {
    getAccessToken: ReturnType<typeof vi.fn>;
    getRefreshToken: ReturnType<typeof vi.fn>;
    setTokens: ReturnType<typeof vi.fn>;
    clearTokens: ReturnType<typeof vi.fn>;
    hasTokens: ReturnType<typeof vi.fn>;
    isTokenExpiringSoon: ReturnType<typeof vi.fn>;
  };
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
    tokenStorageMock = {
      getAccessToken: vi.fn(),
      getRefreshToken: vi.fn(),
      setTokens: vi.fn(),
      clearTokens: vi.fn(),
      hasTokens: vi.fn(),
      isTokenExpiringSoon: vi.fn()
    };

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        AuthService,
        { provide: TokenStorageService, useValue: tokenStorageMock }
      ]
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('login', () => {
    it('should send login request and store tokens', () => {
      const loginRequest: LoginRequest = {
        email: 'test@example.com',
        password: 'Password1!'
      };

      service.login(loginRequest).subscribe(response => {
        expect(response).toEqual(mockAuthResponse);
        expect(tokenStorageMock.setTokens).toHaveBeenCalledWith(
          'test-access-token',
          'test-refresh-token'
        );
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
      tokenStorageMock.getRefreshToken.mockReturnValue('old-refresh-token');

      service.refreshToken().subscribe(response => {
        expect(response).toEqual(mockAuthResponse);
        expect(tokenStorageMock.setTokens).toHaveBeenCalledWith(
          'test-access-token',
          'test-refresh-token'
        );
      });

      const req = httpMock.expectOne(`${apiUrl}/refresh`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual({ refreshToken: 'old-refresh-token' });
      req.flush(mockAuthResponse);
    });

    it('should throw error when no refresh token exists', () => {
      tokenStorageMock.getRefreshToken.mockReturnValue(null);
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
    it('should clear tokens', () => {
      service.logout();
      expect(tokenStorageMock.clearTokens).toHaveBeenCalled();
    });
  });

  describe('getAccessToken', () => {
    it('should delegate to tokenStorage', () => {
      tokenStorageMock.getAccessToken.mockReturnValue('test-token');
      expect(service.getAccessToken()).toBe('test-token');
      expect(tokenStorageMock.getAccessToken).toHaveBeenCalled();
    });
  });

  describe('getRefreshToken', () => {
    it('should delegate to tokenStorage', () => {
      tokenStorageMock.getRefreshToken.mockReturnValue('test-refresh');
      expect(service.getRefreshToken()).toBe('test-refresh');
      expect(tokenStorageMock.getRefreshToken).toHaveBeenCalled();
    });
  });

  describe('isAuthenticated', () => {
    it('should return true when tokens exist', () => {
      tokenStorageMock.hasTokens.mockReturnValue(true);
      expect(service.isAuthenticated()).toBe(true);
    });

    it('should return false when no tokens exist', () => {
      tokenStorageMock.hasTokens.mockReturnValue(false);
      expect(service.isAuthenticated()).toBe(false);
    });
  });

  describe('isTokenExpiringSoon', () => {
    it('should delegate to tokenStorage', () => {
      tokenStorageMock.isTokenExpiringSoon.mockReturnValue(true);
      expect(service.isTokenExpiringSoon()).toBe(true);
      expect(tokenStorageMock.isTokenExpiringSoon).toHaveBeenCalled();
    });
  });
});
