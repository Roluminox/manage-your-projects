import { Component, EventEmitter, Input, OnInit, Output, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import {
  CreateTaskRequest,
  Label,
  Priority,
  PRIORITY_LABELS,
  TaskItem,
  UpdateTaskRequest
} from '../../models/kanban.models';

export interface TaskFormData {
  title: string;
  description?: string;
  priority: Priority;
  dueDate?: string;
  labelIds: string[];
}

@Component({
  selector: 'app-task-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <form [formGroup]="form" (ngSubmit)="onSubmit()" class="task-form">
      <!-- Title -->
      <div class="field-group">
        <label for="title" class="field-label">
          Title <span class="required">*</span>
        </label>
        <input
          id="title"
          type="text"
          formControlName="title"
          class="form-input"
          [class.error]="isFieldInvalid('title')"
          placeholder="Enter task title..."
        />
        @if (isFieldInvalid('title')) {
          <span class="error-message">
            @if (form.get('title')?.errors?.['required']) {
              Title is required
            } @else if (form.get('title')?.errors?.['minlength']) {
              Title must be at least 3 characters
            } @else if (form.get('title')?.errors?.['maxlength']) {
              Title must be less than 200 characters
            }
          </span>
        }
      </div>

      <!-- Description -->
      <div class="field-group">
        <label for="description" class="field-label">Description</label>
        <textarea
          id="description"
          formControlName="description"
          class="form-textarea"
          rows="3"
          placeholder="Add a description..."
        ></textarea>
      </div>

      <!-- Priority & Due Date -->
      <div class="field-row">
        <div class="field-group half">
          <label for="priority" class="field-label">Priority</label>
          <select id="priority" formControlName="priority" class="form-select">
            @for (option of priorityOptions; track option.value) {
              <option [value]="option.value">{{ option.label }}</option>
            }
          </select>
        </div>
        <div class="field-group half">
          <label for="dueDate" class="field-label">Due Date</label>
          <input
            id="dueDate"
            type="date"
            formControlName="dueDate"
            class="form-input"
          />
        </div>
      </div>

      <!-- Labels -->
      @if (availableLabels.length > 0) {
        <div class="field-group">
          <label class="field-label">Labels</label>
          <div class="labels-container">
            @for (label of availableLabels; track label.id) {
              <button
                type="button"
                class="label-chip"
                [class.selected]="isLabelSelected(label.id)"
                [style.--label-color]="label.color"
                (click)="toggleLabel(label.id)"
              >
                <span class="label-dot" [style.background]="label.color"></span>
                {{ label.name }}
                @if (isLabelSelected(label.id)) {
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="2" stroke="currentColor" class="check-icon">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M4.5 12.75l6 6 9-13.5" />
                  </svg>
                }
              </button>
            }
          </div>
        </div>
      }

      <!-- Actions -->
      <div class="form-actions">
        <button type="button" class="btn btn-secondary" (click)="onCancel()">
          Cancel
        </button>
        <button
          type="submit"
          class="btn btn-primary"
          [disabled]="form.invalid || submitting()"
        >
          @if (submitting()) {
            <span class="spinner"></span>
            Saving...
          } @else {
            {{ task ? 'Update' : 'Create' }} Task
          }
        </button>
      </div>
    </form>
  `,
  styles: [`
    .task-form {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .field-group {
      display: flex;
      flex-direction: column;
      gap: 0.375rem;
    }

    .field-row {
      display: flex;
      gap: 1rem;
    }

    .half {
      flex: 1;
    }

    .field-label {
      font-size: 0.875rem;
      font-weight: 500;
      color: var(--text-primary, #1e293b);
    }

    .required {
      color: var(--danger, #ef4444);
    }

    .form-input,
    .form-select,
    .form-textarea {
      width: 100%;
      padding: 0.625rem 0.75rem;
      font-size: 0.875rem;
      border: 1px solid var(--border-color, #e2e8f0);
      border-radius: 0.375rem;
      background: var(--bg-card, #ffffff);
      color: var(--text-primary, #1e293b);
      transition: border-color 0.15s ease, box-shadow 0.15s ease;
    }

    .form-input:focus,
    .form-select:focus,
    .form-textarea:focus {
      outline: none;
      border-color: var(--primary, #6366f1);
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
    }

    .form-input.error,
    .form-textarea.error {
      border-color: var(--danger, #ef4444);
    }

    .form-input.error:focus,
    .form-textarea.error:focus {
      box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.1);
    }

    .form-textarea {
      resize: vertical;
      min-height: 80px;
    }

    .error-message {
      font-size: 0.75rem;
      color: var(--danger, #ef4444);
    }

    .labels-container {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
    }

    .label-chip {
      display: inline-flex;
      align-items: center;
      gap: 0.375rem;
      padding: 0.375rem 0.625rem;
      font-size: 0.8125rem;
      border: 1px solid var(--border-color, #e2e8f0);
      border-radius: 9999px;
      background: var(--bg-card, #ffffff);
      color: var(--text-secondary, #64748b);
      cursor: pointer;
      transition: all 0.15s ease;
    }

    .label-chip:hover {
      border-color: var(--label-color);
      background: color-mix(in srgb, var(--label-color) 10%, transparent);
    }

    .label-chip.selected {
      border-color: var(--label-color);
      background: color-mix(in srgb, var(--label-color) 15%, transparent);
      color: var(--text-primary, #1e293b);
    }

    .label-dot {
      width: 0.5rem;
      height: 0.5rem;
      border-radius: 50%;
    }

    .check-icon {
      width: 0.875rem;
      height: 0.875rem;
      color: var(--label-color);
    }

    .form-actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.75rem;
      margin-top: 0.5rem;
      padding-top: 1rem;
      border-top: 1px solid var(--border-color, #e2e8f0);
    }

    .btn {
      display: inline-flex;
      align-items: center;
      justify-content: center;
      gap: 0.5rem;
      padding: 0.5rem 1rem;
      min-width: 100px;
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

    .spinner {
      width: 1rem;
      height: 1rem;
      border: 2px solid rgba(255, 255, 255, 0.3);
      border-top-color: white;
      border-radius: 50%;
      animation: spin 0.6s linear infinite;
    }

    @keyframes spin {
      to { transform: rotate(360deg); }
    }
  `]
})
export class TaskFormComponent implements OnInit {
  @Input() task: TaskItem | null = null;
  @Input() availableLabels: Label[] = [];

  @Output() save = new EventEmitter<TaskFormData>();
  @Output() cancel = new EventEmitter<void>();

  form!: FormGroup;
  readonly submitting = signal(false);

  readonly priorityOptions = [
    { value: Priority.Low, label: PRIORITY_LABELS[Priority.Low] },
    { value: Priority.Medium, label: PRIORITY_LABELS[Priority.Medium] },
    { value: Priority.High, label: PRIORITY_LABELS[Priority.High] },
    { value: Priority.Critical, label: PRIORITY_LABELS[Priority.Critical] }
  ];

  private selectedLabelIds = new Set<string>();

  constructor(private fb: FormBuilder) {}

  ngOnInit(): void {
    this.initForm();
    if (this.task) {
      this.populateForm(this.task);
    }
  }

  private initForm(): void {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.minLength(3), Validators.maxLength(200)]],
      description: [''],
      priority: [Priority.Medium],
      dueDate: ['']
    });
  }

  private populateForm(task: TaskItem): void {
    this.form.patchValue({
      title: task.title,
      description: task.description ?? '',
      priority: task.priority,
      dueDate: task.dueDate ? task.dueDate.split('T')[0] : ''
    });
    this.selectedLabelIds = new Set(task.labels.map(l => l.id));
  }

  isFieldInvalid(fieldName: string): boolean {
    const field = this.form.get(fieldName);
    return field ? field.invalid && (field.dirty || field.touched) : false;
  }

  isLabelSelected(labelId: string): boolean {
    return this.selectedLabelIds.has(labelId);
  }

  toggleLabel(labelId: string): void {
    if (this.selectedLabelIds.has(labelId)) {
      this.selectedLabelIds.delete(labelId);
    } else {
      this.selectedLabelIds.add(labelId);
    }
  }

  onSubmit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const formValue = this.form.value;
    const data: TaskFormData = {
      title: formValue.title.trim(),
      description: formValue.description?.trim() || undefined,
      priority: Number(formValue.priority) as Priority,
      dueDate: formValue.dueDate || undefined,
      labelIds: Array.from(this.selectedLabelIds)
    };

    this.save.emit(data);
  }

  onCancel(): void {
    this.cancel.emit();
  }

  setSubmitting(value: boolean): void {
    this.submitting.set(value);
  }
}
