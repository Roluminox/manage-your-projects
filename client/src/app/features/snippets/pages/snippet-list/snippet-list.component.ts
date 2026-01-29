import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { SnippetsStateService } from '../../services/snippets-state.service';
import { SnippetCardComponent } from '../../components/snippet-card/snippet-card.component';
import { SnippetFiltersComponent } from '../../components/snippet-filters/snippet-filters.component';
import { SnippetFormComponent } from '../../components/snippet-form/snippet-form.component';
import { ClipboardService } from '../../../../shared/services/clipboard.service';
import { CreateSnippetRequest, Snippet, SnippetFilters, UpdateSnippetRequest } from '../../models/snippet.models';

type ViewMode = 'list' | 'create' | 'edit' | 'view';

@Component({
  selector: 'app-snippet-list',
  standalone: true,
  imports: [
    CommonModule,
    SnippetCardComponent,
    SnippetFiltersComponent,
    SnippetFormComponent
  ],
  template: `
    <div class="snippet-list-page">
      <header class="page-header">
        <h1 class="page-title">My Snippets</h1>
        @if (viewMode() === 'list') {
          <button class="btn btn-primary" (click)="showCreateForm()">
            <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="icon">
              <path stroke-linecap="round" stroke-linejoin="round" d="M12 4.5v15m7.5-7.5h-15" />
            </svg>
            New Snippet
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
          <app-snippet-filters
            [filters]="state.filters()"
            [tags]="state.tags()"
            (filtersChange)="onFiltersChange($event)"
            (resetFilters)="state.resetFilters()"
          />

          @if (state.loading()) {
            <div class="loading-state">
              <div class="spinner"></div>
              <span>Loading snippets...</span>
            </div>
          } @else if (state.snippets().length === 0) {
            <div class="empty-state">
              <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor" class="empty-icon">
                <path stroke-linecap="round" stroke-linejoin="round" d="M19.5 14.25v-2.625a3.375 3.375 0 00-3.375-3.375h-1.5A1.125 1.125 0 0113.5 7.125v-1.5a3.375 3.375 0 00-3.375-3.375H8.25m0 12.75h7.5m-7.5 3H12M10.5 2.25H5.625c-.621 0-1.125.504-1.125 1.125v17.25c0 .621.504 1.125 1.125 1.125h12.75c.621 0 1.125-.504 1.125-1.125V11.25a9 9 0 00-9-9z" />
              </svg>
              <h2>No snippets found</h2>
              <p>Create your first snippet to get started</p>
              <button class="btn btn-primary" (click)="showCreateForm()">
                Create Snippet
              </button>
            </div>
          } @else {
            <div class="snippets-grid">
              @for (snippet of state.snippets(); track snippet.id) {
                <app-snippet-card
                  [snippet]="snippet"
                  (cardClick)="viewSnippet(snippet.id)"
                  (favoriteClick)="state.toggleFavorite(snippet.id)"
                />
              }
            </div>

            @if (state.totalPages() > 1) {
              <div class="pagination">
                <button
                  class="btn btn-secondary"
                  [disabled]="!state.hasPreviousPage()"
                  (click)="state.previousPage()"
                >
                  Previous
                </button>
                <span class="page-info">
                  Page {{ state.currentPage() }} of {{ state.totalPages() }}
                </span>
                <button
                  class="btn btn-secondary"
                  [disabled]="!state.hasNextPage()"
                  (click)="state.nextPage()"
                >
                  Next
                </button>
              </div>
            }
          }
        }

        @case ('create') {
          <div class="form-container">
            <h2 class="form-title">Create New Snippet</h2>
            <app-snippet-form
              [availableTags]="state.tags()"
              [loading]="state.loading()"
              (save)="onCreateSnippet($event)"
              (cancel)="backToList()"
            />
          </div>
        }

        @case ('edit') {
          <div class="form-container">
            <h2 class="form-title">Edit Snippet</h2>
            <app-snippet-form
              [snippet]="state.selectedSnippet()"
              [availableTags]="state.tags()"
              [loading]="state.loading()"
              (save)="onUpdateSnippet($event)"
              (cancel)="backToList()"
            />
          </div>
        }

        @case ('view') {
          @if (state.selectedSnippet(); as snippet) {
            <div class="snippet-detail">
              <div class="detail-header">
                <div class="detail-info">
                  <h2 class="detail-title">{{ snippet.title }}</h2>
                  <div class="detail-meta">
                    <span class="language-badge">{{ snippet.language }}</span>
                    <span class="date">Created: {{ snippet.createdAt | date:'medium' }}</span>
                    @if (snippet.updatedAt) {
                      <span class="date">Updated: {{ snippet.updatedAt | date:'medium' }}</span>
                    }
                  </div>
                </div>
                <div class="detail-actions">
                  <button class="btn btn-icon" (click)="copyCode(snippet.code)" title="Copy code">
                    <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                      <path stroke-linecap="round" stroke-linejoin="round" d="M15.666 3.888A2.25 2.25 0 0013.5 2.25h-3c-1.03 0-1.9.693-2.166 1.638m7.332 0c.055.194.084.4.084.612v0a.75.75 0 01-.75.75H9a.75.75 0 01-.75-.75v0c0-.212.03-.418.084-.612m7.332 0c.646.049 1.288.11 1.927.184 1.1.128 1.907 1.077 1.907 2.185V19.5a2.25 2.25 0 01-2.25 2.25H6.75A2.25 2.25 0 014.5 19.5V6.257c0-1.108.806-2.057 1.907-2.185a48.208 48.208 0 011.927-.184" />
                    </svg>
                  </button>
                  <button
                    class="btn btn-icon"
                    [class.is-favorite]="snippet.isFavorite"
                    (click)="state.toggleFavorite(snippet.id)"
                    title="Toggle favorite"
                  >
                    @if (snippet.isFavorite) {
                      <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor">
                        <path d="M11.645 20.91l-.007-.003-.022-.012a15.247 15.247 0 01-.383-.218 25.18 25.18 0 01-4.244-3.17C4.688 15.36 2.25 12.174 2.25 8.25 2.25 5.322 4.714 3 7.688 3A5.5 5.5 0 0112 5.052 5.5 5.5 0 0116.313 3c2.973 0 5.437 2.322 5.437 5.25 0 3.925-2.438 7.111-4.739 9.256a25.175 25.175 0 01-4.244 3.17 15.247 15.247 0 01-.383.219l-.022.012-.007.004-.003.001a.752.752 0 01-.704 0l-.003-.001z" />
                      </svg>
                    } @else {
                      <svg xmlns="http://www.w3.org/2000/svg" fill="none" viewBox="0 0 24 24" stroke-width="1.5" stroke="currentColor">
                        <path stroke-linecap="round" stroke-linejoin="round" d="M21 8.25c0-2.485-2.099-4.5-4.688-4.5-1.935 0-3.597 1.126-4.312 2.733-.715-1.607-2.377-2.733-4.313-2.733C5.1 3.75 3 5.765 3 8.25c0 7.22 9 12 9 12s9-4.78 9-12z" />
                      </svg>
                    }
                  </button>
                  <button class="btn btn-secondary" (click)="editSnippet()">
                    Edit
                  </button>
                  <button class="btn btn-danger" (click)="deleteSnippet(snippet.id)">
                    Delete
                  </button>
                  <button class="btn btn-secondary" (click)="backToList()">
                    Back
                  </button>
                </div>
              </div>

              @if (snippet.description) {
                <p class="detail-description">{{ snippet.description }}</p>
              }

              @if (snippet.tags.length > 0) {
                <div class="detail-tags">
                  @for (tag of snippet.tags; track tag.id) {
                    <span class="tag-badge" [style.background-color]="tag.color">
                      {{ tag.name }}
                    </span>
                  }
                </div>
              }

              <div class="code-block">
                <pre><code>{{ snippet.code }}</code></pre>
              </div>
            </div>
          }
        }
      }

      @if (copySuccess()) {
        <div class="toast">Code copied to clipboard!</div>
      }
    </div>
  `,
  styles: [`
    .snippet-list-page {
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
    }

    .btn-primary {
      background: var(--primary, #6366f1);
      color: white;
      border: none;
    }

    .btn-primary:hover {
      background: var(--primary-dark, #4f46e5);
    }

    .btn-secondary {
      background: transparent;
      color: var(--text-primary, #1e293b);
      border: 1px solid var(--border-color, #e2e8f0);
    }

    .btn-secondary:hover:not(:disabled) {
      background: var(--bg-secondary, #f8fafc);
    }

    .btn-secondary:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .btn-danger {
      background: var(--danger, #ef4444);
      color: white;
      border: none;
    }

    .btn-danger:hover {
      background: #dc2626;
    }

    .btn-icon {
      padding: 0.5rem;
      background: transparent;
      border: 1px solid var(--border-color, #e2e8f0);
      border-radius: 0.375rem;
      color: var(--text-muted, #94a3b8);
      cursor: pointer;
    }

    .btn-icon:hover {
      background: var(--bg-secondary, #f8fafc);
      color: var(--text-primary, #1e293b);
    }

    .btn-icon.is-favorite {
      color: var(--danger, #ef4444);
    }

    .btn-icon svg {
      width: 1.25rem;
      height: 1.25rem;
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

    .snippets-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
      gap: 1rem;
      margin-top: 1.5rem;
    }

    .pagination {
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 1rem;
      margin-top: 2rem;
    }

    .page-info {
      font-size: 0.875rem;
      color: var(--text-muted, #94a3b8);
    }

    .form-container {
      max-width: 600px;
    }

    .form-title {
      font-size: 1.25rem;
      font-weight: 600;
      margin: 0 0 1.5rem;
      color: var(--text-primary, #1e293b);
    }

    .snippet-detail {
      max-width: 800px;
    }

    .detail-header {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      gap: 1rem;
      margin-bottom: 1rem;
    }

    .detail-title {
      font-size: 1.25rem;
      font-weight: 600;
      margin: 0 0 0.5rem;
      color: var(--text-primary, #1e293b);
    }

    .detail-meta {
      display: flex;
      flex-wrap: wrap;
      align-items: center;
      gap: 0.75rem;
      font-size: 0.75rem;
      color: var(--text-muted, #94a3b8);
    }

    .language-badge {
      background: var(--primary-light, #eef2ff);
      color: var(--primary, #6366f1);
      padding: 0.125rem 0.5rem;
      border-radius: 0.25rem;
      font-weight: 500;
      text-transform: uppercase;
      font-size: 0.625rem;
      letter-spacing: 0.05em;
    }

    .detail-actions {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
    }

    .detail-description {
      margin: 0 0 1rem;
      color: var(--text-secondary, #64748b);
      line-height: 1.6;
    }

    .detail-tags {
      display: flex;
      flex-wrap: wrap;
      gap: 0.5rem;
      margin-bottom: 1.5rem;
    }

    .tag-badge {
      padding: 0.25rem 0.5rem;
      border-radius: 9999px;
      font-size: 0.75rem;
      font-weight: 500;
      color: white;
    }

    .code-block {
      background: #1e293b;
      border-radius: 0.5rem;
      overflow: hidden;
    }

    .code-block pre {
      margin: 0;
      padding: 1rem;
      overflow-x: auto;
    }

    .code-block code {
      font-family: ui-monospace, SFMono-Regular, Menlo, Monaco, Consolas, monospace;
      font-size: 0.875rem;
      line-height: 1.5;
      color: #e2e8f0;
    }

    .toast {
      position: fixed;
      bottom: 1.5rem;
      left: 50%;
      transform: translateX(-50%);
      padding: 0.75rem 1.5rem;
      background: #1e293b;
      color: white;
      border-radius: 0.5rem;
      font-size: 0.875rem;
      box-shadow: 0 10px 15px -3px rgba(0, 0, 0, 0.1);
      animation: slideUp 0.3s ease;
    }

    @keyframes slideUp {
      from {
        opacity: 0;
        transform: translate(-50%, 1rem);
      }
      to {
        opacity: 1;
        transform: translate(-50%, 0);
      }
    }
  `]
})
export class SnippetListComponent implements OnInit {
  readonly state = inject(SnippetsStateService);
  private readonly clipboard = inject(ClipboardService);

