import { TestBed } from '@angular/core/testing';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideHttpClient } from '@angular/common/http';
import { KanbanApiService } from './kanban-api.service';
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
  Priority,
  Project,
  ProjectSummary,
  ReorderColumnsRequest,
  ReorderTasksRequest,
  TaskItem,
  UpdateColumnRequest,
  UpdateProjectRequest,
  UpdateTaskRequest
} from '../models/kanban.models';

describe('KanbanApiService', () => {
  let service: KanbanApiService;
  let httpMock: HttpTestingController;
  const baseUrl = environment.apiUrl;

  const mockLabel = { id: 'label-1', name: 'Bug', color: '#ef4444' };

  const mockChecklistItem: ChecklistItem = {
    id: 'checklist-1',
    text: 'Review code',
    isCompleted: false,
    order: 0
  };

  const mockTask: TaskItem = {
    id: 'task-1',
    title: 'Test Task',
    description: 'A test task',
    priority: Priority.Medium,
    dueDate: '2024-12-31T00:00:00Z',
    order: 0,
    isArchived: false,
    createdAt: '2024-01-01T00:00:00Z',
    labels: [mockLabel],
    checklists: [mockChecklistItem]
  };

  const mockColumn: Column = {
    id: 'column-1',
    name: 'To Do',
    order: 0,
    tasks: [mockTask]
  };

  const mockProject: Project = {
    id: 'project-1',
    name: 'Test Project',
    description: 'A test project',
    color: '#6366f1',
    createdAt: '2024-01-01T00:00:00Z',
    columns: [mockColumn]
  };

  const mockProjectSummary: ProjectSummary = {
    id: 'project-1',
    name: 'Test Project',
    description: 'A test project',
    color: '#6366f1',
    createdAt: '2024-01-01T00:00:00Z',
    columnCount: 3,
    taskCount: 10
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [
        KanbanApiService,
        provideHttpClient(),
        provideHttpClientTesting()
      ]
    });

    service = TestBed.inject(KanbanApiService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  describe('Projects', () => {
    describe('getProjects', () => {
      it('should get all projects', () => {
        const projects: ProjectSummary[] = [mockProjectSummary];

        service.getProjects().subscribe(response => {
          expect(response).toEqual(projects);
        });

        const req = httpMock.expectOne(`${baseUrl}/projects`);
        expect(req.request.method).toBe('GET');
        req.flush(projects);
      });
    });

    describe('getProjectById', () => {
      it('should get project by id', () => {
        service.getProjectById('project-1').subscribe(response => {
          expect(response).toEqual(mockProject);
        });

        const req = httpMock.expectOne(`${baseUrl}/projects/project-1`);
        expect(req.request.method).toBe('GET');
        req.flush(mockProject);
      });
    });

    describe('createProject', () => {
      it('should create project', () => {
        const request: CreateProjectRequest = {
          name: 'New Project',
          description: 'A new project',
          color: '#8b5cf6'
        };

        service.createProject(request).subscribe(response => {
          expect(response).toEqual(mockProject);
        });

        const req = httpMock.expectOne(`${baseUrl}/projects`);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual(request);
        req.flush(mockProject);
      });
    });

    describe('updateProject', () => {
      it('should update project', () => {
        const request: UpdateProjectRequest = {
          name: 'Updated Project',
          description: 'Updated description',
          color: '#ec4899'
        };

        service.updateProject('project-1', request).subscribe(response => {
          expect(response).toEqual(mockProject);
        });

        const req = httpMock.expectOne(`${baseUrl}/projects/project-1`);
        expect(req.request.method).toBe('PUT');
        expect(req.request.body).toEqual(request);
        req.flush(mockProject);
      });
    });

    describe('deleteProject', () => {
      it('should delete project', () => {
        service.deleteProject('project-1').subscribe();

        const req = httpMock.expectOne(`${baseUrl}/projects/project-1`);
        expect(req.request.method).toBe('DELETE');
        req.flush(null);
      });
    });
  });

  describe('Columns', () => {
    describe('createColumn', () => {
      it('should create column', () => {
        const request: CreateColumnRequest = { name: 'In Progress' };

        service.createColumn('project-1', request).subscribe(response => {
          expect(response).toEqual(mockColumn);
        });

        const req = httpMock.expectOne(`${baseUrl}/projects/project-1/columns`);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual(request);
        req.flush(mockColumn);
      });
    });

    describe('updateColumn', () => {
      it('should update column', () => {
        const request: UpdateColumnRequest = { name: 'Done' };

        service.updateColumn('column-1', request).subscribe(response => {
          expect(response).toEqual(mockColumn);
        });

        const req = httpMock.expectOne(`${baseUrl}/columns/column-1`);
        expect(req.request.method).toBe('PUT');
        expect(req.request.body).toEqual(request);
        req.flush(mockColumn);
      });
    });

    describe('deleteColumn', () => {
      it('should delete column', () => {
        service.deleteColumn('column-1').subscribe();

        const req = httpMock.expectOne(`${baseUrl}/columns/column-1`);
        expect(req.request.method).toBe('DELETE');
        req.flush(null);
      });
    });

    describe('reorderColumns', () => {
      it('should reorder columns', () => {
        const request: ReorderColumnsRequest = {
          columnIds: ['column-2', 'column-1', 'column-3']
        };

        service.reorderColumns('project-1', request).subscribe();

        const req = httpMock.expectOne(`${baseUrl}/projects/project-1/columns/reorder`);
        expect(req.request.method).toBe('PUT');
        expect(req.request.body).toEqual(request);
        req.flush(null);
      });
    });
  });

  describe('Tasks', () => {
    describe('getTaskById', () => {
      it('should get task by id', () => {
        service.getTaskById('task-1').subscribe(response => {
          expect(response).toEqual(mockTask);
        });

        const req = httpMock.expectOne(`${baseUrl}/tasks/task-1`);
        expect(req.request.method).toBe('GET');
        req.flush(mockTask);
      });
    });

    describe('createTask', () => {
      it('should create task', () => {
        const request: CreateTaskRequest = {
          title: 'New Task',
          description: 'A new task',
          priority: Priority.High,
          dueDate: '2024-12-31T00:00:00Z',
          labelIds: ['label-1']
        };

        service.createTask('column-1', request).subscribe(response => {
          expect(response).toEqual(mockTask);
        });

        const req = httpMock.expectOne(`${baseUrl}/columns/column-1/tasks`);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual(request);
        req.flush(mockTask);
      });
    });

    describe('updateTask', () => {
      it('should update task', () => {
        const request: UpdateTaskRequest = {
          title: 'Updated Task',
          description: 'Updated description',
          priority: Priority.Critical,
          labelIds: ['label-2']
        };

        service.updateTask('task-1', request).subscribe(response => {
          expect(response).toEqual(mockTask);
        });

        const req = httpMock.expectOne(`${baseUrl}/tasks/task-1`);
        expect(req.request.method).toBe('PUT');
        expect(req.request.body).toEqual(request);
        req.flush(mockTask);
      });
    });

    describe('deleteTask', () => {
      it('should delete task', () => {
        service.deleteTask('task-1').subscribe();

        const req = httpMock.expectOne(`${baseUrl}/tasks/task-1`);
        expect(req.request.method).toBe('DELETE');
        req.flush(null);
      });
    });

    describe('moveTask', () => {
      it('should move task', () => {
        const request: MoveTaskRequest = {
          targetColumnId: 'column-2',
          newOrder: 1
        };

        service.moveTask('task-1', request).subscribe();

        const req = httpMock.expectOne(`${baseUrl}/tasks/task-1/move`);
        expect(req.request.method).toBe('PUT');
        expect(req.request.body).toEqual(request);
        req.flush(null);
      });
    });

    describe('reorderTasks', () => {
      it('should reorder tasks', () => {
        const request: ReorderTasksRequest = {
          taskIds: ['task-2', 'task-1', 'task-3']
        };

        service.reorderTasks('column-1', request).subscribe();

        const req = httpMock.expectOne(`${baseUrl}/columns/column-1/tasks/reorder`);
        expect(req.request.method).toBe('PUT');
        expect(req.request.body).toEqual(request);
        req.flush(null);
      });
    });

    describe('archiveTask', () => {
      it('should archive task', () => {
        const request: ArchiveTaskRequest = { archive: true };

        service.archiveTask('task-1', request).subscribe();

        const req = httpMock.expectOne(`${baseUrl}/tasks/task-1/archive`);
        expect(req.request.method).toBe('PUT');
        expect(req.request.body).toEqual(request);
        req.flush(null);
      });

      it('should unarchive task', () => {
        const request: ArchiveTaskRequest = { archive: false };

        service.archiveTask('task-1', request).subscribe();

        const req = httpMock.expectOne(`${baseUrl}/tasks/task-1/archive`);
        expect(req.request.method).toBe('PUT');
        expect(req.request.body).toEqual(request);
        req.flush(null);
      });
    });
  });

  describe('Checklists', () => {
    describe('addChecklistItem', () => {
      it('should add checklist item', () => {
        const request: AddChecklistItemRequest = { text: 'New checklist item' };

        service.addChecklistItem('task-1', request).subscribe(response => {
          expect(response).toEqual(mockChecklistItem);
        });

        const req = httpMock.expectOne(`${baseUrl}/tasks/task-1/checklist`);
        expect(req.request.method).toBe('POST');
        expect(req.request.body).toEqual(request);
        req.flush(mockChecklistItem);
      });
    });

    describe('toggleChecklistItem', () => {
      it('should toggle checklist item', () => {
        service.toggleChecklistItem('checklist-1').subscribe();

        const req = httpMock.expectOne(`${baseUrl}/checklist/checklist-1/toggle`);
        expect(req.request.method).toBe('PUT');
        expect(req.request.body).toEqual({});
        req.flush(null);
      });
    });

    describe('deleteChecklistItem', () => {
      it('should delete checklist item', () => {
        service.deleteChecklistItem('checklist-1').subscribe();

        const req = httpMock.expectOne(`${baseUrl}/checklist/checklist-1`);
        expect(req.request.method).toBe('DELETE');
        req.flush(null);
      });
    });
  });
});
