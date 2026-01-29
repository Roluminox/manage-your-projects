import { HttpClient } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import {
  AddChecklistItemRequest,
  ArchiveTaskRequest,
  ChecklistItem,
  Column,
  CreateColumnRequest,
  CreateProjectRequest,
  CreateTaskRequest,
  MoveTaskRequest,
  Project,
  ProjectSummary,
  ReorderColumnsRequest,
  ReorderTasksRequest,
  TaskItem,
  UpdateColumnRequest,
  UpdateProjectRequest,
  UpdateTaskRequest
} from '../models/kanban.models';

@Injectable({
  providedIn: 'root'
})
export class KanbanApiService {
  private readonly http = inject(HttpClient);
  private readonly apiUrl = environment.apiUrl;

  // Projects
  getProjects(): Observable<ProjectSummary[]> {
    return this.http.get<ProjectSummary[]>(`${this.apiUrl}/projects`);
  }

  getProjectById(id: string): Observable<Project> {
    return this.http.get<Project>(`${this.apiUrl}/projects/${id}`);
  }

  createProject(request: CreateProjectRequest): Observable<Project> {
    return this.http.post<Project>(`${this.apiUrl}/projects`, request);
  }

  updateProject(id: string, request: UpdateProjectRequest): Observable<Project> {
    return this.http.put<Project>(`${this.apiUrl}/projects/${id}`, request);
  }

  deleteProject(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/projects/${id}`);
  }

  // Columns
  createColumn(projectId: string, request: CreateColumnRequest): Observable<Column> {
    return this.http.post<Column>(`${this.apiUrl}/projects/${projectId}/columns`, request);
  }

  updateColumn(id: string, request: UpdateColumnRequest): Observable<Column> {
    return this.http.put<Column>(`${this.apiUrl}/columns/${id}`, request);
  }

  deleteColumn(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/columns/${id}`);
  }

  reorderColumns(projectId: string, request: ReorderColumnsRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/projects/${projectId}/columns/reorder`, request);
  }

  // Tasks
  getTaskById(id: string): Observable<TaskItem> {
    return this.http.get<TaskItem>(`${this.apiUrl}/tasks/${id}`);
  }

  createTask(columnId: string, request: CreateTaskRequest): Observable<TaskItem> {
    return this.http.post<TaskItem>(`${this.apiUrl}/columns/${columnId}/tasks`, request);
  }

  updateTask(id: string, request: UpdateTaskRequest): Observable<TaskItem> {
    return this.http.put<TaskItem>(`${this.apiUrl}/tasks/${id}`, request);
  }

  deleteTask(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/tasks/${id}`);
  }

  moveTask(id: string, request: MoveTaskRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/tasks/${id}/move`, request);
  }

  reorderTasks(columnId: string, request: ReorderTasksRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/columns/${columnId}/tasks/reorder`, request);
  }

  archiveTask(id: string, request: ArchiveTaskRequest): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/tasks/${id}/archive`, request);
  }

  // Checklists
  addChecklistItem(taskId: string, request: AddChecklistItemRequest): Observable<ChecklistItem> {
    return this.http.post<ChecklistItem>(`${this.apiUrl}/tasks/${taskId}/checklist`, request);
  }

  toggleChecklistItem(id: string): Observable<void> {
    return this.http.put<void>(`${this.apiUrl}/checklist/${id}/toggle`, {});
  }

  deleteChecklistItem(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/checklist/${id}`);
  }
}
