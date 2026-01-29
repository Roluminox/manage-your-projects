import { TestBed } from '@angular/core/testing';
import { of, throwError, delay } from 'rxjs';
import { vi } from 'vitest';
import { KanbanApiService } from './kanban-api.service';
import { KanbanStateService } from './kanban-state.service';
import {
  ChecklistItem,
  Column,
  CreateProjectRequest,
  Priority,
  Project,
  ProjectSummary,
  TaskItem
} from '../models/kanban.models';

describe('KanbanStateService', () => {
  let service: KanbanStateService;
  let apiMock: {
    getProjects: ReturnType<typeof vi.fn>;
    getProjectById: ReturnType<typeof vi.fn>;
    createProject: ReturnType<typeof vi.fn>;
    updateProject: ReturnType<typeof vi.fn>;
    deleteProject: ReturnType<typeof vi.fn>;
    createColumn: ReturnType<typeof vi.fn>;
    updateColumn: ReturnType<typeof vi.fn>;
    deleteColumn: ReturnType<typeof vi.fn>;
    reorderColumns: ReturnType<typeof vi.fn>;
    getTaskById: ReturnType<typeof vi.fn>;
    createTask: ReturnType<typeof vi.fn>;
    updateTask: ReturnType<typeof vi.fn>;
    deleteTask: ReturnType<typeof vi.fn>;
    moveTask: ReturnType<typeof vi.fn>;
    reorderTasks: ReturnType<typeof vi.fn>;
    archiveTask: ReturnType<typeof vi.fn>;
    addChecklistItem: ReturnType<typeof vi.fn>;
    toggleChecklistItem: ReturnType<typeof vi.fn>;
    deleteChecklistItem: ReturnType<typeof vi.fn>;
  };

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
    labels: [],
    checklists: [mockChecklistItem]
  };

  const mockTask2: TaskItem = {
    ...mockTask,
    id: 'task-2',
    title: 'Test Task 2',
    order: 1
  };

  const mockColumn: Column = {
    id: 'column-1',
    name: 'To Do',
    order: 0,
    tasks: [mockTask, mockTask2]
  };

  const mockColumn2: Column = {
    id: 'column-2',
    name: 'In Progress',
    order: 1,
    tasks: []
  };

  const mockProject: Project = {
    id: 'project-1',
    name: 'Test Project',
    description: 'A test project',
    color: '#6366f1',
    createdAt: '2024-01-01T00:00:00Z',
    columns: [mockColumn, mockColumn2]
  };

  const mockProjectSummary: ProjectSummary = {
    id: 'project-1',
    name: 'Test Project',
    description: 'A test project',
    color: '#6366f1',
    createdAt: '2024-01-01T00:00:00Z',
    columnCount: 2,
    taskCount: 2
  };

  beforeEach(() => {
    apiMock = {
      getProjects: vi.fn(),
      getProjectById: vi.fn(),
      createProject: vi.fn(),
      updateProject: vi.fn(),
      deleteProject: vi.fn(),
      createColumn: vi.fn(),
      updateColumn: vi.fn(),
      deleteColumn: vi.fn(),
      reorderColumns: vi.fn(),
      getTaskById: vi.fn(),
      createTask: vi.fn(),
      updateTask: vi.fn(),
      deleteTask: vi.fn(),
      moveTask: vi.fn(),
      reorderTasks: vi.fn(),
      archiveTask: vi.fn(),
      addChecklistItem: vi.fn(),
      toggleChecklistItem: vi.fn(),
      deleteChecklistItem: vi.fn()
    };

    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      providers: [
        KanbanStateService,
        { provide: KanbanApiService, useValue: apiMock }
      ]
    });

    service = TestBed.inject(KanbanStateService);
  });

  describe('initial state', () => {
    it('should have empty projects initially', () => {
      expect(service.projects()).toEqual([]);
    });

    it('should have null selected project initially', () => {
      expect(service.selectedProject()).toBeNull();
    });

    it('should have null selected task initially', () => {
      expect(service.selectedTask()).toBeNull();
    });

    it('should not be loading initially', () => {
      expect(service.loading()).toBe(false);
    });

    it('should have no error initially', () => {
      expect(service.error()).toBeNull();
    });

    it('should have empty columns initially', () => {
      expect(service.columns()).toEqual([]);
    });
  });

  describe('loadProjects', () => {
    it('should load projects successfully', async () => {
      apiMock.getProjects.mockReturnValue(of([mockProjectSummary]));

      service.loadProjects();
      await vi.waitFor(() => {
        expect(service.projects()).toEqual([mockProjectSummary]);
      });
    });

    it('should set error on failed load', async () => {
      apiMock.getProjects.mockReturnValue(
        throwError(() => ({ error: { errors: ['Failed to load'] } }))
      );

      service.loadProjects();
      await vi.waitFor(() => {
        expect(service.error()).toBe('Failed to load');
      });
    });

    it('should set loading during load', () => {
      apiMock.getProjects.mockReturnValue(of([mockProjectSummary]).pipe(delay(100)));

      service.loadProjects();
      expect(service.loading()).toBe(true);
    });
  });

  describe('loadProjectById', () => {
    it('should load project by id', async () => {
      apiMock.getProjectById.mockReturnValue(of(mockProject));

      service.loadProjectById('project-1');
      await vi.waitFor(() => {
        expect(service.selectedProject()).toEqual(mockProject);
        expect(service.columns()).toEqual(mockProject.columns);
      });
    });

    it('should set error on failed load', async () => {
      apiMock.getProjectById.mockReturnValue(
        throwError(() => ({ error: { errors: ['Not found'] } }))
      );

      service.loadProjectById('invalid-id');
      await vi.waitFor(() => {
        expect(service.error()).toBe('Not found');
      });
    });
  });

  describe('createProject', () => {
    it('should create project and reload list', async () => {
      const request: CreateProjectRequest = {
        name: 'New Project',
        description: 'A new project'
      };

      apiMock.createProject.mockReturnValue(of(mockProject));
      apiMock.getProjects.mockReturnValue(of([mockProjectSummary]));

      service.createProject(request);
      await vi.waitFor(() => {
        expect(apiMock.createProject).toHaveBeenCalledWith(request);
      });
      await vi.waitFor(() => {
        expect(apiMock.getProjects).toHaveBeenCalled();
      });
    });

    it('should set error on failed create', async () => {
      apiMock.createProject.mockReturnValue(
        throwError(() => ({ error: { errors: ['Validation failed'] } }))
      );

      service.createProject({ name: '' });
      await vi.waitFor(() => {
        expect(service.error()).toBe('Validation failed');
      });
    });
  });

  describe('deleteProject', () => {
    it('should delete project and reload list', async () => {
      apiMock.deleteProject.mockReturnValue(of(void 0));
      apiMock.getProjects.mockReturnValue(of([]));

      service.deleteProject('project-1');
      await vi.waitFor(() => {
        expect(apiMock.deleteProject).toHaveBeenCalledWith('project-1');
      });
      await vi.waitFor(() => {
        expect(apiMock.getProjects).toHaveBeenCalled();
      });
    });
  });

  describe('reorderColumns (optimistic update)', () => {
    it('should optimistically reorder columns', async () => {
      apiMock.getProjectById.mockReturnValue(of(mockProject));
      apiMock.reorderColumns.mockReturnValue(of(void 0));

      service.loadProjectById('project-1');
      await vi.waitFor(() => {
        expect(service.selectedProject()).not.toBeNull();
      });

      service.reorderColumns('project-1', ['column-2', 'column-1']);

      // Check optimistic update
      const columns = service.columns();
      expect(columns[0].id).toBe('column-2');
      expect(columns[1].id).toBe('column-1');
    });

    it('should rollback on error', async () => {
      apiMock.getProjectById.mockReturnValue(of(mockProject));
      apiMock.reorderColumns.mockReturnValue(
        throwError(() => ({ error: { errors: ['Failed'] } }))
      );

      service.loadProjectById('project-1');
      await vi.waitFor(() => {
        expect(service.selectedProject()).not.toBeNull();
      });

      service.reorderColumns('project-1', ['column-2', 'column-1']);

      await vi.waitFor(() => {
        expect(service.error()).toBe('Failed');
      });

      // Should rollback to original order
      const columns = service.columns();
      expect(columns[0].id).toBe('column-1');
      expect(columns[1].id).toBe('column-2');
    });
  });

  describe('loadTaskById', () => {
    it('should load task by id', async () => {
      apiMock.getTaskById.mockReturnValue(of(mockTask));

      service.loadTaskById('task-1');
      await vi.waitFor(() => {
        expect(service.selectedTask()).toEqual(mockTask);
      });
    });
  });

  describe('moveTask (optimistic update)', () => {
    it('should optimistically move task to different column', async () => {
      apiMock.getProjectById.mockReturnValue(of(mockProject));
      apiMock.moveTask.mockReturnValue(of(void 0));

      service.loadProjectById('project-1');
      await vi.waitFor(() => {
        expect(service.selectedProject()).not.toBeNull();
      });

      service.moveTask('task-1', 'column-2', 0);

      // Check optimistic update
      const columns = service.columns();
      expect(columns[0].tasks.find(t => t.id === 'task-1')).toBeUndefined();
      expect(columns[1].tasks.find(t => t.id === 'task-1')).toBeDefined();
    });

    it('should rollback move on error', async () => {
      apiMock.getProjectById.mockReturnValue(of(mockProject));
      apiMock.moveTask.mockReturnValue(
        throwError(() => ({ error: { errors: ['Failed to move'] } }))
      );

      service.loadProjectById('project-1');
      await vi.waitFor(() => {
        expect(service.selectedProject()).not.toBeNull();
      });

      service.moveTask('task-1', 'column-2', 0);

      await vi.waitFor(() => {
        expect(service.error()).toBe('Failed to move');
      });

      // Should rollback to original position
      const columns = service.columns();
      expect(columns[0].tasks.find(t => t.id === 'task-1')).toBeDefined();
    });
  });

  describe('reorderTasks (optimistic update)', () => {
    it('should optimistically reorder tasks within column', async () => {
      apiMock.getProjectById.mockReturnValue(of(mockProject));
      apiMock.reorderTasks.mockReturnValue(of(void 0));

      service.loadProjectById('project-1');
      await vi.waitFor(() => {
        expect(service.selectedProject()).not.toBeNull();
      });

      service.reorderTasks('column-1', ['task-2', 'task-1']);

      // Check optimistic update
      const column = service.columns().find(c => c.id === 'column-1');
      expect(column?.tasks[0].id).toBe('task-2');
      expect(column?.tasks[1].id).toBe('task-1');
    });
  });

  describe('toggleChecklistItem (optimistic update)', () => {
    it('should optimistically toggle checklist item', async () => {
      apiMock.getTaskById.mockReturnValue(of(mockTask));
      apiMock.toggleChecklistItem.mockReturnValue(of(void 0));

      service.loadTaskById('task-1');
      await vi.waitFor(() => {
        expect(service.selectedTask()).not.toBeNull();
      });

      expect(service.selectedTask()?.checklists[0].isCompleted).toBe(false);

      service.toggleChecklistItem('checklist-1');

      // Check optimistic update
      expect(service.selectedTask()?.checklists[0].isCompleted).toBe(true);
    });

    it('should rollback toggle on error', async () => {
      apiMock.getTaskById.mockReturnValue(of(mockTask));
      apiMock.toggleChecklistItem.mockReturnValue(
        throwError(() => ({ error: { errors: ['Failed'] } }))
      );

      service.loadTaskById('task-1');
      await vi.waitFor(() => {
        expect(service.selectedTask()).not.toBeNull();
      });

      service.toggleChecklistItem('checklist-1');

      await vi.waitFor(() => {
        expect(service.error()).toBe('Failed');
      });

      // Should rollback
      expect(service.selectedTask()?.checklists[0].isCompleted).toBe(false);
    });
  });

  describe('deleteChecklistItem (optimistic update)', () => {
    it('should optimistically delete checklist item', async () => {
      apiMock.getTaskById.mockReturnValue(of(mockTask));
      apiMock.deleteChecklistItem.mockReturnValue(of(void 0));

      service.loadTaskById('task-1');
      await vi.waitFor(() => {
        expect(service.selectedTask()).not.toBeNull();
      });

      expect(service.selectedTask()?.checklists.length).toBe(1);

      service.deleteChecklistItem('checklist-1');

      // Check optimistic update
      expect(service.selectedTask()?.checklists.length).toBe(0);
    });
  });

  describe('utility methods', () => {
    it('should clear selected project', async () => {
      apiMock.getProjectById.mockReturnValue(of(mockProject));

      service.loadProjectById('project-1');
      await vi.waitFor(() => {
        expect(service.selectedProject()).not.toBeNull();
      });

      service.clearSelectedProject();
      expect(service.selectedProject()).toBeNull();
    });

    it('should clear selected task', async () => {
      apiMock.getTaskById.mockReturnValue(of(mockTask));

      service.loadTaskById('task-1');
      await vi.waitFor(() => {
        expect(service.selectedTask()).not.toBeNull();
      });

      service.clearSelectedTask();
      expect(service.selectedTask()).toBeNull();
    });

    it('should clear error', async () => {
      apiMock.getProjects.mockReturnValue(
        throwError(() => ({ error: { errors: ['Error'] } }))
      );

      service.loadProjects();
      await vi.waitFor(() => {
        expect(service.error()).toBe('Error');
      });

      service.clearError();
      expect(service.error()).toBeNull();
    });
  });
});
