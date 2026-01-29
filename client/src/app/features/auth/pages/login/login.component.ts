import { Component, inject } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { AuthFormComponent } from '../../components/auth-form/auth-form.component';
import { AuthStateService } from '../../services/auth-state.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, AuthFormComponent],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  private readonly fb = inject(FormBuilder);
  private readonly authState = inject(AuthStateService);

  readonly loading = this.authState.loading;
  readonly error = this.authState.error;

  readonly form = this.fb.nonNullable.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.authState.login(this.form.getRawValue());
  }

  get emailErrors(): string | null {
    const control = this.form.controls.email;
    if (!control.touched || control.valid) return null;
    if (control.hasError('required')) return 'Email is required';
    if (control.hasError('email')) return 'Invalid email format';
    return null;
  }

  get passwordErrors(): string | null {
    const control = this.form.controls.password;
    if (!control.touched || control.valid) return null;
    if (control.hasError('required')) return 'Password is required';
    return null;
  }
}
