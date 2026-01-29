import { Component, input } from '@angular/core';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-auth-form',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './auth-form.component.html',
  styleUrl: './auth-form.component.scss'
})
export class AuthFormComponent {
  readonly title = input.required<string>();
  readonly subtitle = input.required<string>();
  readonly error = input<string | null>(null);
  readonly linkText = input<string>();
  readonly linkLabel = input<string>();
  readonly linkRoute = input<string>();
}
