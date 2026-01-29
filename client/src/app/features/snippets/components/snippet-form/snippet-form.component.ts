import { Component, input, output, OnInit, effect } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CreateSnippetRequest, Snippet, SUPPORTED_LANGUAGES, Tag, UpdateSnippetRequest } from '../../models/snippet.models';
import { TagBadgeComponent } from '../../../../shared/components/tag-badge/tag-badge.component';

@Component({
  selector: 'app-snippet-form',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, TagBadgeComponent],
  template: `
    <form [formGroup]="form" (ngSubmit)="onSubmit()" class="snippet-form">
      <div class="form-group">
        <label for="title" class="form-label">Title *</label>
        <input
          type="text"
          id="title"
          formControlName="title"
          class="form-input"
          placeholder="Enter snippet title"
        />
        @if (form.get('title')?.invalid && form.get('title')?.touched) {
          <span class="error-message">Title is required (max 200 characters)</span>
        }
      </div>

      <div class="form-group">
        <label for="language" class="form-label">Language *</label>
        <select id="language" formControlName="language" class="form-select">
          <option value="">Select a language</option>
          @for (lang of languages; track lang.value) {
            <option [value]="lang.value">{{ lang.label }}</option>
          }
        </select>
        @if (form.get('language')?.invalid && form.get('language')?.touched) {
          <span class="error-message">Language is required</span>
        }
      </div>

      <div class="form-group">
        <label for="code" class="form-label">Code *</label>
        <textarea
          id="code"
          formControlName="code"
          class="form-textarea code-textarea"
          placeholder="Paste your code here"
          rows="10"
        ></textarea>
        @if (form.get('code')?.invalid && form.get('code')?.touched) {
          <span class="error-message">Code is required</span>
        }
      </div>

      <div class="form-group">
        <label for="description" class="form-label">Description</label>
        <textarea
          id="description"
          formControlName="description"
          class="form-textarea"
          placeholder="Add a description (optional)"
          rows="3"
        ></textarea>
        @if (form.get('description')?.invalid && form.get('description')?.touched) {
          <span class="error-message">Description must not exceed 1000 characters</span>
        }
      </div>

      <div class="form-group">
        <label class="form-label">Tags</label>
        <div class="tags-selector">
          @for (tag of availableTags(); track tag.id) {
            <button
              type="button"
              class="tag-option"
              [class.selected]="isTagSelected(tag.id)"
              (click)="toggleTag(tag.id)"
            >
              <app-tag-badge [name]="tag.name" [color]="tag.color" />
            </button>
          }
          @if (availableTags().length === 0) {
            <span class="no-tags">No tags available. Create some tags first.</span>
          }
        </div>
      </div>

      <div class="form-actions">
        <button type="button" class="btn btn-secondary" (click)="cancel.emit()">
          Cancel
        </button>
        <button
          type="submit"
          class="btn btn-primary"
          [disabled]="form.invalid || loading()"
        >
          @if (loading()) {
            Saving...
          } @else {
            {{ snippet() ? 'Update' : 'Create' }} Snippet
          }
        </button>
      </div>
    </form>
  `,
  styles: [`
    .snippet-form {
      display: flex;
      flex-direction: column;
      gap: 1.25rem;
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 0.375rem;
    }

    .form-label {
      font-size: 0.875rem;
      font-weight: 500;
      color: var(--text-primary, #1e293b);
    }

    .form-input,
    .form-select,
    .form-textarea {
      padding: 0.625rem 0.75rem;
      border: 1px solid var(--border-color, #e2e8f0);
      border-radius: 0.375rem;
      font-size: 0.875rem;
      background: var(--bg-primary, #ffffff);
      color: var(--text-primary, #1e293b);
      transition: border-color 0.15s ease;
    }

    .form-input:focus,
    .form-select:focus,
    .form-textarea:focus {
      outline: none;
      border-color: var(--primary, #6366f1);
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
    }

    .code-textarea {
      font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
      font-size: 0.8125rem;
      line-height: 1.5;
      tab-size: 2;
      resize: vertical;
    }

    .error-message {
      font-size: 0.75rem;
      color: var(--danger, #ef4444);
    }

    .tags-selector {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
    }

    .tag-option {
      padding: 0;
      border: 2px solid transparent;
      background: transparent;
      border-radius: 9999px;
      cursor: pointer;
      transition: border-color 0.15s ease;
    }

    .tag-option:hover {
      border-color: var(--border-color, #e2e8f0);
    }

    .tag-option.selected {
      border-color: var(--primary, #6366f1);
    }

    .no-tags {
      font-size: 0.875rem;
      color: var(--text-muted, #94a3b8);
      font-style: italic;
    }

    .form-actions {
      display: flex;
      justify-content: flex-end;
      gap: 0.75rem;
      margin-top: 0.5rem;
    }

    .btn {
      padding: 0.625rem 1.25rem;
      border-radius: 0.375rem;
      font-size: 0.875rem;
      font-weight: 500;
      cursor: pointer;
      transition: all 0.15s ease;
    }

    .btn-primary {
      background: var(--primary, #6366f1);
      color: white;
      border: none;
    }

    .btn-primary:hover:not(:disabled) {
      background: var(--primary-dark, #4f46e5);
    }

    .btn-primary:disabled {
      opacity: 0.5;
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
  `]
})
export class SnippetFormComponent implements OnInit {
  readonly snippet = input<Snippet | null>(null);
  readonly availableTags = input<Tag[]>([]);
  readonly loading = input<boolean>(false);

  readonly save = output<CreateSnippetRequest | UpdateSnippetRequest>();
  readonly cancel = output<void>();

  readonly languages = SUPPORTED_LANGUAGES;

  form!: FormGroup;
  selectedTagIds: Set<string> = new Set();
  private initialized = false;

  constructor(private fb: FormBuilder) {
    // Use effect to watch for snippet changes
    effect(() => {
      const snippet = this.snippet();
      if (this.initialized) {
        this.populateForm(snippet);
      }
    });
  }

  ngOnInit(): void {
    this.initForm();
    this.populateForm(this.snippet());
    this.initialized = true;
  }

  private initForm(): void {
    this.form = this.fb.group({
      title: ['', [Validators.required, Validators.maxLength(200)]],
      language: ['', Validators.required],
      code: ['', Validators.required],
      description: ['', Validators.maxLength(1000)]
    });
  }

  private populateForm(snippet: Snippet | null): void {
    if (snippet) {
      this.form.patchValue({
        title: snippet.title,
        language: snippet.language,
        code: snippet.code,
        description: snippet.description || ''
      });
      this.selectedTagIds = new Set(snippet.tags.map(t => t.id));
    } else {
      this.form?.reset();
      this.selectedTagIds.clear();
    }
  }

  isTagSelected(tagId: string): boolean {
    return this.selectedTagIds.has(tagId);
  }

  toggleTag(tagId: string): void {
    if (this.selectedTagIds.has(tagId)) {
      this.selectedTagIds.delete(tagId);
    } else {
      this.selectedTagIds.add(tagId);
    }
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    const formValue = this.form.value;
    const request: CreateSnippetRequest | UpdateSnippetRequest = {
      title: formValue.title,
      code: formValue.code,
      language: formValue.language,
      description: formValue.description || undefined,
      tagIds: Array.from(this.selectedTagIds)
    };

    this.save.emit(request);
  }
}
