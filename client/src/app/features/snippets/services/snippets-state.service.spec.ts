import { TestBed } from '@angular/core/testing';
import { of, throwError, delay } from 'rxjs';
import { vi } from 'vitest';
import { SnippetsApiService } from './snippets-api.service';
import { SnippetsStateService } from './snippets-state.service';
import {
  CreateSnippetRequest,
  Snippet,
  SnippetListResponse,
  SnippetSummary,
  Tag
} from '../models/snippet.models';

describe('SnippetsStateService', () => {
  let service: SnippetsStateService;
  let apiMock: {
    getSnippets: ReturnType<typeof vi.fn>;
    getSnippetById: ReturnType<typeof vi.fn>;
    searchSnippets: ReturnType<typeof vi.fn>;
    createSnippet: ReturnType<typeof vi.fn>;
    updateSnippet: ReturnType<typeof vi.fn>;
    deleteSnippet: ReturnType<typeof vi.fn>;
    toggleFavorite: ReturnType<typeof vi.fn>;
    getTags: ReturnType<typeof vi.fn>;
    createTag: ReturnType<typeof vi.fn>;
    deleteTag: ReturnType<typeof vi.fn>;
  };

  const mockTag: Tag = { id: 'tag-1', name: 'JavaScript', color: '#f7df1e' };

  const mockSnippetSummary: SnippetSummary = {
    id: 'snippet-1',
    title: 'Test Snippet',
    language: 'javascript',
    isFavorite: false,
    createdAt: '2024-01-01T00:00:00Z',
    tags: [mockTag]
  };

  const mockSnippet: Snippet = {
    ...mockSnippetSummary,
    code: 'console.log("Hello");',
    description: 'A test snippet'
  };

  const mockSnippetList: SnippetListResponse = {
    items: [mockSnippetSummary],
    totalCount: 1,
    page: 1,
    pageSize: 10,
    totalPages: 1
  };

  beforeEach(() => {
    apiMock = {
      getSnippets: vi.fn(),
      getSnippetById: vi.fn(),
      searchSnippets: vi.fn(),
      createSnippet: vi.fn(),
      updateSnippet: vi.fn(),
      deleteSnippet: vi.fn(),
      toggleFavorite: vi.fn(),
      getTags: vi.fn(),
      createTag: vi.fn(),
      deleteTag: vi.fn()
    };

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        SnippetsStateService,
        { provide: SnippetsApiService, useValue: apiMock }
      ]
    });

    service = TestBed.inject(SnippetsStateService);
  });

  describe('initial state', () => {
    it('should have empty snippets initially', () => {
      expect(service.snippets()).toEqual([]);
    });

    it('should have null selected snippet initially', () => {
      expect(service.selectedSnippet()).toBeNull();
    });

    it('should have empty tags initially', () => {
      expect(service.tags()).toEqual([]);
    });

    it('should not be loading initially', () => {
      expect(service.loading()).toBe(false);
    });

    it('should have no error initially', () => {
      expect(service.error()).toBeNull();
    });

    it('should have default filters', () => {
      expect(service.filters().page).toBe(1);
      expect(service.filters().pageSize).toBe(10);
      expect(service.filters().sortDescending).toBe(true);
    });
  });

  describe('loadSnippets', () => {
    it('should load snippets successfully', async () => {
      apiMock.getSnippets.mockReturnValue(of(mockSnippetList));

      service.loadSnippets();
      await vi.waitFor(() => {
        expect(service.snippets()).toEqual([mockSnippetSummary]);
      });

      expect(service.totalCount()).toBe(1);
      expect(service.totalPages()).toBe(1);
    });

    it('should set error on failed load', async () => {
      apiMock.getSnippets.mockReturnValue(
        throwError(() => ({ error: { errors: ['Failed to load'] } }))
      );

      service.loadSnippets();
      await vi.waitFor(() => {
        expect(service.error()).toBe('Failed to load');
      });
    });

    it('should set loading during load', () => {
      apiMock.getSnippets.mockReturnValue(of(mockSnippetList).pipe(delay(100)));

      service.loadSnippets();
      expect(service.loading()).toBe(true);
    });

    it('should use search API when searchTerm is set', async () => {
      apiMock.searchSnippets.mockReturnValue(of(mockSnippetList));

      service.setFilters({ searchTerm: 'test' });
      await vi.waitFor(() => {
        expect(apiMock.searchSnippets).toHaveBeenCalledWith('test', 1, 10);
      });
    });
  });

  describe('loadSnippetById', () => {
    it('should load snippet by id', async () => {
      apiMock.getSnippetById.mockReturnValue(of(mockSnippet));

      service.loadSnippetById('snippet-1');
      await vi.waitFor(() => {
        expect(service.selectedSnippet()).toEqual(mockSnippet);
      });
    });

    it('should set error on failed load', async () => {
      apiMock.getSnippetById.mockReturnValue(
        throwError(() => ({ error: { errors: ['Not found'] } }))
      );

      service.loadSnippetById('invalid-id');
      await vi.waitFor(() => {
        expect(service.error()).toBe('Not found');
      });
    });
  });

  describe('loadTags', () => {
    it('should load tags', async () => {
      apiMock.getTags.mockReturnValue(of([mockTag]));

      service.loadTags();
      await vi.waitFor(() => {
        expect(service.tags()).toEqual([mockTag]);
      });
    });
  });

  describe('createSnippet', () => {
    it('should create snippet and reload list', async () => {
      const request: CreateSnippetRequest = {
        title: 'New Snippet',
        code: 'const x = 1;',
        language: 'typescript'
      };

      apiMock.createSnippet.mockReturnValue(of(mockSnippet));
      apiMock.getSnippets.mockReturnValue(of(mockSnippetList));

      service.createSnippet(request);
      await vi.waitFor(() => {
        expect(apiMock.createSnippet).toHaveBeenCalledWith(request);
      });
      await vi.waitFor(() => {
        expect(apiMock.getSnippets).toHaveBeenCalled();
      });
    });

    it('should set error on failed create', async () => {
      apiMock.createSnippet.mockReturnValue(
        throwError(() => ({ error: { errors: ['Validation failed'] } }))
      );

      service.createSnippet({ title: '', code: '', language: '' });
      await vi.waitFor(() => {
        expect(service.error()).toBe('Validation failed');
      });
    });
  });

  describe('toggleFavorite', () => {
    it('should toggle favorite and update snippet in list', async () => {
      apiMock.getSnippets.mockReturnValue(of(mockSnippetList));
      apiMock.toggleFavorite.mockReturnValue(of({ isFavorite: true }));

      service.loadSnippets();
      await vi.waitFor(() => {
        expect(service.snippets().length).toBe(1);
      });

      service.toggleFavorite('snippet-1');
      await vi.waitFor(() => {
        expect(service.snippets()[0].isFavorite).toBe(true);
      });
    });
  });

  describe('setFilters', () => {
    it('should update filters and reload', async () => {
      apiMock.getSnippets.mockReturnValue(of(mockSnippetList));

      service.setFilters({ language: 'typescript' });
      await vi.waitFor(() => {
        expect(service.filters().language).toBe('typescript');
      });

      expect(apiMock.getSnippets).toHaveBeenCalled();
    });

    it('should reset page to 1 when filter changes', async () => {
      apiMock.getSnippets.mockReturnValue(of(mockSnippetList));

      service.setFilters({ page: 3 });
      await vi.waitFor(() => {
        expect(service.filters().page).toBe(3);
      });

      service.setFilters({ language: 'javascript' });
      await vi.waitFor(() => {
        expect(service.filters().page).toBe(1);
      });
    });
  });

  describe('pagination', () => {
    it('should navigate to next page', async () => {
      const multiPageResponse: SnippetListResponse = {
        ...mockSnippetList,
        totalPages: 3
      };
      apiMock.getSnippets.mockReturnValue(of(multiPageResponse));

      service.loadSnippets();
      await vi.waitFor(() => {
        expect(service.hasNextPage()).toBe(true);
      });

      service.nextPage();
      await vi.waitFor(() => {
        expect(service.currentPage()).toBe(2);
      });
    });

    it('should navigate to previous page', async () => {
      apiMock.getSnippets.mockReturnValue(of(mockSnippetList));

      service.setFilters({ page: 2 });
      await vi.waitFor(() => {
        expect(service.hasPreviousPage()).toBe(true);
      });

      service.previousPage();
      await vi.waitFor(() => {
        expect(service.currentPage()).toBe(1);
      });
    });
  });

  describe('resetFilters', () => {
    it('should reset all filters to defaults', async () => {
      apiMock.getSnippets.mockReturnValue(of(mockSnippetList));

      service.setFilters({ language: 'typescript', page: 3 });
      await vi.waitFor(() => {
        expect(service.filters().language).toBe('typescript');
      });

      service.resetFilters();
      await vi.waitFor(() => {
        expect(service.filters().language).toBeUndefined();
        expect(service.filters().page).toBe(1);
      });
    });
  });

  describe('clearError', () => {
    it('should clear error', async () => {
      apiMock.getSnippets.mockReturnValue(
        throwError(() => ({ error: { errors: ['Error'] } }))
      );

      service.loadSnippets();
      await vi.waitFor(() => {
        expect(service.error()).toBe('Error');
      });

      service.clearError();
      expect(service.error()).toBeNull();
    });
  });
});
