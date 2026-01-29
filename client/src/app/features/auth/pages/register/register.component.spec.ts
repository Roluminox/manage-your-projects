import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideRouter } from '@angular/router';
import { signal } from '@angular/core';
import { vi } from 'vitest';
import { RegisterComponent } from './register.component';
import { AuthStateService } from '../../services/auth-state.service';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;
  let authStateMock: {
    loading: ReturnType<typeof signal<boolean>>;
    error: ReturnType<typeof signal<string | null>>;
    register: ReturnType<typeof vi.fn>;
  };

  beforeEach(async () => {
    authStateMock = {
      loading: signal(false),
      error: signal<string | null>(null),
      register: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [RegisterComponent],
      providers: [
        provideRouter([]),
        { provide: AuthStateService, useValue: authStateMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  describe('form rendering', () => {
    it('should render all form fields', () => {
      const emailInput = fixture.nativeElement.querySelector('#email');
      const usernameInput = fixture.nativeElement.querySelector('#username');
      const displayNameInput = fixture.nativeElement.querySelector('#displayName');
      const passwordInput = fixture.nativeElement.querySelector('#password');
      const confirmPasswordInput = fixture.nativeElement.querySelector('#confirmPassword');

      expect(emailInput).toBeTruthy();
      expect(usernameInput).toBeTruthy();
      expect(displayNameInput).toBeTruthy();
      expect(passwordInput).toBeTruthy();
      expect(confirmPasswordInput).toBeTruthy();
    });

    it('should render submit button', () => {
      const submitBtn = fixture.nativeElement.querySelector('button[type="submit"]');

      expect(submitBtn).toBeTruthy();
      expect(submitBtn.textContent).toContain('Create Account');
    });

    it('should render login link', () => {
      const loginLink = fixture.nativeElement.querySelector('a[href="/auth/login"]');

      expect(loginLink).toBeTruthy();
    });
  });

  describe('email validation', () => {
    it('should show error when email is empty', () => {
      const control = component.form.controls.email;
      control.setValue('');
      control.markAsTouched();

      expect(component.emailErrors).toBe('Email is required');
    });

    it('should show error when email format is invalid', () => {
      const control = component.form.controls.email;
      control.setValue('invalid-email');
      control.markAsTouched();

      expect(component.emailErrors).toBe('Invalid email format');
    });

    it('should not show error when email is valid', () => {
      const control = component.form.controls.email;
      control.setValue('test@example.com');
      control.markAsTouched();

      expect(component.emailErrors).toBeNull();
    });
  });

  describe('username validation', () => {
    it('should show error when username is empty', () => {
      const control = component.form.controls.username;
      control.setValue('');
      control.markAsTouched();

      expect(component.usernameErrors).toBe('Username is required');
    });

    it('should show error when username is too short', () => {
      const control = component.form.controls.username;
      control.setValue('ab');
      control.markAsTouched();

      expect(component.usernameErrors).toBe('Username must be at least 3 characters');
    });

    it('should not show error when username is valid', () => {
      const control = component.form.controls.username;
      control.setValue('johndoe');
      control.markAsTouched();

      expect(component.usernameErrors).toBeNull();
    });
  });

  describe('displayName validation', () => {
    it('should show error when displayName is empty', () => {
      const control = component.form.controls.displayName;
      control.setValue('');
      control.markAsTouched();

      expect(component.displayNameErrors).toBe('Display name is required');
    });

    it('should not show error when displayName is provided', () => {
      const control = component.form.controls.displayName;
      control.setValue('John Doe');
      control.markAsTouched();

      expect(component.displayNameErrors).toBeNull();
    });
  });

  describe('password validation', () => {
    it('should show error when password is empty', () => {
      const control = component.form.controls.password;
      control.setValue('');
      control.markAsTouched();

      expect(component.passwordErrors).toBe('Password is required');
    });

    it('should show error when password is too short', () => {
      const control = component.form.controls.password;
      control.setValue('Short1!');
      control.markAsTouched();

      expect(component.passwordErrors).toBe('Password must be at least 8 characters');
    });

    it('should show error when password has no uppercase', () => {
      const control = component.form.controls.password;
      control.setValue('password1!');
      control.markAsTouched();

      expect(component.passwordErrors).toBe('Password must contain an uppercase letter');
    });

    it('should show error when password has no digit', () => {
      const control = component.form.controls.password;
      control.setValue('Password!');
      control.markAsTouched();

      expect(component.passwordErrors).toBe('Password must contain a digit');
    });

    it('should show error when password has no special character', () => {
      const control = component.form.controls.password;
      control.setValue('Password1');
      control.markAsTouched();

      expect(component.passwordErrors).toBe('Password must contain a special character');
    });

    it('should not show error when password is valid', () => {
      const control = component.form.controls.password;
      control.setValue('Password1!');
      control.markAsTouched();

      expect(component.passwordErrors).toBeNull();
    });
  });

  describe('confirmPassword validation', () => {
    it('should show error when confirmPassword is empty', () => {
      const control = component.form.controls.confirmPassword;
      control.setValue('');
      control.markAsTouched();

      expect(component.confirmPasswordErrors).toBe('Please confirm your password');
    });

    it('should show error when passwords do not match', () => {
      component.form.controls.password.setValue('Password1!');
      component.form.controls.confirmPassword.setValue('DifferentPassword1!');
      component.form.controls.confirmPassword.markAsTouched();

      expect(component.confirmPasswordErrors).toBe('Passwords do not match');
    });

    it('should not show error when passwords match', () => {
      component.form.controls.password.setValue('Password1!');
      component.form.controls.confirmPassword.setValue('Password1!');
      component.form.controls.confirmPassword.markAsTouched();

      expect(component.confirmPasswordErrors).toBeNull();
    });
  });

  describe('form submission', () => {
    it('should call authState.register with form values on valid submit', () => {
      component.form.controls.email.setValue('test@example.com');
      component.form.controls.username.setValue('testuser');
      component.form.controls.displayName.setValue('Test User');
      component.form.controls.password.setValue('Password1!');
      component.form.controls.confirmPassword.setValue('Password1!');

      component.onSubmit();

      expect(authStateMock.register).toHaveBeenCalledWith({
        email: 'test@example.com',
        username: 'testuser',
        displayName: 'Test User',
        password: 'Password1!'
      });
    });

    it('should not include confirmPassword in register call', () => {
      component.form.controls.email.setValue('test@example.com');
      component.form.controls.username.setValue('testuser');
      component.form.controls.displayName.setValue('Test User');
      component.form.controls.password.setValue('Password1!');
      component.form.controls.confirmPassword.setValue('Password1!');

      component.onSubmit();

      const registerCall = authStateMock.register.mock.calls[0][0];
      expect(registerCall).not.toHaveProperty('confirmPassword');
    });

    it('should not call authState.register when form is invalid', () => {
      component.form.controls.email.setValue('invalid');

      component.onSubmit();

      expect(authStateMock.register).not.toHaveBeenCalled();
    });

    it('should mark all fields as touched when submitting invalid form', () => {
      component.onSubmit();

      expect(component.form.controls.email.touched).toBe(true);
      expect(component.form.controls.username.touched).toBe(true);
      expect(component.form.controls.displayName.touched).toBe(true);
      expect(component.form.controls.password.touched).toBe(true);
      expect(component.form.controls.confirmPassword.touched).toBe(true);
    });
  });

  describe('loading state', () => {
    it('should disable submit button when loading', () => {
      authStateMock.loading.set(true);
      fixture.detectChanges();

      const submitBtn = fixture.nativeElement.querySelector('button[type="submit"]');

      expect(submitBtn.disabled).toBe(true);
      expect(submitBtn.textContent).toContain('Creating account');
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
      authStateMock.error.set('Email already taken');
      fixture.detectChanges();

      const errorBanner = fixture.nativeElement.querySelector('.error-banner');

      expect(errorBanner).toBeTruthy();
      expect(errorBanner.textContent).toContain('Email already taken');
    });

    it('should not display error banner when no error', () => {
      authStateMock.error.set(null);
      fixture.detectChanges();

      const errorBanner = fixture.nativeElement.querySelector('.error-banner');

      expect(errorBanner).toBeFalsy();
    });
  });
});
