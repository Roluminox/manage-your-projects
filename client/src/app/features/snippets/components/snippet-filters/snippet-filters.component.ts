import { Component, input, output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { SnippetFilters, SUPPORTED_LANGUAGES, Tag } from '../../models/snippet.models';

@Component({
  selector: 'app-snippet-filters',
  standalone: true,
  imports: [CommonModule, FormsModule],
  template: `
    <div class="filters-container">
      <div class="search-box">
        <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="search-icon">
          <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-5.197-5.197m0 0A7.5 7.5 0 105.196 5.196a7.5 7.5 0 0010.607 10.607z" />
        </svg>
        <input
          type="text"
          class="search-input"
          placeholder="Search snippets..."
          [ngModel]="filters().searchTerm || ''"
          (ngModelChange)="onSearchChange($event)"
        />
        @if (filters().searchTerm) {
          <button type="button" class="clear-btn" (click)="onSearchChange('')">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        }
      </div>

      <div class="filter-row">
        <select
          class="filter-select"
          [ngModel]="filters().language || ''"
          (ngModelChange)="onLanguageChange($event)"
        >
          <option value="">All Languages</option>
          @for (lang of languages; track lang.value) {
            <option [value]="lang.value">{{ lang.label }}</option>
          }
        </select>

        <select
          class="filter-select"
          [ngModel]="filters().tagId || ''"
          (ngModelChange)="onTagChange($event)"
        >
          <option value="">All Tags</option>
          @for (tag of tags(); track tag.id) {
            <option [value]="tag.id">{{ tag.name }}</option>
          }
        </select>

        <select
          class="filter-select"
          [ngModel]="favoriteValue"
          (ngModelChange)="onFavoriteChange($event)"
        >
          <option value="">All Snippets</option>
          <option value="true">Favorites Only</option>
          <option value="false">Non-Favorites</option>
        </select>

        @if (hasActiveFilters) {
          <button type="button" class="reset-btn" (click)="resetFilters.emit()">
            Reset Filters
          </button>
        }
      </div>
    </div>
  `,
  styles: [`
    .filters-container {
      display: flex;
      flex-direction: column;
      gap: 1rem;
    }

    .search-box {
      position: relative;
      display: flex;
      align-items: center;
    }

    .search-icon {
      position: absolute;
      left: 0.75rem;
      width: 1.25rem;
      height: 1.25rem;
      color: var(--text-muted, #94a3b8);
      pointer-events: none;
    }

    .search-input {
      width: 100%;
      padding: 0.625rem 2.5rem;
      border: 1px solid var(--border-color, #e2e8f0);
      border-radius: 0.5rem;
      font-size: 0.875rem;
      background: var(--bg-primary, #ffffff);
      color: var(--text-primary, #1e293b);
      transition: border-color 0.15s ease;
    }

    .search-input:focus {
      outline: none;
      border-color: var(--primary, #6366f1);
      box-shadow: 0 0 0 3px rgba(99, 102, 241, 0.1);
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

    .filter-row {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
      align-items: center;
    }

    .filter-select {
      padding: 0.5rem 0.75rem;
      border: 1px solid var(--border-color, #e2e8f0);
      border-radius: 0.375rem;
      font-size: 0.875rem;
      background: var(--bg-primary, #ffffff);
      color: var(--text-primary, #1e293b);
      cursor: pointer;
      min-width: 140px;
    }

    .filter-select:focus {
      outline: none;
      border-color: var(--primary, #6366f1);
    }

    .reset-btn {
      padding: 0.5rem 0.75rem;
      border: 1px solid var(--border-color, #e2e8f0);
      border-radius: 0.375rem;
      font-size: 0.875rem;
      background: transparent;
      color: var(--text-muted, #94a3b8);
      cursor: pointer;
      transition: all 0.15s ease;
    }

    .reset-btn:hover {
      border-color: var(--danger, #ef4444);
      color: var(--danger, #ef4444);
    }
  `]
})
export class SnippetFiltersComponent {
  readonly filters = input.required<SnippetFilters>();
  readonly tags = input<Tag[]>([]);

  readonly filtersChange = output<Partial<SnippetFilters>>();
  readonly resetFilters = output<void>();

  readonly languages = SUPPORTED_LANGUAGES;

  private searchTimeout?: number;

  get favoriteValue(): string {
    const isFavorite = this.filters().isFavorite;
    return isFavorite === undefined ? '' : isFavorite.toString();
  }

  get hasActiveFilters(): boolean {
    const f = this.filters();
    return !!(f.searchTerm || f.language || f.tagId || f.isFavorite !== undefined);
  }

  onSearchChange(value: string): void {
    if (this.searchTimeout) {
      clearTimeout(this.searchTimeout);
    }

    this.searchTimeout = window.setTimeout(() => {
      this.filtersChange.emit({ searchTerm: value || undefined });
    }, 300);
  }

  onLanguageChange(value: string): void {
    this.filtersChange.emit({ language: value || undefined });
  }

  onTagChange(value: string): void {
    this.filtersChange.emit({ tagId: value || undefined });
  }

  onFavoriteChange(value: string): void {
    const isFavorite = value === '' ? undefined : value === 'true';
    this.filtersChange.emit({ isFavorite });
  }
}
