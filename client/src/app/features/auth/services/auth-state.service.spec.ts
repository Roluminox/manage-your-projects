import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { of, throwError, delay } from 'rxjs';
import { vi } from 'vitest';
import { AuthService, User, AuthResponse, LoginRequest, RegisterRequest } from '../../../core';
import { AuthStateService } from './auth-state.service';

describe('AuthStateService', () => {
  let service: AuthStateService;
  let authServiceMock: {
    login: ReturnType<typeof vi.fn>;
    register: ReturnType<typeof vi.fn>;
    logout: ReturnType<typeof vi.fn>;
    getCurrentUser: ReturnType<typeof vi.fn>;
    isAuthenticated: ReturnType<typeof vi.fn>;
  };
  let routerMock: {
    navigate: ReturnType<typeof vi.fn>;
  };

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
    authServiceMock = {
      login: vi.fn(),
      register: vi.fn(),
      logout: vi.fn(),
      getCurrentUser: vi.fn(),
      isAuthenticated: vi.fn()
    };
    routerMock = {
      navigate: vi.fn()
    };

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        AuthStateService,
        { provide: AuthService, useValue: authServiceMock },
        { provide: Router, useValue: routerMock }
      ]
    });

    service = TestBed.inject(AuthStateService);
  });

  describe('initial state', () => {
    it('should have null user initially', () => {
      expect(service.user()).toBeNull();
    });

    it('should not be loading initially', () => {
      expect(service.loading()).toBe(false);
    });

    it('should not be authenticated initially', () => {
      expect(service.isAuthenticated()).toBe(false);
    });

    it('should not be initialized initially', () => {
      expect(service.initialized()).toBe(false);
    });
  });

  describe('initialize', () => {
    it('should set initialized when no token exists', () => {
      authServiceMock.isAuthenticated.mockReturnValue(false);

      service.initialize();

      expect(service.initialized()).toBe(true);
      expect(service.user()).toBeNull();
    });

    it('should fetch user when token exists', async () => {
      authServiceMock.isAuthenticated.mockReturnValue(true);
      authServiceMock.getCurrentUser.mockReturnValue(of(mockUser));

      service.initialize();
      await vi.waitFor(() => {
        expect(authServiceMock.getCurrentUser).toHaveBeenCalled();
      });

      expect(service.user()).toEqual(mockUser);
      expect(service.initialized()).toBe(true);
    });

    it('should logout when getCurrentUser fails', async () => {
      authServiceMock.isAuthenticated.mockReturnValue(true);
      authServiceMock.getCurrentUser.mockReturnValue(throwError(() => new Error('Unauthorized')));

      service.initialize();
      await vi.waitFor(() => {
        expect(authServiceMock.logout).toHaveBeenCalled();
      });

      expect(service.initialized()).toBe(true);
    });

    it('should not reinitialize if already initialized', () => {
      authServiceMock.isAuthenticated.mockReturnValue(false);

      service.initialize();
      service.initialize();

      expect(authServiceMock.isAuthenticated).toHaveBeenCalledTimes(1);
    });
  });

  describe('login', () => {
    const loginRequest: LoginRequest = {
      email: 'test@example.com',
      password: 'Password1!'
    };

    it('should set user on successful login', async () => {
      authServiceMock.login.mockReturnValue(of(mockAuthResponse));

      service.login(loginRequest);
      await vi.waitFor(() => {
        expect(service.user()).toEqual(mockUser);
      });

      expect(service.isAuthenticated()).toBe(true);
      expect(routerMock.navigate).toHaveBeenCalledWith(['/']);
    });

    it('should set error on failed login', async () => {
      authServiceMock.login.mockReturnValue(
        throwError(() => ({ error: { message: 'Invalid credentials' } }))
      );

      service.login(loginRequest);
      await vi.waitFor(() => {
        expect(service.error()).toBe('Invalid credentials');
      });

      expect(service.user()).toBeNull();
    });

    it('should set loading during login', () => {
      authServiceMock.login.mockReturnValue(of(mockAuthResponse).pipe(delay(100)));

      service.login(loginRequest);
      expect(service.loading()).toBe(true);
    });
  });

  describe('register', () => {
    const registerRequest: RegisterRequest = {
      email: 'test@example.com',
      username: 'testuser',
      password: 'Password1!',
      displayName: 'Test User'
    };

    it('should navigate to login on successful registration', async () => {
      authServiceMock.register.mockReturnValue(of({ userId: '123' }));

      service.register(registerRequest);
      await vi.waitFor(() => {
        expect(routerMock.navigate).toHaveBeenCalledWith(['/auth/login']);
      });
    });

    it('should set error on failed registration', async () => {
      authServiceMock.register.mockReturnValue(
        throwError(() => ({ error: { message: 'Email already exists' } }))
      );

      service.register(registerRequest);
      await vi.waitFor(() => {
        expect(service.error()).toBe('Email already exists');
      });
    });
  });

  describe('logout', () => {
    it('should clear user and navigate to login', async () => {
      authServiceMock.isAuthenticated.mockReturnValue(true);
      authServiceMock.getCurrentUser.mockReturnValue(of(mockUser));

      service.initialize();
      await vi.waitFor(() => {
        expect(service.user()).toEqual(mockUser);
      });

      service.logout();

      expect(authServiceMock.logout).toHaveBeenCalled();
      expect(service.user()).toBeNull();
      expect(service.isAuthenticated()).toBe(false);
      expect(routerMock.navigate).toHaveBeenCalledWith(['/auth/login']);
    });
  });

  describe('clearError', () => {
    it('should clear error', async () => {
      authServiceMock.login.mockReturnValue(
        throwError(() => ({ error: { message: 'Error' } }))
      );

      service.login({ email: 'test@test.com', password: 'test' });
      await vi.waitFor(() => {
        expect(service.error()).toBe('Error');
      });

      service.clearError();

      expect(service.error()).toBeNull();
    });
  });
});
