import { Component, input, output, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Tag } from '../../models/snippet.models';
import { TagBadgeComponent } from '../../../../shared/components/tag-badge/tag-badge.component';

@Component({
  selector: 'app-tag-selector',
  standalone: true,
  imports: [CommonModule, TagBadgeComponent],
  template: `
    <div class="tag-selector">
      <div class="selected-tags" *ngIf="selectedTags().length > 0">
        @for (tag of selectedTagObjects(); track tag.id) {
          <app-tag-badge
            [name]="tag.name"
            [color]="tag.color"
            [removable]="!disabled()"
            (remove)="removeTag(tag.id)"
          />
        }
      </div>

      <div class="selector-input" *ngIf="!disabled()">
        <div class="input-wrapper">
          <input
            type="text"
            class="tag-input"
            [placeholder]="placeholder()"
            [value]="searchTerm"
            (focus)="showDropdown = true"
            (input)="onSearchInput($event)"
            [disabled]="disabled()"
          />
          @if (searchTerm) {
            <button type="button" class="clear-btn" (click)="clearSearch()">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
              </svg>
            </button>
          }
        </div>

        @if (showDropdown && (filteredTags().length > 0 || canCreateTag())) {
          <div class="dropdown">
            @if (filteredTags().length > 0) {
              <div class="dropdown-section">
                <span class="dropdown-label">Available Tags</span>
                @for (tag of filteredTags(); track tag.id) {
                  <button
                    type="button"
                    class="dropdown-item"
                    (click)="selectTag(tag)"
                  >
                    <app-tag-badge [name]="tag.name" [color]="tag.color" />
                  </button>
                }
              </div>
            }

            @if (canCreateTag()) {
              <div class="dropdown-section">
                <button
                  type="button"
                  class="dropdown-item create-item"
                  (click)="createNewTag()"
                >
                  <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="icon">
                    <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
                  </svg>
                  Create "{{ searchTerm }}"
                </button>
              </div>
            }
          </div>
        }
      </div>

      @if (availableTags().length === 0 && selectedTags().length === 0) {
        <span class="no-tags-message">No tags available</span>
      }
    </div>
  `,
  styles: [`
    .tag-selector {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .selected-tags {
      display: flex;
      flex-wrap: wrap;
      gap: 0.375rem;
    }

    .selector-input {
      position: relative;
    }

    .input-wrapper {
      position: relative;
      display: flex;
      align-items: center;
    }

    .tag-input {
      width: 100%;
      padding: 0.5rem 2rem 0.5rem 0.75rem;
      border: 1px solid var(--border-color, #e2e8f0);
      border-radius: 0.375rem;
      font-size: 0.875rem;
      background: var(--bg-primary, #ffffff);
      color: var(--text-primary, #1e293b);
      transition: border-color 0.15s ease;
    }

    .tag-input:focus {
      outline: none;
      border-color: var(--primary, #6366f1);
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
    }

    .tag-input:disabled {
      background: var(--bg-secondary, #f8fafc);
      cursor: not-allowed;
    }

    .clear-btn {
      position: absolute;
      right: 0.5rem;
      padding: 0.25rem;
      border: none;
      background: transparent;
      color: var(--text-muted, #94a3b8);
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
    }

    .clear-btn:hover {
      color: var(--text-primary, #1e293b);
    }

    .clear-btn svg {
      width: 1rem;
      height: 1rem;
    }

    .dropdown {
      position: absolute;
      top: 100%;
      left: 0;
      right: 0;
      margin-top: 0.25rem;
      background: var(--bg-primary, #ffffff);
      border: 1px solid var(--border-color, #e2e8f0);
      border-radius: 0.375rem;
      box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
      z-index: 50;
      max-height: 200px;
      overflow-y: auto;
    }

    .dropdown-section {
      padding: 0.25rem;
    }

    .dropdown-section + .dropdown-section {
      border-top: 1px solid var(--border-color, #e2e8f0);
    }

    .dropdown-label {
      display: block;
      padding: 0.375rem 0.5rem;
      font-size: 0.75rem;
      font-weight: 500;
      color: var(--text-muted, #94a3b8);
      text-transform: uppercase;
      letter-spacing: 0.05em;
    }

    .dropdown-item {
      display: flex;
      align-items: center;
      width: 100%;
      padding: 0.5rem;
      border: none;
      background: transparent;
      cursor: pointer;
      border-radius: 0.25rem;
      transition: background-color 0.15s ease;
    }

    .dropdown-item:hover {
      background: var(--bg-secondary, #f8fafc);
    }

    .create-item {
      gap: 0.5rem;
      font-size: 0.875rem;
      color: var(--primary, #6366f1);
    }

    .icon {
      width: 1rem;
      height: 1rem;
    }

    .no-tags-message {
      font-size: 0.875rem;
      color: var(--text-muted, #94a3b8);
      font-style: italic;
    }
  `]
})
export class TagSelectorComponent {
  readonly availableTags = input<Tag[]>([]);
  readonly selectedTags = input<string[]>([]);
  readonly disabled = input<boolean>(false);
  readonly allowCreate = input<boolean>(true);
  readonly placeholder = input<string>('Search or create tags...');

  readonly selectionChange = output<string[]>();
  readonly createTag = output<string>();

  searchTerm = '';
  showDropdown = false;

  readonly selectedTagObjects = computed(() => {
    const selected = this.selectedTags();
    return this.availableTags().filter(tag => selected.includes(tag.id));
  });

  readonly filteredTags = computed(() => {
    const selected = this.selectedTags();
    const search = this.searchTerm.toLowerCase();

    return this.availableTags()
      .filter(tag => !selected.includes(tag.id))
      .filter(tag => !search || tag.name.toLowerCase().includes(search));
  });

  canCreateTag(): boolean {
    if (!this.allowCreate() || !this.searchTerm.trim()) {
      return false;
    }

    const normalizedSearch = this.searchTerm.trim().toLowerCase();
    const exists = this.availableTags().some(
      tag => tag.name.toLowerCase() === normalizedSearch
    );

    return !exists;
  }

  selectTag(tag: Tag): void {
    const current = this.selectedTags();
    if (!current.includes(tag.id)) {
      this.selectionChange.emit([...current, tag.id]);
    }
    this.clearSearch();
  }

  removeTag(tagId: string): void {
    const current = this.selectedTags();
    this.selectionChange.emit(current.filter(id => id !== tagId));
  }

  createNewTag(): void {
    const name = this.searchTerm.trim();
    if (name) {
      this.createTag.emit(name);
      this.clearSearch();
    }
  }

  onSearchInput(event: Event): void {
    const target = event.target as HTMLInputElement;
    this.searchTerm = target.value;
    this.showDropdown = true;
  }

  clearSearch(): void {
    this.searchTerm = '';
    this.showDropdown = false;
  }
}
