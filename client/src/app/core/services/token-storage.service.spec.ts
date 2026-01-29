import { TestBed } from '@angular/core/testing';
import { TokenStorageService } from './token-storage.service';

describe('TokenStorageService', () => {
  let service: TokenStorageService;

  const createToken = (payload: object): string => {
    const header = btoa(JSON.stringify({ alg: 'HS256', typ: 'JWT' }));
    const body = btoa(JSON.stringify(payload));
    const signature = 'fake-signature';
    return `${header}.${body}.${signature}`;
  };

  const createValidToken = (expiresInSeconds = 3600): string => {
    return createToken({
      sub: 'user-123',
      email: 'test@example.com',
      exp: Math.floor(Date.now() / 1000) + expiresInSeconds,
      iat: Math.floor(Date.now() / 1000)
    });
  };

  const createExpiredToken = (): string => {
    return createToken({
      sub: 'user-123',
      email: 'test@example.com',
      exp: Math.floor(Date.now() / 1000) - 3600,
      iat: Math.floor(Date.now() / 1000) - 7200
    });
  };

  beforeEach(() => {
    localStorage.clear();
    TestBed.configureTestingModule({});
    service = TestBed.inject(TokenStorageService);
  });

  afterEach(() => {
    localStorage.clear();
  });

  describe('getAccessToken', () => {
    it('should return null when no token exists', () => {
      expect(service.getAccessToken()).toBeNull();
    });

    it('should return token when it exists', () => {
      localStorage.setItem('myp-token', 'test-token');
      expect(service.getAccessToken()).toBe('test-token');
    });
  });

  describe('getRefreshToken', () => {
    it('should return null when no refresh token exists', () => {
      expect(service.getRefreshToken()).toBeNull();
    });

    it('should return refresh token when it exists', () => {
      localStorage.setItem('myp-refresh-token', 'test-refresh');
      expect(service.getRefreshToken()).toBe('test-refresh');
    });
  });

  describe('setTokens', () => {
    it('should store both tokens in localStorage', () => {
      service.setTokens('access-token', 'refresh-token');

      expect(localStorage.getItem('myp-token')).toBe('access-token');
      expect(localStorage.getItem('myp-refresh-token')).toBe('refresh-token');
    });
  });

  describe('clearTokens', () => {
    it('should remove both tokens from localStorage', () => {
      localStorage.setItem('myp-token', 'access-token');
      localStorage.setItem('myp-refresh-token', 'refresh-token');

      service.clearTokens();

      expect(localStorage.getItem('myp-token')).toBeNull();
      expect(localStorage.getItem('myp-refresh-token')).toBeNull();
    });
  });

  describe('hasTokens', () => {
    it('should return false when no token exists', () => {
      expect(service.hasTokens()).toBe(false);
    });

    it('should return true when access token exists', () => {
      localStorage.setItem('myp-token', 'test-token');
      expect(service.hasTokens()).toBe(true);
    });
  });

  describe('isTokenExpired', () => {
    it('should return true when no token exists', () => {
      expect(service.isTokenExpired()).toBe(true);
    });

    it('should return true for expired token', () => {
      localStorage.setItem('myp-token', createExpiredToken());
      expect(service.isTokenExpired()).toBe(true);
    });

    it('should return false for valid token', () => {
      localStorage.setItem('myp-token', createValidToken());
      expect(service.isTokenExpired()).toBe(false);
    });

    it('should return true for invalid token format', () => {
      localStorage.setItem('myp-token', 'invalid-token');
      expect(service.isTokenExpired()).toBe(true);
    });
  });

  describe('isTokenExpiringSoon', () => {
    it('should return true when no token exists', () => {
      expect(service.isTokenExpiringSoon()).toBe(true);
    });

    it('should return true when token expires within threshold', () => {
      localStorage.setItem('myp-token', createValidToken(30));
      expect(service.isTokenExpiringSoon(60)).toBe(true);
    });

    it('should return false when token has plenty of time', () => {
      localStorage.setItem('myp-token', createValidToken(3600));
      expect(service.isTokenExpiringSoon(60)).toBe(false);
    });
  });

  describe('getTokenPayload', () => {
    it('should return null when no token exists', () => {
      expect(service.getTokenPayload()).toBeNull();
    });

    it('should return decoded payload for valid token', () => {
      const token = createValidToken();
      localStorage.setItem('myp-token', token);

      const payload = service.getTokenPayload();

      expect(payload).not.toBeNull();
      expect(payload?.sub).toBe('user-123');
      expect(payload?.email).toBe('test@example.com');
    });

    it('should return null for invalid token', () => {
      localStorage.setItem('myp-token', 'invalid');
      expect(service.getTokenPayload()).toBeNull();
    });
  });
});
