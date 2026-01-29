import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { signal } from '@angular/core';
import { vi } from 'vitest';
import { LoginComponent } from './login.component';
import { AuthStateService } from '../../services/auth-state.service';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authStateMock: {
    loading: ReturnType<typeof signal<boolean>>;
    error: ReturnType<typeof signal<string | null>>;
    login: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    authStateMock = {
      loading: signal(false),
      error: signal<string | null>(null),
      login: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        provideRouter([]),
        { provide: AuthStateService, useValue: authStateMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  describe('form rendering', () => {
    it('should render email and password fields', () => {
      const emailInput = fixture.nativeElement.querySelector('input[type="email"]');
      const passwordInput = fixture.nativeElement.querySelector('input[type="password"]');

      expect(emailInput).toBeTruthy();
      expect(passwordInput).toBeTruthy();
    });

    it('should render submit button', () => {
      const submitBtn = fixture.nativeElement.querySelector('button[type="submit"]');

      expect(submitBtn).toBeTruthy();
      expect(submitBtn.textContent).toContain('Sign In');
    });

    it('should render register link', () => {
      const registerLink = fixture.nativeElement.querySelector('a[href="/auth/register"]');

      expect(registerLink).toBeTruthy();
    });
  });

  describe('form validation', () => {
    it('should show error when email is invalid', () => {
      const emailControl = component.form.controls.email;
      emailControl.setValue('invalid');
      emailControl.markAsTouched();
      fixture.detectChanges();

      expect(component.emailErrors).toBe('Invalid email format');
    });

    it('should show error when email is empty', () => {
      const emailControl = component.form.controls.email;
      emailControl.setValue('');
      emailControl.markAsTouched();
      fixture.detectChanges();

      expect(component.emailErrors).toBe('Email is required');
    });

    it('should show error when password is empty', () => {
      const passwordControl = component.form.controls.password;
      passwordControl.setValue('');
      passwordControl.markAsTouched();
      fixture.detectChanges();

      expect(component.passwordErrors).toBe('Password is required');
    });

    it('should not show errors when fields are valid', () => {
      component.form.controls.email.setValue('test@example.com');
      component.form.controls.password.setValue('password123');
      component.form.markAllAsTouched();
      fixture.detectChanges();

      expect(component.emailErrors).toBeNull();
      expect(component.passwordErrors).toBeNull();
    });
  });

  describe('form submission', () => {
    it('should call authState.login with form values on valid submit', () => {
      component.form.controls.email.setValue('test@example.com');
      component.form.controls.password.setValue('Password1!');

      component.onSubmit();

      expect(authStateMock.login).toHaveBeenCalledWith({
        email: 'test@example.com',
        password: 'Password1!'
      });
    });

    it('should not call authState.login when form is invalid', () => {
      component.form.controls.email.setValue('invalid');
      component.form.controls.password.setValue('');

      component.onSubmit();

      expect(authStateMock.login).not.toHaveBeenCalled();
    });

    it('should mark all fields as touched when submitting invalid form', () => {
      component.form.controls.email.setValue('');
      component.form.controls.password.setValue('');

      component.onSubmit();

      expect(component.form.controls.email.touched).toBe(true);
      expect(component.form.controls.password.touched).toBe(true);
    });
  });

  describe('loading state', () => {
    it('should disable submit button when loading', () => {
      authStateMock.loading.set(true);
      fixture.detectChanges();

      const submitBtn = fixture.nativeElement.querySelector('button[type="submit"]');

      expect(submitBtn.disabled).toBe(true);
      expect(submitBtn.textContent).toContain('Signing in');
    });

    it('should enable submit button when not loading', () => {
      authStateMock.loading.set(false);
      fixture.detectChanges();

      const submitBtn = fixture.nativeElement.querySelector('button[type="submit"]');

      expect(submitBtn.disabled).toBe(false);
    });
  });

  describe('error display', () => {
    it('should display error message when error exists', () => {
      authStateMock.error.set('Invalid credentials');
      fixture.detectChanges();

      const errorBanner = fixture.nativeElement.querySelector('.error-banner');

      expect(errorBanner).toBeTruthy();
      expect(errorBanner.textContent).toContain('Invalid credentials');
    });

    it('should not display error banner when no error', () => {
      authStateMock.error.set(null);
      fixture.detectChanges();

      const errorBanner = fixture.nativeElement.querySelector('.error-banner');

      expect(errorBanner).toBeFalsy();
    });
  });
});
