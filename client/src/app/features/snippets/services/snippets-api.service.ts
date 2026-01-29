import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  CreateSnippetRequest,
  CreateTagRequest,
  Snippet,
  SnippetFilters,
  SnippetListResponse,
  Tag,
  UpdateSnippetRequest
} from '../models/snippet.models';

@Injectable({
  providedIn: 'root'
})
export class SnippetsApiService {
  private readonly http = inject(HttpClient);
  private readonly snippetsUrl = `${environment.apiUrl}/snippets`;
  private readonly tagsUrl = `${environment.apiUrl}/tags`;

  getSnippets(filters: Partial<SnippetFilters> = {}): Observable<SnippetListResponse> {
    let params = new HttpParams();

    if (filters.page) params = params.set('page', filters.page.toString());
    if (filters.pageSize) params = params.set('pageSize', filters.pageSize.toString());
    if (filters.language) params = params.set('language', filters.language);
    if (filters.tagId) params = params.set('tagId', filters.tagId);
    if (filters.isFavorite !== undefined) params = params.set('isFavorite', filters.isFavorite.toString());
    if (filters.sortBy) params = params.set('sortBy', filters.sortBy);
    if (filters.sortDescending !== undefined) params = params.set('sortDescending', filters.sortDescending.toString());

    return this.http.get<SnippetListResponse>(this.snippetsUrl, { params });
  }

  getSnippetById(id: string): Observable<Snippet> {
    return this.http.get<Snippet>(`${this.snippetsUrl}/${id}`);
  }

  searchSnippets(searchTerm: string, page = 1, pageSize = 10): Observable<SnippetListResponse> {
    const params = new HttpParams()
      .set('q', searchTerm)
      .set('page', page.toString())
      .set('pageSize', pageSize.toString());

    return this.http.get<SnippetListResponse>(`${this.snippetsUrl}/search`, { params });
  }

  createSnippet(request: CreateSnippetRequest): Observable<Snippet> {
    return this.http.post<Snippet>(this.snippetsUrl, request);
  }

  updateSnippet(id: string, request: UpdateSnippetRequest): Observable<Snippet> {
    return this.http.put<Snippet>(`${this.snippetsUrl}/${id}`, request);
  }

  deleteSnippet(id: string): Observable<void> {
    return this.http.delete<void>(`${this.snippetsUrl}/${id}`);
  }

  toggleFavorite(id: string): Observable<{ isFavorite: boolean }> {
    return this.http.patch<{ isFavorite: boolean }>(`${this.snippetsUrl}/${id}/favorite`, {});
  }

  getTags(): Observable<Tag[]> {
    return this.http.get<Tag[]>(this.tagsUrl);
  }

  createTag(request: CreateTagRequest): Observable<Tag> {
    return this.http.post<Tag>(this.tagsUrl, request);
  }

  deleteTag(id: string): Observable<void> {
    return this.http.delete<void>(`${this.tagsUrl}/${id}`);
  }
}
