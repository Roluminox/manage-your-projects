import { Injectable } from '@angular/core';

const ACCESS_TOKEN_KEY = 'myp-token';
const REFRESH_TOKEN_KEY = 'myp-refresh-token';

export interface TokenPayload {
  sub: string;
  email: string;
  exp: number;
  iat: number;
}

@Injectable({
  providedIn: 'root'
})
export class TokenStorageService {
  getAccessToken(): string | null {
    return localStorage.getItem(ACCESS_TOKEN_KEY);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(REFRESH_TOKEN_KEY);
  }

  setTokens(accessToken: string, refreshToken: string): void {
    localStorage.setItem(ACCESS_TOKEN_KEY, accessToken);
    localStorage.setItem(REFRESH_TOKEN_KEY, refreshToken);
  }

  clearTokens(): void {
    localStorage.removeItem(ACCESS_TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
  }

  hasTokens(): boolean {
    return !!this.getAccessToken();
  }

  isTokenExpired(): boolean {
    const token = this.getAccessToken();
    if (!token) return true;

    const payload = this.decodeToken(token);
    if (!payload) return true;

    const expirationDate = new Date(payload.exp * 1000);
    return expirationDate <= new Date();
  }

  isTokenExpiringSoon(thresholdSeconds = 60): boolean {
    const token = this.getAccessToken();
    if (!token) return true;

    const payload = this.decodeToken(token);
    if (!payload) return true;

    const expirationDate = new Date(payload.exp * 1000);
    const threshold = new Date(Date.now() + thresholdSeconds * 1000);
    return expirationDate <= threshold;
  }

  getTokenPayload(): TokenPayload | null {
    const token = this.getAccessToken();
    if (!token) return null;
    return this.decodeToken(token);
  }

  private decodeToken(token: string): TokenPayload | null {
    try {
      const parts = token.split('.');
      if (parts.length !== 3) return null;

      const payload = parts[1];
      const decoded = atob(payload.replace(/-/g, '+').replace(/_/g, '/'));
      return JSON.parse(decoded);
    } catch {
      return null;
    }
  }
}
