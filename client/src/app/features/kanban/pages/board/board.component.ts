import { Component, OnInit, OnDestroy, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { KanbanStateService } from '../../services/kanban-state.service';
import {
  Column,
  CreateColumnRequest,
  CreateTaskRequest,
  Priority,
  PRIORITY_LABELS,
  PRIORITY_COLORS
} from '../../models/kanban.models';

@Component({
  selector: 'app-board',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  template: `
    <div class="board-page">
      <header class="board-header">
        <div class="header-left">
          <a routerLink="/projects" class="back-link">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M10.5 19.5L3 12m0 0l7.5-7.5M3 12h18" />
            </svg>
            Back to Projects
          </a>
          @if (state.selectedProject(); as project) {
            <h1 class="board-title">
              <span class="project-color" [style.background-color]="project.color"></span>
              {{ project.name }}
            </h1>
          }
        </div>
        <div class="header-right">
          <button class="btn btn-primary" (click)="showAddColumnForm()">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="icon">
              <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
            </svg>
            Add Column
          </button>
        </div>
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

      @if (state.loading() && !state.selectedProject()) {
        <div class="loading-state">
          <div class="spinner"></div>
          <span>Loading board...</span>
        </div>
      } @else if (!state.selectedProject()) {
        <div class="error-state">
          <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="error-icon">
            <path stroke-linecap="round" stroke-linejoin="round" d="M12 9v3.75m9-.75a9 9 0 11-18 0 9 9 0 0118 0zm-9 3.75h.008v.008H12v-.008z" />
          </svg>
          <h2>Project not found</h2>
          <p>The project you're looking for doesn't exist or has been deleted.</p>
          <a routerLink="/projects" class="btn btn-primary">Back to Projects</a>
        </div>
      } @else {
        <div class="board-container">
          <div class="columns-wrapper">
            @for (column of state.columns(); track column.id) {
              <div class="column">
                <div class="column-header">
                  <h3 class="column-title">
                    {{ column.name }}
                    <span class="task-count">{{ column.tasks.length }}</span>
                  </h3>
                  <div class="column-actions">
                    <button class="btn-icon" (click)="showAddTaskForm(column.id)" title="Add task">
                      <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
                      </svg>
                    </button>
                    <button class="btn-icon btn-danger" (click)="deleteColumn(column.id)" title="Delete column">
                      <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0" />
                      </svg>
                    </button>
                  </div>
                </div>

                <div class="column-content">
                  @if (addingTaskToColumn() === column.id) {
                    <div class="task-form">
                      <input
                        type="text"
                        [(ngModel)]="newTaskTitle"
                        placeholder="Enter task title..."
                        class="form-control"
                        (keyup.enter)="createTask(column.id)"
                        (keyup.escape)="cancelAddTask()"
                        #taskInput
                      />
                      <div class="form-actions">
                        <button class="btn btn-primary btn-sm" (click)="createTask(column.id)" [disabled]="!newTaskTitle.trim()">
                          Add
                        </button>
                        <button class="btn btn-secondary btn-sm" (click)="cancelAddTask()">
                          Cancel
                        </button>
                      </div>
                    </div>
                  }

                  <div class="tasks-list">
                    @for (task of column.tasks; track task.id) {
                      @if (!task.isArchived) {
                        <div class="task-card" (click)="openTaskDetail(task.id)">
                          <div class="task-title">{{ task.title }}</div>
                          @if (task.description) {
                            <div class="task-description">{{ task.description }}</div>
                          }
                          <div class="task-meta">
                            <span
                              class="priority-badge"
                              [style.background-color]="getPriorityColor(task.priority)"
                            >
                              {{ getPriorityLabel(task.priority) }}
                            </span>
                            @if (task.dueDate) {
                              <span class="due-date" [class.overdue]="isOverdue(task.dueDate)">
                                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                                  <path stroke-linecap="round" stroke-linejoin="round" d="M6.75 3v2.25M17.25 3v2.25M3 18.75V7.5a2.25 2.25 0 012.25-2.25h13.5A2.25 2.25 0 0121 7.5v11.25m-18 0A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75m-18 0v-7.5A2.25 2.25 0 015.25 9h13.5A2.25 2.25 0 0121 11.25v7.5" />
                                </svg>
                                {{ task.dueDate | date:'shortDate' }}
                              </span>
                            }
                            @if (task.checklists.length > 0) {
                              <span class="checklist-count">
                                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                                  <path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75L11.25 15 15 9.75M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                                </svg>
                                {{ getCompletedChecklists(task) }}/{{ task.checklists.length }}
                              </span>
                            }
                          </div>
                        </div>
                      }
                    }
                  </div>
                </div>
              </div>
            }

            @if (showColumnForm()) {
              <div class="column add-column-form">
                <div class="column-header">
                  <input
                    type="text"
                    [(ngModel)]="newColumnName"
                    placeholder="Enter column name..."
                    class="form-control"
                    (keyup.enter)="createColumn()"
                    (keyup.escape)="cancelAddColumn()"
                    #columnInput
                  />
                </div>
                <div class="form-actions" style="padding: 0.75rem;">
                  <button class="btn btn-primary btn-sm" (click)="createColumn()" [disabled]="!newColumnName.trim()">
                    Add Column
                  </button>
                  <button class="btn btn-secondary btn-sm" (click)="cancelAddColumn()">
                    Cancel
                  </button>
                </div>
              </div>
            }

            @if (state.columns().length === 0 && !showColumnForm()) {
              <div class="empty-board">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="empty-icon">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M9 4.5v15m6-15v15m-10.875 0h15.75c.621 0 1.125-.504 1.125-1.125V5.625c0-.621-.504-1.125-1.125-1.125H4.125C3.504 4.5 3 5.004 3 5.625v12.75c0 .621.504 1.125 1.125 1.125z" />
                </svg>
                <h2>No columns yet</h2>
                <p>Add your first column to start organizing tasks</p>
                <button class="btn btn-primary" (click)="showAddColumnForm()">
                  Add Column
                </button>
              </div>
            }
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .board-page {
      height: 100vh;
      display: flex;
      flex-direction: column;
      background: var(--bg-secondary, #f8fafc);
    }

    .board-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1rem 1.5rem;
      background: var(--bg-card, #ffffff);
      border-bottom: 1px solid var(--border-color, #e2e8f0);
    }

    .header-left {
      display: flex;
      align-items: center;
      gap: 1.5rem;
    }

    .back-link {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.875rem;
      color: var(--text-muted, #94a3b8);
      text-decoration: none;
      transition: color 0.15s ease;
    }

    .back-link:hover {
      color: var(--text-primary, #1e293b);
    }

    .back-link svg {
      width: 1rem;
      height: 1rem;
    }

    .board-title {
      display: flex;
      align-items: center;
      gap: 0.75rem;
      margin: 0;
      font-size: 1.25rem;
      font-weight: 600;
      color: var(--text-primary, #1e293b);
    }

    .project-color {
      width: 0.75rem;
      height: 0.75rem;
      border-radius: 50%;
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

    .btn-sm {
      padding: 0.375rem 0.75rem;
      font-size: 0.8125rem;
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
      padding: 0.375rem;
      background: transparent;
      border: none;
      border-radius: 0.25rem;
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
      width: 1rem;
      height: 1rem;
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
      border-bottom: 1px solid #fecaca;
      color: #dc2626;
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
    .error-state {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      flex: 1;
      padding: 2rem;
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

    .error-icon {
      width: 4rem;
      height: 4rem;
      margin-bottom: 1rem;
      color: var(--danger, #ef4444);
    }

    .error-state h2,
    .empty-board h2 {
      margin: 0 0 0.5rem;
      font-size: 1.125rem;
      color: var(--text-primary, #1e293b);
    }

    .error-state p,
    .empty-board p {
      margin: 0 0 1.5rem;
    }

    .board-container {
      flex: 1;
      overflow-x: auto;
      padding: 1.5rem;
    }

    .columns-wrapper {
      display: flex;
      gap: 1rem;
      height: 100%;
      min-height: 400px;
    }

    .column {
      flex-shrink: 0;
      width: 300px;
      background: var(--bg-card, #ffffff);
      border: 1px solid var(--border-color, #e2e8f0);
      border-radius: 0.5rem;
      display: flex;
      flex-direction: column;
      max-height: calc(100vh - 180px);
    }

    .column-header {
      display: flex;
      align-items: center;
      justify-content: space-between;
      padding: 0.75rem;
      border-bottom: 1px solid var(--border-color, #e2e8f0);
    }

    .column-title {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      margin: 0;
      font-size: 0.875rem;
      font-weight: 600;
      color: var(--text-primary, #1e293b);
    }

    .task-count {
      background: var(--bg-secondary, #f1f5f9);
      padding: 0.125rem 0.5rem;
      border-radius: 9999px;
      font-size: 0.75rem;
      font-weight: 500;
      color: var(--text-muted, #94a3b8);
    }

    .column-actions {
      display: flex;
      gap: 0.25rem;
    }

    .column-content {
      flex: 1;
      overflow-y: auto;
      padding: 0.75rem;
    }

    .task-form {
      margin-bottom: 0.75rem;
    }

    .form-control {
      width: 100%;
      padding: 0.5rem 0.75rem;
      font-size: 0.875rem;
      border: 1px solid var(--border-color, #e2e8f0);
      border-radius: 0.375rem;
      background: var(--bg-card, #ffffff);
      color: var(--text-primary, #1e293b);
    }

    .form-control:focus {
      outline: none;
      border-color: var(--primary, #6366f1);
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
    }

    .form-actions {
      display: flex;
      gap: 0.5rem;
      margin-top: 0.5rem;
    }

    .tasks-list {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .task-card {
      background: var(--bg-card, #ffffff);
      border: 1px solid var(--border-color, #e2e8f0);
      border-radius: 0.375rem;
      padding: 0.75rem;
      cursor: pointer;
      transition: all 0.15s ease;
    }

    .task-card:hover {
      border-color: var(--primary, #6366f1);
      box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
    }

    .task-title {
      font-size: 0.875rem;
      font-weight: 500;
      color: var(--text-primary, #1e293b);
      margin-bottom: 0.5rem;
    }

    .task-description {
      font-size: 0.75rem;
      color: var(--text-secondary, #64748b);
      margin-bottom: 0.5rem;
      display: -webkit-box;
      -webkit-line-clamp: 2;
      -webkit-box-orient: vertical;
      overflow: hidden;
    }

    .task-meta {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
      align-items: center;
    }

    .priority-badge {
      padding: 0.125rem 0.5rem;
      border-radius: 9999px;
      font-size: 0.625rem;
      font-weight: 600;
      color: white;
      text-transform: uppercase;
      letter-spacing: 0.025em;
    }

    .due-date,
    .checklist-count {
      display: flex;
      align-items: center;
      gap: 0.25rem;
      font-size: 0.75rem;
      color: var(--text-muted, #94a3b8);
    }

    .due-date svg,
    .checklist-count svg {
      width: 0.875rem;
      height: 0.875rem;
    }

    .due-date.overdue {
      color: var(--danger, #ef4444);
    }

    .add-column-form {
      background: var(--bg-secondary, #f8fafc);
      border-style: dashed;
    }

    .empty-board {
      display: flex;
      flex-direction: column;
      align-items: center;
      justify-content: center;
      padding: 2rem;
      text-align: center;
      background: var(--bg-card, #ffffff);
      border: 2px dashed var(--border-color, #e2e8f0);
      border-radius: 0.5rem;
      min-width: 300px;
      color: var(--text-muted, #94a3b8);
    }

    .empty-icon {
      width: 3rem;
      height: 3rem;
      margin-bottom: 1rem;
    }
  `]
})
export class BoardComponent implements OnInit, OnDestroy {
  readonly state = inject(KanbanStateService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly showColumnForm = signal(false);
  readonly addingTaskToColumn = signal<string | null>(null);

  newColumnName = '';
  newTaskTitle = '';
  private projectId: string | null = null;

  ngOnInit(): void {
    this.projectId = this.route.snapshot.paramMap.get('id');
    if (this.projectId) {
      this.state.loadProjectById(this.projectId);
    }
  }

  ngOnDestroy(): void {
    this.state.clearSelectedProject();
  }

  showAddColumnForm(): void {
    this.newColumnName = '';
    this.showColumnForm.set(true);
  }

  cancelAddColumn(): void {
    this.newColumnName = '';
    this.showColumnForm.set(false);
  }

  createColumn(): void {
    if (!this.projectId || !this.newColumnName.trim()) return;

    const request: CreateColumnRequest = {
      name: this.newColumnName.trim()
    };
    this.state.createColumn(this.projectId, request);
    this.cancelAddColumn();
  }

  showAddTaskForm(columnId: string): void {
    this.newTaskTitle = '';
    this.addingTaskToColumn.set(columnId);
  }

  cancelAddTask(): void {
    this.newTaskTitle = '';
    this.addingTaskToColumn.set(null);
  }

  createTask(columnId: string): void {
    if (!this.newTaskTitle.trim()) return;

    const request: CreateTaskRequest = {
      title: this.newTaskTitle.trim(),
      priority: Priority.Medium
    };
    this.state.createTask(columnId, request);
    this.cancelAddTask();
  }

  deleteColumn(columnId: string): void {
    if (confirm('Are you sure you want to delete this column? All tasks will be deleted.')) {
      this.state.deleteColumn(columnId);
    }
  }

  openTaskDetail(taskId: string): void {
    this.state.loadTaskById(taskId);
  }

  getPriorityLabel(priority: Priority): string {
    return PRIORITY_LABELS[priority];
  }

  getPriorityColor(priority: Priority): string {
    return PRIORITY_COLORS[priority];
  }

  isOverdue(dueDate: string): boolean {
    return new Date(dueDate) < new Date();
  }

  getCompletedChecklists(task: { checklists: { isCompleted: boolean }[] }): number {
    return task.checklists.filter(c => c.isCompleted).length;
  }
}
