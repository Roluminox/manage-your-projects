import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { SnippetsApiService } from './snippets-api.service';
import { environment } from '../../../../environments/environment';
import {
  CreateSnippetRequest,
  CreateTagRequest,
  Snippet,
  SnippetListResponse,
  Tag,
  UpdateSnippetRequest
} from '../models/snippet.models';

describe('SnippetsApiService', () => {
  let service: SnippetsApiService;
  let httpMock: HttpTestingController;
  const baseUrl = environment.apiUrl;

  const mockTag: Tag = { id: 'tag-1', name: 'JavaScript', color: '#f7df1e' };

  const mockSnippet: Snippet = {
    id: 'snippet-1',
    title: 'Test Snippet',
    code: 'console.log("Hello");',
    language: 'javascript',
    description: 'A test snippet',
    isFavorite: false,
    createdAt: '2024-01-01T00:00:00Z',
    tags: [mockTag]
  };

  const mockSnippetList: SnippetListResponse = {
    items: [
      {
        id: 'snippet-1',
        title: 'Test Snippet',
        language: 'javascript',
        isFavorite: false,
        createdAt: '2024-01-01T00:00:00Z',
        tags: [mockTag]
      }
    ],
    totalCount: 1,
    page: 1,
    pageSize: 10,
    totalPages: 1
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        SnippetsApiService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(SnippetsApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('getSnippets', () => {
    it('should get snippets without filters', () => {
      service.getSnippets().subscribe(response => {
        expect(response).toEqual(mockSnippetList);
      });

      const req = httpMock.expectOne(`${baseUrl}/snippets`);
      expect(req.request.method).toBe('GET');
      req.flush(mockSnippetList);
    });

    it('should get snippets with filters', () => {
      service.getSnippets({
        page: 2,
        pageSize: 20,
        language: 'typescript',
        isFavorite: true
      }).subscribe();

      const req = httpMock.expectOne(r =>
        r.url === `${baseUrl}/snippets` &&
        r.params.get('page') === '2' &&
        r.params.get('pageSize') === '20' &&
        r.params.get('language') === 'typescript' &&
        r.params.get('isFavorite') === 'true'
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockSnippetList);
    });
  });

  describe('getSnippetById', () => {
    it('should get snippet by id', () => {
      service.getSnippetById('snippet-1').subscribe(response => {
        expect(response).toEqual(mockSnippet);
      });

      const req = httpMock.expectOne(`${baseUrl}/snippets/snippet-1`);
      expect(req.request.method).toBe('GET');
      req.flush(mockSnippet);
    });
  });

  describe('searchSnippets', () => {
    it('should search snippets', () => {
      service.searchSnippets('console', 1, 10).subscribe(response => {
        expect(response).toEqual(mockSnippetList);
      });

      const req = httpMock.expectOne(r =>
        r.url === `${baseUrl}/snippets/search` &&
        r.params.get('q') === 'console' &&
        r.params.get('page') === '1' &&
        r.params.get('pageSize') === '10'
      );
      expect(req.request.method).toBe('GET');
      req.flush(mockSnippetList);
    });
  });

  describe('createSnippet', () => {
    it('should create snippet', () => {
      const request: CreateSnippetRequest = {
        title: 'New Snippet',
        code: 'const x = 1;',
        language: 'typescript'
      };

      service.createSnippet(request).subscribe(response => {
        expect(response).toEqual(mockSnippet);
      });

      const req = httpMock.expectOne(`${baseUrl}/snippets`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(request);
      req.flush(mockSnippet);
    });
  });

  describe('updateSnippet', () => {
    it('should update snippet', () => {
      const request: UpdateSnippetRequest = {
        title: 'Updated Snippet',
        code: 'const x = 2;',
        language: 'typescript'
      };

      service.updateSnippet('snippet-1', request).subscribe(response => {
        expect(response).toEqual(mockSnippet);
      });

      const req = httpMock.expectOne(`${baseUrl}/snippets/snippet-1`);
      expect(req.request.method).toBe('PUT');
      expect(req.request.body).toEqual(request);
      req.flush(mockSnippet);
    });
  });

  describe('deleteSnippet', () => {
    it('should delete snippet', () => {
      service.deleteSnippet('snippet-1').subscribe();

      const req = httpMock.expectOne(`${baseUrl}/snippets/snippet-1`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });

  describe('toggleFavorite', () => {
    it('should toggle favorite', () => {
      const response = { isFavorite: true };

      service.toggleFavorite('snippet-1').subscribe(res => {
        expect(res).toEqual(response);
      });

      const req = httpMock.expectOne(`${baseUrl}/snippets/snippet-1/favorite`);
      expect(req.request.method).toBe('PATCH');
      req.flush(response);
    });
  });

  describe('getTags', () => {
    it('should get tags', () => {
      const tags: Tag[] = [mockTag];

      service.getTags().subscribe(response => {
        expect(response).toEqual(tags);
      });

      const req = httpMock.expectOne(`${baseUrl}/tags`);
      expect(req.request.method).toBe('GET');
      req.flush(tags);
    });
  });

  describe('createTag', () => {
    it('should create tag', () => {
      const request: CreateTagRequest = { name: 'New Tag', color: '#ff0000' };

      service.createTag(request).subscribe(response => {
        expect(response).toEqual(mockTag);
      });

      const req = httpMock.expectOne(`${baseUrl}/tags`);
      expect(req.request.method).toBe('POST');
      expect(req.request.body).toEqual(request);
      req.flush(mockTag);
    });
  });

  describe('deleteTag', () => {
    it('should delete tag', () => {
      service.deleteTag('tag-1').subscribe();

      const req = httpMock.expectOne(`${baseUrl}/tags/tag-1`);
      expect(req.request.method).toBe('DELETE');
      req.flush(null);
    });
  });
});
