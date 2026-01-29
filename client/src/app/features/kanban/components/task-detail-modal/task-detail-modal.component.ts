import { Component, EventEmitter, Input, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {
  ChecklistItem,
  Priority,
  PRIORITY_COLORS,
  PRIORITY_LABELS,
  TaskItem,
  UpdateTaskRequest
} from '../../models/kanban.models';

@Component({
  selector: 'app-task-detail-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    @if (task) {
      <div class="modal-overlay" (click)="onClose()">
        <div class="modal-content" (click)="$event.stopPropagation()">
          <header class="modal-header">
            <div class="header-actions">
              @if (task.isArchived) {
                <span class="archived-badge">Archived</span>
              }
              <button class="btn-icon" (click)="onClose()" title="Close">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
                </svg>
              </button>
            </div>
          </header>

          <div class="modal-body">
            <!-- Title -->
            <div class="field-group">
              @if (editingTitle()) {
                <input
                  type="text"
                  [(ngModel)]="editedTitle"
                  class="title-input"
                  (blur)="saveTitle()"
                  (keyup.enter)="saveTitle()"
                  (keyup.escape)="cancelEditTitle()"
                  #titleInput
                />
              } @else {
                <h2 class="task-title" (click)="startEditTitle()">{{ task.title }}</h2>
              }
            </div>

            <!-- Description -->
            <div class="field-group">
              <label class="field-label">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M3.75 6.75h16.5M3.75 12h16.5m-16.5 5.25H12" />
                </svg>
                Description
              </label>
              @if (editingDescription()) {
                <textarea
                  [(ngModel)]="editedDescription"
                  class="description-input"
                  rows="4"
                  placeholder="Add a description..."
                  (blur)="saveDescription()"
                  (keyup.escape)="cancelEditDescription()"
                ></textarea>
              } @else {
                <div class="description-content" (click)="startEditDescription()">
                  @if (task.description) {
                    {{ task.description }}
                  } @else {
                    <span class="placeholder">Add a description...</span>
                  }
                </div>
              }
            </div>

            <!-- Priority & Due Date -->
            <div class="field-row">
              <div class="field-group half">
                <label class="field-label">
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M3 13.125C3 12.504 3.504 12 4.125 12h2.25c.621 0 1.125.504 1.125 1.125v6.75C7.5 20.496 6.996 21 6.375 21h-2.25A1.125 1.125 0 013 19.875v-6.75zM9.75 8.625c0-.621.504-1.125 1.125-1.125h2.25c.621 0 1.125.504 1.125 1.125v11.25c0 .621-.504 1.125-1.125 1.125h-2.25a1.125 1.125 0 01-1.125-1.125V8.625zM16.5 4.125c0-.621.504-1.125 1.125-1.125h2.25C20.496 3 21 3.504 21 4.125v15.75c0 .621-.504 1.125-1.125 1.125h-2.25a1.125 1.125 0 01-1.125-1.125V4.125z" />
                  </svg>
                  Priority
                </label>
                <select [(ngModel)]="selectedPriority" (change)="onPriorityChange()" class="form-select">
                  @for (p of priorities; track p.value) {
                    <option [value]="p.value">{{ p.label }}</option>
                  }
                </select>
              </div>
              <div class="field-group half">
                <label class="field-label">
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M6.75 3v2.25M17.25 3v2.25M3 18.75V7.5a2.25 2.25 0 012.25-2.25h13.5A2.25 2.25 0 0121 7.5v11.25m-18 0A2.25 2.25 0 005.25 21h13.5A2.25 2.25 0 0021 18.75m-18 0v-7.5A2.25 2.25 0 015.25 9h13.5A2.25 2.25 0 0121 11.25v7.5" />
                  </svg>
                  Due Date
                </label>
                <input
                  type="date"
                  [(ngModel)]="selectedDueDate"
                  (change)="onDueDateChange()"
                  class="form-input"
                />
              </div>
            </div>

            <!-- Checklists -->
            <div class="field-group">
              <label class="field-label">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M9 12.75L11.25 15 15 9.75M21 12a9 9 0 11-18 0 9 9 0 0118 0z" />
                </svg>
                Checklist
                @if (task.checklists.length > 0) {
                  <span class="checklist-progress">
                    {{ completedCount }}/{{ task.checklists.length }}
                  </span>
                }
              </label>

              @if (task.checklists.length > 0) {
                <div class="progress-bar">
                  <div class="progress-fill" [style.width.%]="progressPercent"></div>
                </div>
              }

              <div class="checklist-items">
                @for (item of task.checklists; track item.id) {
                  <div class="checklist-item" [class.completed]="item.isCompleted">
                    <input
                      type="checkbox"
                      [checked]="item.isCompleted"
                      (change)="toggleChecklistItem.emit(item.id)"
                      class="checkbox"
                    />
                    <span class="item-text">{{ item.text }}</span>
                    <button class="btn-icon-sm" (click)="deleteChecklistItem.emit(item.id)" title="Delete">
                      <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
                      </svg>
                    </button>
                  </div>
                }
              </div>

              <div class="add-checklist">
                @if (addingChecklistItem()) {
                  <div class="add-form">
                    <input
                      type="text"
                      [(ngModel)]="newChecklistText"
                      placeholder="Add checklist item..."
                      class="form-input"
                      (keyup.enter)="addChecklistItem()"
                      (keyup.escape)="cancelAddChecklistItem()"
                    />
                    <button class="btn btn-primary btn-sm" (click)="addChecklistItem()" [disabled]="!newChecklistText.trim()">
                      Add
                    </button>
                    <button class="btn btn-secondary btn-sm" (click)="cancelAddChecklistItem()">
                      Cancel
                    </button>
                  </div>
                } @else {
                  <button class="btn btn-secondary btn-sm" (click)="startAddChecklistItem()">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
                    </svg>
                    Add item
                  </button>
                }
              </div>
            </div>

            <!-- Metadata -->
            <div class="metadata">
              <span>Created: {{ task.createdAt | date:'medium' }}</span>
              @if (task.updatedAt) {
                <span>Updated: {{ task.updatedAt | date:'medium' }}</span>
              }
            </div>
          </div>

          <footer class="modal-footer">
            <div class="footer-left">
              <button class="btn btn-secondary" (click)="archiveTask.emit(!task.isArchived)">
                @if (task.isArchived) {
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M9 8.25H7.5a2.25 2.25 0 00-2.25 2.25v9a2.25 2.25 0 002.25 2.25h9a2.25 2.25 0 002.25-2.25v-9a2.25 2.25 0 00-2.25-2.25H15m0-3l-3-3m0 0l-3 3m3-3V15" />
                  </svg>
                  Restore
                } @else {
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M20.25 7.5l-.625 10.632a2.25 2.25 0 01-2.247 2.118H6.622a2.25 2.25 0 01-2.247-2.118L3.75 7.5m8.25 3v6.75m0 0l-3-3m3 3l3-3M3.375 7.5h17.25c.621 0 1.125-.504 1.125-1.125v-1.5c0-.621-.504-1.125-1.125-1.125H3.375c-.621 0-1.125.504-1.125 1.125v1.5c0 .621.504 1.125 1.125 1.125z" />
                  </svg>
                  Archive
                }
              </button>
            </div>
            <div class="footer-right">
              <button class="btn btn-danger" (click)="confirmDelete()">
                <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                  <path stroke-linecap="round" stroke-linejoin="round" d="M14.74 9l-.346 9m-4.788 0L9.26 9m9.968-3.21c.342.052.682.107 1.022.166m-1.022-.165L18.16 19.673a2.25 2.25 0 01-2.244 2.077H8.084a2.25 2.25 0 01-2.244-2.077L4.772 5.79m14.456 0a48.108 48.108 0 00-3.478-.397m-12 .562c.34-.059.68-.114 1.022-.165m0 0a48.11 48.11 0 013.478-.397m7.5 0v-.916c0-1.18-.91-2.164-2.09-2.201a51.964 51.964 0 00-3.32 0c-1.18.037-2.09 1.022-2.09 2.201v.916m7.5 0a48.667 48.667 0 00-7.5 0" />
                </svg>
                Delete
              </button>
            </div>
          </footer>
        </div>
      </div>
    }
  `,
  styles: [`
    .modal-overlay {
      position: fixed;
      top: 0;
      left: 0;
      right: 0;
      bottom: 0;
      background: rgba(0, 0, 0, 0.5);
      display: flex;
      align-items: center;
      justify-content: center;
      z-index: 1000;
      padding: 1rem;
    }

    .modal-content {
      background: var(--bg-card, #ffffff);
      border-radius: 0.5rem;
      width: 100%;
      max-width: 600px;
      max-height: 90vh;
      display: flex;
      flex-direction: column;
      box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
    }

    .modal-header {
      display: flex;
      justify-content: flex-end;
      padding: 1rem;
      border-bottom: 1px solid var(--border-color, #e2e8f0);
    }

    .header-actions {
      display: flex;
      align-items: center;
      gap: 0.5rem;
    }

    .archived-badge {
      padding: 0.25rem 0.5rem;
      background: var(--bg-secondary, #f1f5f9);
      border-radius: 0.25rem;
      font-size: 0.75rem;
      color: var(--text-muted, #94a3b8);
    }

    .modal-body {
      flex: 1;
      overflow-y: auto;
      padding: 1.5rem;
    }

    .field-group {
      margin-bottom: 1.5rem;
    }

    .field-row {
      display: flex;
      gap: 1rem;
    }

    .half {
      flex: 1;
    }

    .field-label {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      font-size: 0.75rem;
      font-weight: 600;
      text-transform: uppercase;
      letter-spacing: 0.05em;
      color: var(--text-muted, #94a3b8);
      margin-bottom: 0.5rem;
    }

    .field-label svg {
      width: 1rem;
      height: 1rem;
    }

    .task-title {
      margin: 0;
      font-size: 1.25rem;
      font-weight: 600;
      color: var(--text-primary, #1e293b);
      cursor: pointer;
      padding: 0.5rem;
      margin: -0.5rem;
      border-radius: 0.25rem;
    }

    .task-title:hover {
      background: var(--bg-secondary, #f8fafc);
    }

    .title-input {
      width: 100%;
      font-size: 1.25rem;
      font-weight: 600;
      padding: 0.5rem;
      margin: -0.5rem;
      border: 2px solid var(--primary, #6366f1);
      border-radius: 0.25rem;
      outline: none;
    }

    .description-content {
      padding: 0.75rem;
      background: var(--bg-secondary, #f8fafc);
      border-radius: 0.375rem;
      min-height: 60px;
      cursor: pointer;
      font-size: 0.875rem;
      line-height: 1.5;
      color: var(--text-secondary, #64748b);
    }

    .description-content:hover {
      background: var(--bg-tertiary, #f1f5f9);
    }

    .placeholder {
      color: var(--text-muted, #94a3b8);
      font-style: italic;
    }

    .description-input {
      width: 100%;
      padding: 0.75rem;
      font-size: 0.875rem;
      border: 2px solid var(--primary, #6366f1);
      border-radius: 0.375rem;
      resize: vertical;
      outline: none;
    }

    .form-select,
    .form-input {
      width: 100%;
      padding: 0.5rem 0.75rem;
      font-size: 0.875rem;
      border: 1px solid var(--border-color, #e2e8f0);
      border-radius: 0.375rem;
      background: var(--bg-card, #ffffff);
      color: var(--text-primary, #1e293b);
    }

    .form-select:focus,
    .form-input:focus {
      outline: none;
      border-color: var(--primary, #6366f1);
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
    }

    .checklist-progress {
      margin-left: auto;
      font-size: 0.75rem;
      color: var(--text-secondary, #64748b);
    }

    .progress-bar {
      height: 4px;
      background: var(--bg-secondary, #e2e8f0);
      border-radius: 2px;
      overflow: hidden;
      margin-bottom: 0.75rem;
    }

    .progress-fill {
      height: 100%;
      background: var(--success, #22c55e);
      transition: width 0.2s ease;
    }

    .checklist-items {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .checklist-item {
      display: flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.5rem;
      background: var(--bg-secondary, #f8fafc);
      border-radius: 0.375rem;
    }

    .checklist-item.completed .item-text {
      text-decoration: line-through;
      color: var(--text-muted, #94a3b8);
    }

    .checkbox {
      width: 1.125rem;
      height: 1.125rem;
      cursor: pointer;
    }

    .item-text {
      flex: 1;
      font-size: 0.875rem;
    }

    .btn-icon-sm {
      padding: 0.25rem;
      background: transparent;
      border: none;
      border-radius: 0.25rem;
      color: var(--text-muted, #94a3b8);
      cursor: pointer;
      opacity: 0;
      transition: all 0.15s ease;
    }

    .checklist-item:hover .btn-icon-sm {
      opacity: 1;
    }

    .btn-icon-sm:hover {
      background: #fef2f2;
      color: var(--danger, #ef4444);
    }

    .btn-icon-sm svg {
      width: 0.875rem;
      height: 0.875rem;
    }

    .add-checklist {
      margin-top: 0.75rem;
    }

    .add-form {
      display: flex;
      gap: 0.5rem;
    }

    .add-form .form-input {
      flex: 1;
    }

    .metadata {
      display: flex;
      flex-direction: column;
      gap: 0.25rem;
      font-size: 0.75rem;
      color: var(--text-muted, #94a3b8);
      padding-top: 1rem;
      border-top: 1px solid var(--border-color, #e2e8f0);
    }

    .modal-footer {
      display: flex;
      justify-content: space-between;
      padding: 1rem;
      border-top: 1px solid var(--border-color, #e2e8f0);
    }

    .footer-left,
    .footer-right {
      display: flex;
      gap: 0.5rem;
    }

    .btn {
      display: inline-flex;
      align-items: center;
      gap: 0.5rem;
      padding: 0.5rem 1rem;
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

    .btn-danger {
      background: var(--danger, #ef4444);
      color: white;
    }

    .btn-danger:hover {
      background: #dc2626;
    }

    .btn svg {
      width: 1rem;
      height: 1rem;
    }

    .btn-icon {
      padding: 0.5rem;
      background: transparent;
      border: none;
      border-radius: 0.375rem;
      color: var(--text-muted, #94a3b8);
      cursor: pointer;
    }

    .btn-icon:hover {
      background: var(--bg-secondary, #f1f5f9);
      color: var(--text-primary, #1e293b);
    }

    .btn-icon svg {
      width: 1.25rem;
      height: 1.25rem;
    }
  `]
})
export class TaskDetailModalComponent {
  @Input() task: TaskItem | null = null;

  @Output() close = new EventEmitter<void>();
  @Output() update = new EventEmitter<UpdateTaskRequest>();
  @Output() deleteTask = new EventEmitter<void>();
  @Output() archiveTask = new EventEmitter<boolean>();
  @Output() addChecklistItemEvent = new EventEmitter<string>();
  @Output() toggleChecklistItem = new EventEmitter<string>();
  @Output() deleteChecklistItem = new EventEmitter<string>();

  readonly editingTitle = signal(false);
  readonly editingDescription = signal(false);
  readonly addingChecklistItem = signal(false);

  editedTitle = '';
  editedDescription = '';
  selectedPriority: Priority = Priority.Medium;
  selectedDueDate = '';
  newChecklistText = '';

  readonly priorities = [
    { value: Priority.Low, label: 'Low' },
    { value: Priority.Medium, label: 'Medium' },
    { value: Priority.High, label: 'High' },
    { value: Priority.Critical, label: 'Critical' }
  ];

  get completedCount(): number {
    return this.task?.checklists.filter(c => c.isCompleted).length ?? 0;
  }

  get progressPercent(): number {
    if (!this.task || this.task.checklists.length === 0) return 0;
    return (this.completedCount / this.task.checklists.length) * 100;
  }

  ngOnChanges(): void {
    if (this.task) {
      this.selectedPriority = this.task.priority;
      this.selectedDueDate = this.task.dueDate ? this.task.dueDate.split('T')[0] : '';
    }
  }

  onClose(): void {
    this.close.emit();
  }

  startEditTitle(): void {
    if (!this.task) return;
    this.editedTitle = this.task.title;
    this.editingTitle.set(true);
  }

  saveTitle(): void {
    if (!this.task || !this.editedTitle.trim()) {
      this.cancelEditTitle();
      return;
    }
    if (this.editedTitle.trim() !== this.task.title) {
      this.emitUpdate({ title: this.editedTitle.trim() });
    }
    this.editingTitle.set(false);
  }

  cancelEditTitle(): void {
    this.editedTitle = this.task?.title ?? '';
    this.editingTitle.set(false);
  }

  startEditDescription(): void {
    this.editedDescription = this.task?.description ?? '';
    this.editingDescription.set(true);
  }

  saveDescription(): void {
    if (!this.task) {
      this.cancelEditDescription();
      return;
    }
    if (this.editedDescription !== (this.task.description ?? '')) {
      this.emitUpdate({ description: this.editedDescription || undefined });
    }
    this.editingDescription.set(false);
  }

  cancelEditDescription(): void {
    this.editedDescription = this.task?.description ?? '';
    this.editingDescription.set(false);
  }

  onPriorityChange(): void {
    if (!this.task || this.selectedPriority === this.task.priority) return;
    this.emitUpdate({ priority: this.selectedPriority });
  }

  onDueDateChange(): void {
    if (!this.task) return;
    const currentDate = this.task.dueDate ? this.task.dueDate.split('T')[0] : '';
    if (this.selectedDueDate !== currentDate) {
      this.emitUpdate({ dueDate: this.selectedDueDate || undefined });
    }
  }

  startAddChecklistItem(): void {
    this.newChecklistText = '';
    this.addingChecklistItem.set(true);
  }

  cancelAddChecklistItem(): void {
    this.newChecklistText = '';
    this.addingChecklistItem.set(false);
  }

  addChecklistItem(): void {
    if (!this.newChecklistText.trim()) return;
    this.addChecklistItemEvent.emit(this.newChecklistText.trim());
    this.cancelAddChecklistItem();
  }

  confirmDelete(): void {
    if (confirm('Are you sure you want to delete this task?')) {
      this.deleteTask.emit();
    }
  }

  private emitUpdate(changes: Partial<UpdateTaskRequest>): void {
    if (!this.task) return;
    this.update.emit({
      title: changes.title ?? this.task.title,
      description: changes.description !== undefined ? changes.description : this.task.description,
      priority: changes.priority ?? this.task.priority,
      dueDate: changes.dueDate !== undefined ? changes.dueDate : this.task.dueDate,
      labelIds: this.task.labels.map(l => l.id)
    });
  }
}