  readonly viewMode = signal<ViewMode>('list');
  readonly copySuccess = signal(false);

  ngOnInit(): void {
    this.state.loadSnippets();
    this.state.loadTags();
  }

  showCreateForm(): void {
    this.state.clearSelectedSnippet();
    this.viewMode.set('create');
  }

  viewSnippet(id: string): void {
    this.state.loadSnippetById(id);
    this.viewMode.set('view');
  }

  editSnippet(): void {
    this.viewMode.set('edit');
  }

  backToList(): void {
    this.state.clearSelectedSnippet();
    this.viewMode.set('list');
  }

  onFiltersChange(filters: Partial<SnippetFilters>): void {
    this.state.setFilters(filters);
  }

  onCreateSnippet(request: CreateSnippetRequest | UpdateSnippetRequest): void {
    this.state.createSnippet(request as CreateSnippetRequest);
    this.viewMode.set('list');
  }

  onUpdateSnippet(request: CreateSnippetRequest | UpdateSnippetRequest): void {
    const snippet = this.state.selectedSnippet();
    if (snippet) {
      this.state.updateSnippet(snippet.id, request as UpdateSnippetRequest);
      this.viewMode.set('view');
    }
  }

  deleteSnippet(id: string): void {
    if (confirm('Are you sure you want to delete this snippet?')) {
      this.state.deleteSnippet(id);
      this.viewMode.set('list');
    }
  }

  async copyCode(code: string): Promise<void> {
    const success = await this.clipboard.copy(code);
    if (success) {
      this.copySuccess.set(true);
      setTimeout(() => this.copySuccess.set(false), 2000);
    }
  }
}
