import { Component, inject } from '@angular/core';
import {
  AbstractControl,
  FormBuilder,
  ReactiveFormsModule,
  ValidationErrors,
  Validators
} from '@angular/forms';
import { RouterLink } from '@angular/router';
import { AuthStateService } from '../../services/auth-state.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authState = inject(AuthStateService);

  readonly loading = this.authState.loading;
  readonly error = this.authState.error;

  readonly form = this.fb.nonNullable.group(
    {
      email: ['', [Validators.required, Validators.email]],
      username: ['', [Validators.required, Validators.minLength(3)]],
      displayName: ['', [Validators.required]],
      password: ['', [Validators.required, this.passwordValidator]],
      confirmPassword: ['', [Validators.required]]
    },
    { validators: this.passwordMatchValidator }
  );

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const { confirmPassword, ...registerData } = this.form.getRawValue();
    this.authState.register(registerData);
  }

  private passwordValidator(control: AbstractControl): ValidationErrors | null {
    const value = control.value;
    if (!value) return null;

    const errors: ValidationErrors = {};

    if (value.length < 8) {
      errors['minLength'] = true;
    }
    if (!/[A-Z]/.test(value)) {
      errors['uppercase'] = true;
    }
    if (!/[0-9]/.test(value)) {
      errors['digit'] = true;
    }
    if (!/[!@#$%^&*(),.?":{}|<>]/.test(value)) {
      errors['special'] = true;
    }

    return Object.keys(errors).length ? errors : null;
  }

  private passwordMatchValidator(group: AbstractControl): ValidationErrors | null {
    const password = group.get('password')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;

    if (password && confirmPassword && password !== confirmPassword) {
      return { passwordMismatch: true };
    }
    return null;
  }

  get emailErrors(): string | null {
    const control = this.form.controls.email;
    if (!control.touched || control.valid) return null;
    if (control.hasError('required')) return 'Email is required';
    if (control.hasError('email')) return 'Invalid email format';
    return null;
  }

  get usernameErrors(): string | null {
    const control = this.form.controls.username;
    if (!control.touched || control.valid) return null;
    if (control.hasError('required')) return 'Username is required';
    if (control.hasError('minlength')) return 'Username must be at least 3 characters';
    return null;
  }

  get displayNameErrors(): string | null {
    const control = this.form.controls.displayName;
    if (!control.touched || control.valid) return null;
    if (control.hasError('required')) return 'Display name is required';
    return null;
  }

  get passwordErrors(): string | null {
    const control = this.form.controls.password;
    if (!control.touched || control.valid) return null;
    if (control.hasError('required')) return 'Password is required';
    if (control.hasError('minLength')) return 'Password must be at least 8 characters';
    if (control.hasError('uppercase')) return 'Password must contain an uppercase letter';
    if (control.hasError('digit')) return 'Password must contain a digit';
    if (control.hasError('special')) return 'Password must contain a special character';
    return null;
  }

  get confirmPasswordErrors(): string | null {
    const control = this.form.controls.confirmPassword;
    if (!control.touched) return null;
    if (control.hasError('required')) return 'Please confirm your password';
    if (this.form.hasError('passwordMismatch')) return 'Passwords do not match';
    return null;
  }
}
