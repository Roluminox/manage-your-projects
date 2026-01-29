import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { vi } from 'vitest';
import { SnippetListComponent } from './snippet-list.component';
import { SnippetsStateService } from '../../services/snippets-state.service';
import { ClipboardService } from '../../../../shared/services/clipboard.service';
import { SnippetFilters, SnippetSummary, Tag } from '../../models/snippet.models';

describe('SnippetListComponent', () => {
  let component: SnippetListComponent;
  let fixture: ComponentFixture<SnippetListComponent>;
  let stateServiceMock: Partial<SnippetsStateService>;
  let clipboardServiceMock: Partial<ClipboardService>;

  const mockSnippet: SnippetSummary = {
    id: 'snippet-1',
    title: 'Test Snippet',
    language: 'javascript',
    isFavorite: false,
    createdAt: '2024-01-01T00:00:00Z',
    tags: []
  };

  const mockFilters: SnippetFilters = {
    page: 1,
    pageSize: 10,
    sortDescending: true
  };

  beforeEach(async () => {
    stateServiceMock = {
      snippets: signal<SnippetSummary[]>([]),
      selectedSnippet: signal(null),
      tags: signal<Tag[]>([]),
      totalCount: signal(0),
      totalPages: signal(0),
      filters: signal(mockFilters),
      loading: signal(false),
      error: signal<string | null>(null),
      currentPage: signal(1),
      pageSize: signal(10),
      hasNextPage: signal(false),
      hasPreviousPage: signal(false),
      loadSnippets: vi.fn(),
      loadSnippetById: vi.fn(),
      loadTags: vi.fn(),
      createSnippet: vi.fn(),
      updateSnippet: vi.fn(),
      deleteSnippet: vi.fn(),
      toggleFavorite: vi.fn(),
      setFilters: vi.fn(),
      resetFilters: vi.fn(),
      nextPage: vi.fn(),
      previousPage: vi.fn(),
      clearSelectedSnippet: vi.fn(),
      clearError: vi.fn()
    };

    clipboardServiceMock = {
      copy: vi.fn().mockResolvedValue(true)
    };

    await TestBed.configureTestingModule({
      imports: [SnippetListComponent],
      providers: [
        { provide: SnippetsStateService, useValue: stateServiceMock },
        { provide: ClipboardService, useValue: clipboardServiceMock }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(SnippetListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load snippets and tags on init', () => {
    expect(stateServiceMock.loadSnippets).toHaveBeenCalled();
    expect(stateServiceMock.loadTags).toHaveBeenCalled();
  });

  it('should show empty state when no snippets', () => {
    const emptyState = fixture.nativeElement.querySelector('.empty-state');
    expect(emptyState).toBeTruthy();
  });

  it('should show loading state when loading', () => {
    (stateServiceMock.loading as any).set(true);
    fixture.detectChanges();

    const loadingState = fixture.nativeElement.querySelector('.loading-state');
    expect(loadingState).toBeTruthy();
  });

  it('should show error banner when error exists', () => {
    (stateServiceMock.error as any).set('Test error');
    fixture.detectChanges();

    const errorBanner = fixture.nativeElement.querySelector('.error-banner');
    expect(errorBanner).toBeTruthy();
    expect(errorBanner.textContent).toContain('Test error');
  });

  it('should display snippets grid when snippets exist', () => {
    (stateServiceMock.snippets as any).set([mockSnippet]);
    fixture.detectChanges();

    const grid = fixture.nativeElement.querySelector('.snippets-grid');
    expect(grid).toBeTruthy();
  });

  it('should switch to create view when new snippet button clicked', () => {
    const newBtn = fixture.nativeElement.querySelector('.page-header .btn-primary');
    newBtn.click();
    fixture.detectChanges();

    expect(component.viewMode()).toBe('create');
    const formTitle = fixture.nativeElement.querySelector('.form-title');
    expect(formTitle.textContent).toContain('Create New Snippet');
  });

  it('should call toggleFavorite when favorite button is clicked in list', () => {
    (stateServiceMock.snippets as any).set([mockSnippet]);
    fixture.detectChanges();

    component.state.toggleFavorite('snippet-1');
    expect(stateServiceMock.toggleFavorite).toHaveBeenCalledWith('snippet-1');
  });

  it('should show pagination when multiple pages exist', () => {
    (stateServiceMock.snippets as any).set([mockSnippet]);
    (stateServiceMock.totalPages as any).set(3);
    fixture.detectChanges();

    const pagination = fixture.nativeElement.querySelector('.pagination');
    expect(pagination).toBeTruthy();
  });

  it('should call setFilters when filters change', () => {
    component.onFiltersChange({ language: 'typescript' });
    expect(stateServiceMock.setFilters).toHaveBeenCalledWith({ language: 'typescript' });
  });

  it('should clear error when close button clicked', () => {
    (stateServiceMock.error as any).set('Test error');
    fixture.detectChanges();

    const closeBtn = fixture.nativeElement.querySelector('.error-banner .close-btn');
    closeBtn.click();

    expect(stateServiceMock.clearError).toHaveBeenCalled();
  });
});
