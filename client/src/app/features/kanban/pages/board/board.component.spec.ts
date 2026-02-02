import { ComponentFixture, TestBed } from '@angular/core/testing';
import { signal } from '@angular/core';
import { ActivatedRoute, convertToParamMap, provideRouter } from '@angular/router';
import { vi } from 'vitest';
import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { BoardComponent } from './board.component';
import { KanbanStateService } from '../../services/kanban-state.service';
import { Column, Priority, Project, ProjectSummary, TaskItem } from '../../models/kanban.models';

describe('BoardComponent', () => {
  let component: BoardComponent;
  let fixture: ComponentFixture<BoardComponent>;
  let stateServiceMock: Partial<KanbanStateService>;

  const mockTask: TaskItem = {
    id: 'task-1',
    title: 'Test Task',
    description: 'A test task',
    priority: Priority.Medium,
    order: 0,
    isArchived: false,
    createdAt: '2024-01-01T00:00:00Z',
    labels: [],
    checklists: []
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

  beforeEach(async () => {
    stateServiceMock = {
      projects: signal<ProjectSummary[]>([]),
      selectedProject: signal<Project | null>(null),
      selectedTask: signal<TaskItem | null>(null),
      columns: signal<Column[]>([]),
      loading: signal(false),
      error: signal<string | null>(null),
      loadProjects: vi.fn(),
      loadProjectById: vi.fn(),
      createColumn: vi.fn(),
      deleteColumn: vi.fn(),
      createTask: vi.fn(),
      loadTaskById: vi.fn(),
      clearSelectedProject: vi.fn(),
      clearError: vi.fn(),
      moveTask: vi.fn(),
      reorderTasks: vi.fn()
    };

    await TestBed.configureTestingModule({
      imports: [BoardComponent],
      providers: [
        provideRouter([]),
        { provide: KanbanStateService, useValue: stateServiceMock },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: {
              paramMap: convertToParamMap({ id: 'project-1' })
            }
          }
        }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(BoardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should load project on init', () => {
    expect(stateServiceMock.loadProjectById).toHaveBeenCalledWith('project-1');
  });

  it('should clear selected project on destroy', () => {
    component.ngOnDestroy();
    expect(stateServiceMock.clearSelectedProject).toHaveBeenCalled();
  });

  it('should show loading state when loading', () => {
    (stateServiceMock.loading as any).set(true);
    fixture.detectChanges();

    const loadingState = fixture.nativeElement.querySelector('.loading-state');
    expect(loadingState).toBeTruthy();
  });

  it('should show error state when project not found', () => {
    (stateServiceMock.loading as any).set(false);
    fixture.detectChanges();

    const errorState = fixture.nativeElement.querySelector('.error-state');
    expect(errorState).toBeTruthy();
    expect(errorState.textContent).toContain('Project not found');
  });

  it('should display project name in header', () => {
    (stateServiceMock.selectedProject as any).set(mockProject);
    (stateServiceMock.columns as any).set(mockProject.columns);
    fixture.detectChanges();

    const title = fixture.nativeElement.querySelector('.board-title');
    expect(title.textContent).toContain('Test Project');
  });

  it('should display columns when project is loaded', () => {
    (stateServiceMock.selectedProject as any).set(mockProject);
    (stateServiceMock.columns as any).set(mockProject.columns);
    fixture.detectChanges();

    const columns = fixture.nativeElement.querySelectorAll('.column');
    expect(columns.length).toBe(1);

    const columnTitle = fixture.nativeElement.querySelector('.column-title');
    expect(columnTitle.textContent).toContain('To Do');
  });

  it('should display tasks in columns', () => {
    (stateServiceMock.selectedProject as any).set(mockProject);
    (stateServiceMock.columns as any).set(mockProject.columns);
    fixture.detectChanges();

    const taskCard = fixture.nativeElement.querySelector('.task-card');
    expect(taskCard).toBeTruthy();

    const taskTitle = fixture.nativeElement.querySelector('.task-title');
    expect(taskTitle.textContent).toBe('Test Task');
  });

  it('should show add column form when button clicked', () => {
    (stateServiceMock.selectedProject as any).set(mockProject);
    (stateServiceMock.columns as any).set(mockProject.columns);
    fixture.detectChanges();

    component.showAddColumnForm();
    fixture.detectChanges();

    expect(component.showColumnForm()).toBe(true);
    const addColumnForm = fixture.nativeElement.querySelector('.add-column-form');
    expect(addColumnForm).toBeTruthy();
  });

  it('should call createColumn when form is submitted', () => {
    (stateServiceMock.selectedProject as any).set(mockProject);
    (stateServiceMock.columns as any).set(mockProject.columns);
    fixture.detectChanges();

    component.showAddColumnForm();
    component.newColumnName = 'New Column';
    component.createColumn();

    expect(stateServiceMock.createColumn).toHaveBeenCalledWith('project-1', { name: 'New Column' });
    expect(component.showColumnForm()).toBe(false);
  });

  it('should show task form when add task button clicked', () => {
    (stateServiceMock.selectedProject as any).set(mockProject);
    (stateServiceMock.columns as any).set(mockProject.columns);
    fixture.detectChanges();

    component.showAddTaskForm('column-1');
    fixture.detectChanges();

    expect(component.addingTaskToColumn()).toBe('column-1');
    const taskForm = fixture.nativeElement.querySelector('.task-form');
    expect(taskForm).toBeTruthy();
  });

  it('should call createTask when task form is submitted', () => {
    (stateServiceMock.selectedProject as any).set(mockProject);
    (stateServiceMock.columns as any).set(mockProject.columns);
    fixture.detectChanges();

    component.showAddTaskForm('column-1');
    component.newTaskTitle = 'New Task';
    component.createTask('column-1');

    expect(stateServiceMock.createTask).toHaveBeenCalledWith('column-1', {
      title: 'New Task',
      priority: Priority.Medium
    });
    expect(component.addingTaskToColumn()).toBeNull();
  });

  it('should call deleteColumn when delete is confirmed', () => {
    vi.spyOn(window, 'confirm').mockReturnValue(true);

    component.deleteColumn('column-1');

    expect(stateServiceMock.deleteColumn).toHaveBeenCalledWith('column-1');
  });

  it('should not call deleteColumn when delete is cancelled', () => {
    vi.spyOn(window, 'confirm').mockReturnValue(false);

    component.deleteColumn('column-1');

    expect(stateServiceMock.deleteColumn).not.toHaveBeenCalled();
  });

  it('should call loadTaskById when task card is clicked', () => {
    (stateServiceMock.selectedProject as any).set(mockProject);
    (stateServiceMock.columns as any).set(mockProject.columns);
    fixture.detectChanges();

    component.openTaskDetail('task-1');

    expect(stateServiceMock.loadTaskById).toHaveBeenCalledWith('task-1');
  });

  it('should return correct priority label', () => {
    expect(component.getPriorityLabel(Priority.Low)).toBe('Low');
    expect(component.getPriorityLabel(Priority.Medium)).toBe('Medium');
    expect(component.getPriorityLabel(Priority.High)).toBe('High');
    expect(component.getPriorityLabel(Priority.Critical)).toBe('Critical');
  });

  it('should detect overdue tasks', () => {
    const pastDate = new Date();
    pastDate.setDate(pastDate.getDate() - 1);
    expect(component.isOverdue(pastDate.toISOString())).toBe(true);

    const futureDate = new Date();
    futureDate.setDate(futureDate.getDate() + 1);
    expect(component.isOverdue(futureDate.toISOString())).toBe(false);
  });

  it('should count completed checklists', () => {
    const task = {
      checklists: [
        { isCompleted: true },
        { isCompleted: false },
        { isCompleted: true }
      ]
    };
    expect(component.getCompletedChecklists(task)).toBe(2);
  });

  it('should show empty board message when no columns', () => {
    (stateServiceMock.selectedProject as any).set({ ...mockProject, columns: [] });
    (stateServiceMock.columns as any).set([]);
    fixture.detectChanges();

    const emptyBoard = fixture.nativeElement.querySelector('.empty-board');
    expect(emptyBoard).toBeTruthy();
    expect(emptyBoard.textContent).toContain('No columns yet');
  });

  // Drag & Drop Tests (TASK-03-206)
  describe('Drag & Drop Operations', () => {
    const mockTask2: TaskItem = {
      id: 'task-2',
      title: 'Test Task 2',
      description: undefined,
      priority: Priority.High,
      order: 1,
      isArchived: false,
      createdAt: '2024-01-02T00:00:00Z',
      labels: [],
      checklists: []
    };

    const mockArchivedTask: TaskItem = {
      id: 'task-3',
      title: 'Archived Task',
      description: undefined,
      priority: Priority.Low,
      order: 2,
      isArchived: true,
      createdAt: '2024-01-03T00:00:00Z',
      labels: [],
      checklists: []
    };

    const mockColumn2: Column = {
      id: 'column-2',
      name: 'In Progress',
      order: 1,
      tasks: []
    };

    it('should filter out archived tasks from active tasks', () => {
      const columnWithArchived: Column = {
        ...mockColumn,
        tasks: [mockTask, mockTask2, mockArchivedTask]
      };

      (stateServiceMock.selectedProject as any).set({
        ...mockProject,
        columns: [columnWithArchived]
      });
      (stateServiceMock.columns as any).set([columnWithArchived]);
      fixture.detectChanges();

      const activeTasks = component.getActiveTasks(columnWithArchived);
      expect(activeTasks.length).toBe(2);
      expect(activeTasks.map(t => t.id)).toEqual(['task-1', 'task-2']);
    });

    it('should return correct connected drop lists', () => {
      (stateServiceMock.selectedProject as any).set({
        ...mockProject,
        columns: [mockColumn, mockColumn2]
      });
      (stateServiceMock.columns as any).set([mockColumn, mockColumn2]);
      fixture.detectChanges();

      const connectedLists = component.getConnectedDropLists();
      expect(connectedLists).toEqual(['column-column-1', 'column-column-2']);
    });

    it('should call reorderTasks when dropping task in same column', () => {
      (stateServiceMock.selectedProject as any).set({
        ...mockProject,
        columns: [{ ...mockColumn, tasks: [mockTask, mockTask2] }]
      });
      (stateServiceMock.columns as any).set([{ ...mockColumn, tasks: [mockTask, mockTask2] }]);
      fixture.detectChanges();

      // Same container reference for same column reorder
      const sameContainer = { id: 'column-column-1', data: [mockTask, mockTask2] };
      const mockDropEvent = {
        previousContainer: sameContainer,
        container: sameContainer,
        previousIndex: 0,
        currentIndex: 1,
        item: { data: mockTask }
      } as unknown as CdkDragDrop<TaskItem[]>;

      component.onTaskDrop(mockDropEvent, 'column-1');

      expect(stateServiceMock.reorderTasks).toHaveBeenCalledWith('column-1', ['task-2', 'task-1']);
    });

    it('should call moveTask when dropping task in different column', () => {
      (stateServiceMock.selectedProject as any).set({
        ...mockProject,
        columns: [mockColumn, mockColumn2]
      });
      (stateServiceMock.columns as any).set([mockColumn, mockColumn2]);
      fixture.detectChanges();

      const sourceContainer = { id: 'column-column-1', data: [mockTask] };
      const targetContainer = { id: 'column-column-2', data: [] };

      const mockDropEvent = {
        previousContainer: sourceContainer,
        container: targetContainer,
        previousIndex: 0,
        currentIndex: 0,
        item: { data: mockTask }
      } as unknown as CdkDragDrop<TaskItem[]>;

      component.onTaskDrop(mockDropEvent, 'column-2');

      expect(stateServiceMock.moveTask).toHaveBeenCalledWith('task-1', 'column-2', 0);
    });

    it('should not call reorderTasks when moving task to different column', () => {
      const sourceContainer = { id: 'column-column-1', data: [mockTask] };
      const targetContainer = { id: 'column-column-2', data: [] };

      const mockDropEvent = {
        previousContainer: sourceContainer,
        container: targetContainer,
        previousIndex: 0,
        currentIndex: 0,
        item: { data: mockTask }
      } as unknown as CdkDragDrop<TaskItem[]>;

      component.onTaskDrop(mockDropEvent, 'column-2');

      expect(stateServiceMock.reorderTasks).not.toHaveBeenCalled();
    });

    it('should render drop list elements with correct IDs', () => {
      (stateServiceMock.selectedProject as any).set({
        ...mockProject,
        columns: [mockColumn, mockColumn2]
      });
      (stateServiceMock.columns as any).set([mockColumn, mockColumn2]);
      fixture.detectChanges();

      const dropLists = fixture.nativeElement.querySelectorAll('[cdkDropList]');
      expect(dropLists.length).toBe(2);
      expect(dropLists[0].id).toBe('column-column-1');
      expect(dropLists[1].id).toBe('column-column-2');
    });

    it('should render task cards with cdkDrag directive', () => {
      (stateServiceMock.selectedProject as any).set(mockProject);
      (stateServiceMock.columns as any).set(mockProject.columns);
      fixture.detectChanges();

      const taskCards = fixture.nativeElement.querySelectorAll('.task-card');
      expect(taskCards.length).toBe(1);
      // Task cards should be draggable
      expect(taskCards[0].hasAttribute('cdkdrag') || taskCards[0].classList.contains('cdk-drag')).toBe(true);
    });
  });
});
