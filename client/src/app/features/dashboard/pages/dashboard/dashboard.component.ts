import { Component, inject } from '@angular/core';
import { AuthStateService } from '../../../auth/services/auth-state.service';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  template: `
    <div class="dashboard">
      <header>
        <h1>Welcome, {{ authState.user()?.displayName ?? 'User' }}!</h1>
        <button (click)="logout()" class="logout-btn">Logout</button>
      </header>
      <main>
        <p>Dashboard coming soon...</p>
      </main>
    </div>
  `,
  styles: [`
    .dashboard {
      min-height: 100vh;
      background: #f3f4f6;
      padding: 2rem;
    }
    header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 2rem;
    }
    h1 {
      margin: 0;
      color: #1a1a2e;
    }
    .logout-btn {
      padding: 0.5rem 1rem;
      background: #dc2626;
      color: white;
      border: none;
      border-radius: 0.375rem;
      cursor: pointer;
    }
    .logout-btn:hover {
      background: #b91c1c;
    }
    main {
      background: white;
      padding: 2rem;
      border-radius: 0.5rem;
      box-shadow: 0 1px 3px rgba(0,0,0,0.1);
    }
  `]
})
export class DashboardComponent {
  readonly authState = inject(AuthStateService);

  logout(): void {
    this.authState.logout();
  }
}
