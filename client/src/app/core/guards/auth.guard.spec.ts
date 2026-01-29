import { TestBed } from '@angular/core/testing';
import { Router, UrlTree } from '@angular/router';
import { vi } from 'vitest';
import { authGuard, guestGuard } from './auth.guard';
import { AuthStateService } from '../../features/auth/services/auth-state.service';

describe('AuthGuard', () => {
  let authStateMock: {
    isAuthenticated: ReturnType<typeof vi.fn>;
  };
  let routerMock: {
    createUrlTree: ReturnType<typeof vi.fn>;
  };
  const mockUrlTree = {} as UrlTree;

  beforeEach(() => {
    authStateMock = {
      isAuthenticated: vi.fn()
    };
    routerMock = {
      createUrlTree: vi.fn().mockReturnValue(mockUrlTree)
    };

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        { provide: AuthStateService, useValue: authStateMock },
        { provide: Router, useValue: routerMock }
      ]
    });
  });

  describe('authGuard', () => {
    it('should allow access when authenticated', () => {
      authStateMock.isAuthenticated.mockReturnValue(true);

      const result = TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));

      expect(result).toBe(true);
    });

    it('should redirect to login when not authenticated', () => {
      authStateMock.isAuthenticated.mockReturnValue(false);

      const result = TestBed.runInInjectionContext(() => authGuard({} as any, {} as any));

      expect(routerMock.createUrlTree).toHaveBeenCalledWith(['/auth/login']);
      expect(result).toBe(mockUrlTree);
    });
  });

  describe('guestGuard', () => {
    it('should allow access when not authenticated', () => {
      authStateMock.isAuthenticated.mockReturnValue(false);

      const result = TestBed.runInInjectionContext(() => guestGuard({} as any, {} as any));

      expect(result).toBe(true);
    });

    it('should redirect to home when authenticated', () => {
      authStateMock.isAuthenticated.mockReturnValue(true);

      const result = TestBed.runInInjectionContext(() => guestGuard({} as any, {} as any));

      expect(routerMock.createUrlTree).toHaveBeenCalledWith(['/']);
      expect(result).toBe(mockUrlTree);
    });
  });
});
