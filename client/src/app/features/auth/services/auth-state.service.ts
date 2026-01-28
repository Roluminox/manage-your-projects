import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, finalize, of, tap } from 'rxjs';
import { AuthService, LoginRequest, RegisterRequest, User } from '../../../core';

@Injectable({
  providedIn: 'root'
})
export class AuthStateService {
  private readonly authService = inject(AuthService);
  private readonly router = inject(Router);

  private readonly userSignal = signal<User | null>(null);
  private readonly loadingSignal = signal(false);
  private readonly errorSignal = signal<string | null>(null);
  private readonly initializedSignal = signal(false);

  readonly user = this.userSignal.asReadonly();
  readonly loading = this.loadingSignal.asReadonly();
  readonly error = this.errorSignal.asReadonly();
  readonly initialized = this.initializedSignal.asReadonly();

  readonly isAuthenticated = computed(() => this.userSignal() !== null);

  initialize(): void {
    if (this.initializedSignal()) {
      return;
    }

    if (!this.authService.isAuthenticated()) {
      this.initializedSignal.set(true);
      return;
    }

    this.loadingSignal.set(true);
    this.authService.getCurrentUser().pipe(
      tap(user => {
        this.userSignal.set(user);
        this.initializedSignal.set(true);
      }),
      catchError(() => {
        this.authService.logout();
        this.initializedSignal.set(true);
        return of(null);
      }),
      finalize(() => this.loadingSignal.set(false))
    ).subscribe();
  }

  login(request: LoginRequest): void {
    this.loadingSignal.set(true);
    this.errorSignal.set(null);

    this.authService.login(request).pipe(
      tap(response => {
        this.userSignal.set(response.user);
        this.router.navigate(['/']);
      }),
      catchError(error => {
        this.errorSignal.set(error.error?.message || 'Login failed');
        return of(null);
      }),
      finalize(() => this.loadingSignal.set(false))
    ).subscribe();
  }

  register(request: RegisterRequest): void {
    this.loadingSignal.set(true);
    this.errorSignal.set(null);

    this.authService.register(request).pipe(
      tap(() => {
        this.router.navigate(['/auth/login']);
      }),
      catchError(error => {
        this.errorSignal.set(error.error?.message || 'Registration failed');
        return of(null);
      }),
      finalize(() => this.loadingSignal.set(false))
    ).subscribe();
  }

  logout(): void {
    this.authService.logout();
    this.userSignal.set(null);
    this.router.navigate(['/auth/login']);
  }

  clearError(): void {
    this.errorSignal.set(null);
  }
}
