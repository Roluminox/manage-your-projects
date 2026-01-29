import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { KanbanStateService } from '../../services/kanban-state.service';
import { CreateProjectRequest, DEFAULT_PROJECT_COLORS, UpdateProjectRequest } from '../../models/kanban.models';

type ViewMode = 'list' | 'create' | 'edit';

interface ProjectForm {
  name: string;
  description: string;
  color: string;
}

@Component({
  selector: 'app-project-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="project-list-page">
      <header class="page-header">
        <h1 class="page-title">My Projects</h1>
        @if (viewMode() === 'list') {
          <button class="btn btn-primary" (click)="showCreateForm()">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="icon">
              <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
            </svg>
            New Project
          </button>
        }
      </header>

      @if (state.error()) {
        <div class="error-banner">
          {{ state.error() }}
          <button class="close-btn" (click)="state.clearError()">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
      }

      @switch (viewMode()) {
        @case ('list') {
          @if (state.loading()) {
            <div class="loading-state">
              <div class="spinner"></div>
              <span>Loading projects...</span>
            </div>
          } @else if (state.projects().length === 0) {
            <div class="empty-state">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="empty-icon">
                <path stroke-linecap="round" stroke-linejoin="round" d="M3.75 6A2.25 2.25 0 016 3.75h2.25A2.25 2.25 0 0110.5 6v2.25a2.25 2.25 0 01-2.25 2.25H6a2.25 2.25 0 01-2.25-2.25V6zM3.75 15.75A2.25 2.25 0 016 13.5h2.25a2.25 2.25 0 012.25 2.25V18a2.25 2.25 0 01-2.25 2.25H6A2.25 2.25 0 013.75 18v-2.25zM13.5 6a2.25 2.25 0 012.25-2.25H18A2.25 2.25 0 0120.25 6v2.25A2.25 2.25 0 0118 10.5h-2.25a2.25 2.25 0 01-2.25-2.25V6zM13.5 15.75a2.25 2.25 0 012.25-2.25H18a2.25 2.25 0 012.25 2.25V18A2.25 2.25 0 0118 20.25h-2.25A2.25 2.25 0 0113.5 18v-2.25z" />
              </svg>
              <h2>No projects yet</h2>
              <p>Create your first project to get started with your Kanban board</p>
              <button class="btn btn-primary" (click)="showCreateForm()">
                Create Project
              </button>
            </div>
          } @else {
            <div class="projects-grid">
              @for (project of state.projects(); track project.id) {
                <div class="project-card" (click)="openProject(project.id)">
                  <div class="project-color" [style.background-color]="project.color"></div>
                  <div class="project-content">
                    <h3 class="project-name">{{ project.name }}</h3>
                    @if (project.description) {
                      <p class="project-description">{{ project.description }}</p>
                    }
                    <div class="project-stats">
                      <span class="stat">
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                          <path stroke-linecap="round" stroke-linejoin="round" d="M9 4.5v15m6-15v15m-10.875 0h15.75c.621 0 1.125-.504 1.125-1.125V5.625c0-.621-.504-1.125-1.125-1.125H4.125C3.504 4.5 3 5.004 3 5.625v12.75c0 .621.504 1.125 1.125 1.125z" />
                        </svg>
                        {{ project.columnCount }} columns
                      </span>
                      <span class="stat">
                        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                          <path stroke-linecap="round" stroke-linejoin="round" d="M9 12h3.75M9 15h3.75M9 18h3.75m3 .75H18a2.25 2.25 0 002.25-2.25V6.108c0-1.135-.845-2.098-1.976-2.192a48.424 48.424 0 00-1.123-.08m-5.801 0c-.065.21-.1.433-.1.664 0 .414.336.75.75.75h4.5a.75.75 0 00.75-.75 2.25 2.25 0 00-.1-.664m-5.8 0A2.251 2.251 0 0113.5 2.25H15c1.012 0 1.867.668 2.15 1.586m-5.8 0c-.376.023-.75.05-1.124.08C9.095 4.01 8.25 4.973 8.25 6.108V8.25m0 0H4.875c-.621 0-1.125.504-1.125 1.125v11.25c0 .621.504 1.125 1.125 1.125h9.75c.621 0 1.125-.504 1.125-1.125V9.375c0-.621-.504-1.125-1.125-1.125H8.25z" />
                        </svg>
                        {{ project.taskCount }} tasks
                      </span>
                    </div>
                    <div class="project-date">
                      Created {{ project.createdAt | date:'mediumDate' }}
                    </div>
                  </div>
                  <div class="project-actions" (click)="$event.stopPropagation()">
                    <button class="btn-icon" (click)="showEditForm(project)" title="Edit project">
                      <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M16.862 4.487l1.687-1.688a1.875 1.875 0 112.652 2.652L10.582 16.07a4.5 4.5 0 01-1.897 1.13L6 18l.8-2.685a4.5 4.5 0 011.13-1.897l8.932-8.931zm0 0L19.5 7.125M18 14v4.75A2.25 2.25 0 0115.75 21H5.25A2.25 2.25 0 013 18.75V8.25A2.25 2.25 0 015.25 6H10" />
                      </svg>
                    </button>
                    <button class="btn-icon btn-danger" (click)="deleteProject(project.id)" title="Delete project">
                      <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0" />
                      </svg>
                    </button>
                  </div>
                </div>
              }
            </div>
          }
        }

        @case ('create') {
          <div class="form-container">
            <h2 class="form-title">Create New Project</h2>
            <form (ngSubmit)="onCreateProject()" #projectForm="ngForm">
              <div class="form-group">
                <label for="name">Project Name *</label>
                <input
                  type="text"
                  id="name"
                  name="name"
                  [(ngModel)]="form.name"
                  required
                  maxlength="100"
                  class="form-control"
                  placeholder="Enter project name"
                />
              </div>

              <div class="form-group">
                <label for="description">Description</label>
                <textarea
                  id="description"
                  name="description"
                  [(ngModel)]="form.description"
                  maxlength="500"
                  class="form-control"
                  rows="3"
                  placeholder="Enter project description (optional)"
                ></textarea>
              </div>

              <div class="form-group">
                <label>Color</label>
                <div class="color-picker">
                  @for (color of colors; track color) {
                    <button
                      type="button"
                      class="color-option"
                      [class.selected]="form.color === color"
                      [style.background-color]="color"
                      (click)="form.color = color"
                    ></button>
                  }
                </div>
              </div>

              <div class="form-actions">
                <button type="button" class="btn btn-secondary" (click)="backToList()">
                  Cancel
                </button>
                <button
                  type="submit"
                  class="btn btn-primary"
                  [disabled]="projectForm.invalid || state.loading()"
                >
                  @if (state.loading()) {
                    <div class="btn-spinner"></div>
                    Creating...
                  } @else {
                    Create Project
                  }
                </button>
              </div>
            </form>
          </div>
        }

        @case ('edit') {
          <div class="form-container">
            <h2 class="form-title">Edit Project</h2>
            <form (ngSubmit)="onUpdateProject()" #projectForm="ngForm">
              <div class="form-group">
                <label for="name">Project Name *</label>
                <input
                  type="text"
                  id="name"
                  name="name"
                  [(ngModel)]="form.name"
                  required
                  maxlength="100"
                  class="form-control"
                  placeholder="Enter project name"
                />
              </div>

              <div class="form-group">
                <label for="description">Description</label>
                <textarea
                  id="description"
                  name="description"
                  [(ngModel)]="form.description"
                  maxlength="500"
                  class="form-control"
                  rows="3"
                  placeholder="Enter project description (optional)"
                ></textarea>
              </div>

              <div class="form-group">
                <label>Color</label>
                <div class="color-picker">
                  @for (color of colors; track color) {
                    <button
                      type="button"
                      class="color-option"
                      [class.selected]="form.color === color"
                      [style.background-color]="color"
                      (click)="form.color = color"
                    ></button>
                  }
                </div>
              </div>

              <div class="form-actions">
                <button type="button" class="btn btn-secondary" (click)="backToList()">
                  Cancel
                </button>
                <button
                  type="submit"
                  class="btn btn-primary"
                  [disabled]="projectForm.invalid || state.loading()"
                >
                  @if (state.loading()) {
                    <div class="btn-spinner"></div>
                    Saving...
                  } @else {
                    Save Changes
                  }
                </button>
              </div>
            </form>
          </div>
        }
      }
    </div>
  `,
  styles: [`
    .project-list-page {
      max-width: 1200px;
      margin: 0 auto;
      padding: 1.5rem;
    }

    .page-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
    }

    .page-title {
      font-size: 1.5rem;
      font-weight: 600;
      margin: 0;
      color: var(--text-primary, #1e293b);
    }

    .btn {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.625rem 1rem;
      border-radius: 0.375rem;
      font-size: 0.875rem;
      font-weight: 500;
      cursor: pointer;
      transition: all 0.15s ease;
      border: none;
    }

    .btn-primary {
      background: var(--primary, #6366f1);
      color: white;
    }

    .btn-primary:hover:not(:disabled) {
      background: var(--primary-dark, #4f46e5);
    }

    .btn-primary:disabled {
      opacity: 0.6;
      cursor: not-allowed;
    }

    .btn-secondary {
      background: transparent;
      color: var(--text-primary, #1e293b);
      border: 1px solid var(--border-color, #e2e8f0);
    }

    .btn-secondary:hover {
      background: var(--bg-secondary, #f8fafc);
    }

    .btn-icon {
      padding: 0.5rem;
      background: transparent;
      border: none;
      border-radius: 0.375rem;
      color: var(--text-muted, #94a3b8);
      cursor: pointer;
      transition: all 0.15s ease;
    }

    .btn-icon:hover {
      background: var(--bg-secondary, #f1f5f9);
      color: var(--text-primary, #1e293b);
    }

    .btn-icon.btn-danger:hover {
      background: #fef2f2;
      color: var(--danger, #ef4444);
    }

    .btn-icon svg {
      width: 1.125rem;
      height: 1.125rem;
    }

    .icon {
      width: 1rem;
      height: 1rem;
    }

    .error-banner {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0.75rem 1rem;
      background: #fef2f2;
      border: 1px solid #fecaca;
      border-radius: 0.375rem;
      color: #dc2626;
      margin-bottom: 1rem;
    }

    .close-btn {
      padding: 0.25rem;
      background: transparent;
      border: none;
      cursor: pointer;
      color: inherit;
    }

    .close-btn svg {
      width: 1rem;
      height: 1rem;
    }

    .loading-state,
    .empty-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 4rem 2rem;
      text-align: center;
      color: var(--text-muted, #94a3b8);
    }

    .spinner {
      width: 2rem;
      height: 2rem;
      border: 2px solid var(--border-color, #e2e8f0);
      border-top-color: var(--primary, #6366f1);
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
      margin-bottom: 1rem;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }

    .empty-icon {
      width: 4rem;
      height: 4rem;
      margin-bottom: 1rem;
    }

    .empty-state h2 {
      margin: 0 0 0.5rem;
      font-size: 1.125rem;
      color: var(--text-primary, #1e293b);
    }

    .empty-state p {
      margin: 0 0 1.5rem;
    }

    .projects-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 1rem;
    }

    .project-card {
      position: relative;
      background: var(--bg-card, #ffffff);
      border: 1px solid var(--border-color, #e2e8f0);
      border-radius: 0.5rem;
      overflow: hidden;
      cursor: pointer;
      transition: all 0.15s ease;
    }

    .project-card:hover {
      border-color: var(--primary, #6366f1);
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
    }

    .project-color {
      height: 4px;
    }

    .project-content {
      padding: 1rem;
    }

    .project-name {
      margin: 0 0 0.5rem;
      font-size: 1rem;
      font-weight: 600;
      color: var(--text-primary, #1e293b);
    }

    .project-description {
      margin: 0 0 0.75rem;
      font-size: 0.875rem;
      color: var(--text-secondary, #64748b);
      line-height: 1.5;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }

    .project-stats {
      display: flex;
      gap: 1rem;
      margin-bottom: 0.5rem;
    }

    .stat {
      display: flex;
      align-items: center;
      gap: 0.25rem;
      font-size: 0.75rem;
      color: var(--text-muted, #94a3b8);
    }

    .stat svg {
      width: 0.875rem;
      height: 0.875rem;
    }

    .project-date {
      font-size: 0.75rem;
      color: var(--text-muted, #94a3b8);
    }

    .project-actions {
      position: absolute;
      top: 0.75rem;
      right: 0.75rem;
      display: flex;
      gap: 0.25rem;
      opacity: 0;
      transition: opacity 0.15s ease;
    }

    .project-card:hover .project-actions {
      opacity: 1;
    }

    .form-container {
      max-width: 500px;
    }

    .form-title {
      font-size: 1.25rem;
      font-weight: 600;
      margin: 0 0 1.5rem;
      color: var(--text-primary, #1e293b);
    }

    .form-group {
      margin-bottom: 1.25rem;
    }

    .form-group label {
      display: block;
      margin-bottom: 0.5rem;
      font-size: 0.875rem;
      font-weight: 500;
      color: var(--text-primary, #1e293b);
    }

    .form-control {
      width: 100%;
      padding: 0.625rem 0.75rem;
      font-size: 0.875rem;
      border: 1px solid var(--border-color, #e2e8f0);
      border-radius: 0.375rem;
      background: var(--bg-card, #ffffff);
      color: var(--text-primary, #1e293b);
      transition: border-color 0.15s ease;
    }

    .form-control:focus {
      outline: none;
      border-color: var(--primary, #6366f1);
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
    }

    .form-control::placeholder {
      color: var(--text-muted, #94a3b8);
    }

    textarea.form-control {
      resize: vertical;
      min-height: 80px;
    }

    .color-picker {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
    }

    .color-option {
      width: 2rem;
      height: 2rem;
      border-radius: 50%;
      border: 2px solid transparent;
      cursor: pointer;
      transition: all 0.15s ease;
    }

    .color-option:hover {
      transform: scale(1.1);
    }

    .color-option.selected {
      border-color: var(--text-primary, #1e293b);
      box-shadow: 0 0 0 2px white, 0 0 0 4px var(--text-primary, #1e293b);
    }

    .form-actions {
      display: flex;
      gap: 0.75rem;
      margin-top: 1.5rem;
    }

    .btn-spinner {
      width: 1rem;
      height: 1rem;
      border: 2px solid rgba(255, 255, 255, 0.3);
      border-top-color: white;
      border-radius: 50%;
      animation: spin 0.8s linear infinite;
    }
  `]
})
export class ProjectListComponent implements OnInit {
  readonly state = inject(KanbanStateService);
  private readonly router = inject(Router);

  readonly viewMode = signal<ViewMode>('list');
  readonly colors = DEFAULT_PROJECT_COLORS;

  form: ProjectForm = this.getEmptyForm();
  private editingProjectId: string | null = null;

  ngOnInit(): void {
    this.state.loadProjects();
  }

  showCreateForm(): void {
    this.form = this.getEmptyForm();
    this.editingProjectId = null;
    this.viewMode.set('create');
  }

  showEditForm(project: { id: string; name: string; description?: string; color: string }): void {
    this.form = {
      name: project.name,
      description: project.description || '',
      color: project.color
    };
    this.editingProjectId = project.id;
    this.viewMode.set('edit');
  }

  backToList(): void {
    this.form = this.getEmptyForm();
    this.editingProjectId = null;
    this.viewMode.set('list');
  }

  openProject(id: string): void {
    this.router.navigate(['/projects', id]);
  }

  onCreateProject(): void {
    const request: CreateProjectRequest = {
      name: this.form.name.trim(),
      description: this.form.description.trim() || undefined,
      color: this.form.color
    };
    this.state.createProject(request);
    this.backToList();
  }

  onUpdateProject(): void {
    if (!this.editingProjectId) return;

    const request: UpdateProjectRequest = {
      name: this.form.name.trim(),
      description: this.form.description.trim() || undefined,
      color: this.form.color
    };
    this.state.updateProject(this.editingProjectId, request);
    this.backToList();
  }

  deleteProject(id: string): void {
    if (confirm('Are you sure you want to delete this project? All columns and tasks will be deleted.')) {
      this.state.deleteProject(id);
    }
  }

  private getEmptyForm(): ProjectForm {
    return {
      name: '',
      description: '',
      color: DEFAULT_PROJECT_COLORS[0]
    };
  }
}
