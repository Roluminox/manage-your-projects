import { Injectable, computed, inject, signal } from '@angular/core';
import { catchError, finalize, of, tap } from 'rxjs';
import {
  CreateSnippetRequest,
  CreateTagRequest,
  Snippet,
  SnippetFilters,
  SnippetSummary,
  Tag,
  UpdateSnippetRequest
} from '../models/snippet.models';
import { SnippetsApiService } from './snippets-api.service';

interface SnippetsState {
  snippets: SnippetSummary[];
  selectedSnippet: Snippet | null;
  tags: Tag[];
  totalCount: number;
  totalPages: number;
  filters: SnippetFilters;
  loading: boolean;
  error: string | null;
}

const initialFilters: SnippetFilters = {
  page: 1,
  pageSize: 10,
  sortDescending: true,
};

const initialState: SnippetsState = {
  snippets: [],
  selectedSnippet: null,
  tags: [],
  totalCount: 0,
  totalPages: 0,
  filters: initialFilters,
  loading: false,
  error: null,
};

@Injectable({
  providedIn: 'root'
})
export class SnippetsStateService {
  private readonly api = inject(SnippetsApiService);

  private readonly state = signal<SnippetsState>(initialState);

  readonly snippets = computed(() => this.state().snippets);
  readonly selectedSnippet = computed(() => this.state().selectedSnippet);
  readonly tags = computed(() => this.state().tags);
  readonly totalCount = computed(() => this.state().totalCount);
  readonly totalPages = computed(() => this.state().totalPages);
  readonly filters = computed(() => this.state().filters);
  readonly loading = computed(() => this.state().loading);
  readonly error = computed(() => this.state().error);

  readonly currentPage = computed(() => this.state().filters.page);
  readonly pageSize = computed(() => this.state().filters.pageSize);
  readonly hasNextPage = computed(() => this.state().filters.page < this.state().totalPages);
  readonly hasPreviousPage = computed(() => this.state().filters.page > 1);

  loadSnippets(): void {
    this.updateState({ loading: true, error: null });

    const filters = this.state().filters;
    const request$ = filters.searchTerm
      ? this.api.searchSnippets(filters.searchTerm, filters.page, filters.pageSize)
      : this.api.getSnippets(filters);

    request$.pipe(
      tap(response => {
        this.updateState({
          snippets: response.items,
          totalCount: response.totalCount,
          totalPages: response.totalPages,
        });
      }),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to load snippets' });
        return of(null);
      }),
      finalize(() => this.updateState({ loading: false }))
    ).subscribe();
  }

  loadSnippetById(id: string): void {
    this.updateState({ loading: true, error: null, selectedSnippet: null });

    this.api.getSnippetById(id).pipe(
      tap(snippet => this.updateState({ selectedSnippet: snippet })),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to load snippet' });
        return of(null);
      }),
      finalize(() => this.updateState({ loading: false }))
    ).subscribe();
  }

  loadTags(): void {
    this.api.getTags().pipe(
      tap(tags => this.updateState({ tags })),
      catchError(() => of([]))
    ).subscribe();
  }

  createSnippet(request: CreateSnippetRequest): void {
    this.updateState({ loading: true, error: null });

    this.api.createSnippet(request).pipe(
      tap(() => this.loadSnippets()),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to create snippet', loading: false });
        return of(null);
      })
    ).subscribe();
  }

  updateSnippet(id: string, request: UpdateSnippetRequest): void {
    this.updateState({ loading: true, error: null });

    this.api.updateSnippet(id, request).pipe(
      tap(snippet => {
        this.updateState({ selectedSnippet: snippet });
        this.loadSnippets();
      }),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to update snippet', loading: false });
        return of(null);
      })
    ).subscribe();
  }

  deleteSnippet(id: string): void {
    this.updateState({ loading: true, error: null });

    this.api.deleteSnippet(id).pipe(
      tap(() => {
        this.updateState({ selectedSnippet: null });
        this.loadSnippets();
      }),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to delete snippet', loading: false });
        return of(null);
      })
    ).subscribe();
  }

  toggleFavorite(id: string): void {
    this.api.toggleFavorite(id).pipe(
      tap(response => {
        const snippets = this.state().snippets.map(s =>
          s.id === id ? { ...s, isFavorite: response.isFavorite } : s
        );
        this.updateState({ snippets });

        const selected = this.state().selectedSnippet;
        if (selected?.id === id) {
          this.updateState({ selectedSnippet: { ...selected, isFavorite: response.isFavorite } });
        }
      }),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to toggle favorite' });
        return of(null);
      })
    ).subscribe();
  }

  createTag(request: CreateTagRequest): void {
    this.api.createTag(request).pipe(
      tap(() => this.loadTags()),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to create tag' });
        return of(null);
      })
    ).subscribe();
  }

  deleteTag(id: string): void {
    this.api.deleteTag(id).pipe(
      tap(() => this.loadTags()),
      catchError(error => {
        this.updateState({ error: error.error?.errors?.[0] || 'Failed to delete tag' });
        return of(null);
      })
    ).subscribe();
  }

  setFilters(filters: Partial<SnippetFilters>): void {
    const currentFilters = this.state().filters;
    const newFilters = { ...currentFilters, ...filters };

    if (filters.language !== undefined || filters.tagId !== undefined ||
        filters.isFavorite !== undefined || filters.searchTerm !== undefined) {
      newFilters.page = 1;
    }

    this.updateState({ filters: newFilters });
    this.loadSnippets();
  }

  setPage(page: number): void {
    this.setFilters({ page });
  }

  nextPage(): void {
    if (this.hasNextPage()) {
      this.setPage(this.currentPage() + 1);
    }
  }

  previousPage(): void {
    if (this.hasPreviousPage()) {
      this.setPage(this.currentPage() - 1);
    }
  }

  resetFilters(): void {
    this.updateState({ filters: initialFilters });
    this.loadSnippets();
  }

  clearSelectedSnippet(): void {
    this.updateState({ selectedSnippet: null });
  }

  clearError(): void {
    this.updateState({ error: null });
  }

  private updateState(partialState: Partial<SnippetsState>): void {
    this.state.update(state => ({ ...state, ...partialState }));
  }
}
